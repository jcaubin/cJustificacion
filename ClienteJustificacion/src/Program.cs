using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using CsvHelper;
using PowerArgs;
using System.IO;
using Aeseg.ProxyJustificacion;

namespace ClienteJustificacion
{
    class Program
    {
        private static Random random = new Random();
        private static Logger _loger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            try
            {
                var clientArgs = Args.Parse<ClientArgs>(args);
                var parseErrors = new List<ParseResult>(); //Listado de errores de parseo

                Console.WriteLine("Proceso del fichero: {0}; {1}; {2}; {3};", clientArgs.User, "*****", clientArgs.FileType, clientArgs.File);
                _loger.Info("Proceso del fichero: {0}; {1}; {2}; {3};", clientArgs.User, "*****", clientArgs.FileType, clientArgs.File);

                //Carga y parseo del fichero de datos
                var csv = new CsvReader(new StreamReader(clientArgs.File));
                csv.Configuration.Delimiter = ";";
                csv.Configuration.RegisterClassMap<JbsInterchageModelCsvDocMap>();
                csv.Configuration.IgnoreReadingExceptions = true;
                csv.Configuration.ReadingExceptionCallback = (ex, row) =>
                {
                    int rownumber = row.Row;
                    string message = ex.Message + " " + ex.Data["CsvHelper"];
                    parseErrors.Add(new ParseResult() { Id = rownumber, Descripcion = message });
                };
                var records = csv.GetRecords<JbsInterchageModel>().ToList();

                if (parseErrors.Count > 0)
                {
                    Console.WriteLine("Se ha encontrado errores en el fichero de entrada, corrijalos antes de continuar.");
                    _loger.Error("Se ha encontrado errores en el fichero de entrada, corrijalos antes de continuar.");
                    foreach (var error in parseErrors)
                    {
                        Console.WriteLine("Linea: {0}; Error {1};", error.Id, error.Descripcion);
                        _loger.Error("Linea: {0}; Error {1};", error.Id, error.Descripcion);
                    }
                    return;
                }

                using (JustificationClient client = new JustificationClient())
                {
                    client.ClientCredentials.UserName.UserName = clientArgs.User;
                    client.ClientCredentials.UserName.Password = clientArgs.Password;
                    var resultadosEnvio = new List<LoadResult>();
                    foreach (var item in records)
                    {
                        Console.Write("Envio: {0}, {1}.", item.Expediente, item.NumeroFactura);
                        if (!string.IsNullOrWhiteSpace(item.NombreFichero))
                        {
                            item.Fichero = File.ReadAllBytes(item.NombreFichero);
                            item.NombreFichero = Path.GetFileName(item.NombreFichero);
                        }
                        LoadResult r = client.LoadJbs(item);
                        resultadosEnvio.Add(r);
                        Console.WriteLine(" Resultado: {0};", r.ResultadoCarga);
                        _loger.Info("Envio: {0}, {1}. Resultado {2}", item.Expediente, item.NumeroFactura, r.ResultadoCarga);
                        if (r.ResultadoCarga == ResultadoCarga.Erroneo)
                        {
                            foreach (var error in r.Errores)
                            {
                                _loger.Warn("Envio: {0}, {1}. Error {2} - {3}", item.Expediente, item.NumeroFactura, error.Nombre, error.Descripcion);
                            }
                        }
                    }
                }
            }
            catch (ArgException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ArgUsage.GenerateUsageFromTemplate<ClientArgs>());
            }
            catch (Exception e)
            {
                _loger.Error(e, "error!");
                Console.WriteLine("Error." + e.Message);
            }
        }
    }

}
