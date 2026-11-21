
namespace Proyecto_FarmaControl.DTOs
{
    public class DetalleAjusteInventarioDto
    {
        // IDENTIFICADOR DEL DETALLE

        public int Id { get; set; }

        // AJUSTE AL QUE PERTENECE

        public int AjusteInventarioId { get; set; }

        // INFORMACIÓN DEL LOTE

        public int LoteId { get; set; }

        public string NumeroLote { get; set; }
            = string.Empty;

        // INFORMACIÓN DEL MEDICAMENTO

        public int MedicamentoId { get; set; }

        public string MedicamentoNombre { get; set; }
            = string.Empty;

        // EXISTENCIAS AL REGISTRAR EL CONTEO

        public int ExistenciaSistema { get; set; }

        // CANTIDAD CONTADA FÍSICAMENTE

        public int CantidadFisica { get; set; }

        // DIFERENCIA CALCULADA
        // CantidadFisica - ExistenciaSistema

        public int Diferencia { get; set; }

        // OBSERVACIÓN DEL DETALLE

        public string? Observacion { get; set; }
    }
}
