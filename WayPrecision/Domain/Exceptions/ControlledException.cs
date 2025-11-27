using System;

namespace WayPrecision.Domain.Exceptions
{
    /// <summary>
    /// Excepción controlada usada para representar errores esperados dentro del dominio
    /// donde se desea transportar información adicional (código, referencia, etc.).
    /// Hereda de <see cref="Exception"/> para integrarse con el manejo estándar.
    /// </summary>
    public class ControlledException : Exception
    {
        /// <summary>
        /// Código numérico opcional que identifica el tipo de error dentro del dominio.
        /// </summary>
        public int? ErrorCode { get; }

        /// <summary>
        /// Identificador de referencia opcional para rastrear o correlacionar la excepción
        /// (por ejemplo, un id de operación o request).
        /// </summary>
        public string? Reference { get; }

        /// <summary>
        /// Indica si la excepción corresponde a un error controlado/esperado en la lógica de la aplicación.
        /// </summary>
        public bool IsControlled { get; } = true;

        public ControlledException(string message)
            : base(message)
        {
        }

        public override string ToString()
        {
            var baseText = base.ToString();

            if (!ErrorCode.HasValue && string.IsNullOrWhiteSpace(Reference))
                return baseText;

            return $"{baseText} (ErrorCode: {ErrorCode?.ToString() ?? "-"}, Reference: {Reference ?? "-"})";
        }
    }
}