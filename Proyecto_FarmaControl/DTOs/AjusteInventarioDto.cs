
using System;
using System.Collections.Generic;

namespace Proyecto_FarmaControl.DTOs
{
    public class AjusteInventarioDto
    {
        // IDENTIFICADOR DEL AJUSTE

        public int Id { get; set; }

        // ALMACÉN

        public int AlmacenId { get; set; }

        public string AlmacenNombre { get; set; }
            = string.Empty;

        // USUARIO QUE REGISTRA EL AJUSTE

        public int UsuarioId { get; set; }

        public string UsuarioNombre { get; set; }
            = string.Empty;

        // USUARIO QUE CONFIRMA EL AJUSTE

        public int? ConfirmadoPorUsuarioId { get; set; }

        public string? ConfirmadoPorUsuarioNombre
        { get; set; }

        // FECHAS

        public DateTime FechaAjuste { get; set; }

        public DateTime? FechaConfirmacion
        { get; set; }

        // TIPO DE AJUSTE: Conteo o Correccion

        public string TipoAjuste { get; set; }
            = string.Empty;

        // ESTADO: Borrador, Confirmado o Anulado

        public string Estado { get; set; }
            = string.Empty;

        // MOTIVO

        public string Motivo { get; set; }
            = string.Empty;

        // OBSERVACIONES

        public string? Observaciones { get; set; }

        // MOVIMIENTO DE INVENTARIO RELACIONADO

        public int? MovimientoInventarioId
        { get; set; }

        // DETALLES DEL CONTEO FÍSICO

        public List<DetalleAjusteInventarioDto> Detalles
        { get; set; } = new();
    }
}
