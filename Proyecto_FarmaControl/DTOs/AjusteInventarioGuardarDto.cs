
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Proyecto_FarmaControl.DTOs
{
    public class AjusteInventarioGuardarDto
    {
        // ALMACÉN DONDE SE REALIZA EL CONTEO

        [Range(1, int.MaxValue,
            ErrorMessage = "Seleccione un almacén válido.")]
        public int AlmacenId { get; set; }

        // TIPO DE AJUSTE: Conteo o Correccion

        [Required(ErrorMessage =
            "El tipo de ajuste es obligatorio.")]
        [StringLength(30)]
        [RegularExpression("^(Conteo|Correccion)$",
            ErrorMessage = "El tipo de ajuste no es válido.")]
        public string TipoAjuste { get; set; } = "Conteo";

        // FECHA DEL AJUSTE

        public DateTime FechaAjuste { get; set; }
            = Proyecto_FarmaControl.Utilidades.FechaNicaragua.Hoy;

        // MOTIVO DEL AJUSTE

        [Required(ErrorMessage =
            "El motivo del ajuste es obligatorio.")]
        [StringLength(150,
            ErrorMessage = "El motivo no puede superar 150 caracteres.")]
        public string Motivo { get; set; } = string.Empty;

        // OBSERVACIONES GENERALES

        [StringLength(250,
            ErrorMessage = "Las observaciones no pueden superar 250 caracteres.")]
        public string? Observaciones { get; set; }

        // DETALLES DEL CONTEO FÍSICO

        [Required(ErrorMessage =
            "Debe registrar los detalles del ajuste.")]
        [MinLength(1,
            ErrorMessage = "Debe registrar al menos un lote.")]
        public List<DetalleAjusteInventarioGuardarDto> Detalles
        { get; set; } = new();
    }
}
