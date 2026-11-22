using System;

namespace Proyecto_FarmaControl.Utilidades
{
    /// <summary>
    /// Día comercial de la farmacia en Nicaragua. No modifica marcas de tiempo:
    /// pagos, movimientos, confirmaciones y turnos se siguen guardando en UTC.
    /// </summary>
    public static class FechaNicaragua
    {
        // Nicaragua no utiliza horario de verano actualmente (UTC-06:00).
        // Un desfase explícito evita depender de la zona horaria del servidor.
        public static DateTime Hoy => DateTime.UtcNow.AddHours(-6).Date;
    }
}
