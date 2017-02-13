using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aeseg.ProxyJustificacion;
using CsvHelper.Configuration;

namespace ClienteJustificacion
{
    public class JpersonalInterchageModelCsvMap : CsvClassMap<JpersonalInterchageModel>
    {
        public JpersonalInterchageModelCsvMap()
        {
            Map(m => m.Expediente).Name("ID_EXPEDIENTE");
            Map(m => m.IdPartida).Name("ID_PARTIDA");
            Map(m => m.idConcepto).Name("ID_CONCEPTO");
            Map(m => m.idTipoJustificante).Name("TIPO_JUSTIFICANTE");
            Map(m => m.ImporteTotal).Name("IMPORTE_TOTAL");
            Map(m => m.ImporteImputado).Name("IMPORTE_IMPUTADO");
            Map(m => m.FechaPago).Name("FECHA_PAGO");
            Map(m => m.Anualidad).Name("ANUALIDAD");
            Map(m => m.Observaciones).Name("OBSERVACIONES");
            Map(m => m.NombreFichero).Name("DOC").TypeConverter<FileTypeConverter>();

        }
    }
}
