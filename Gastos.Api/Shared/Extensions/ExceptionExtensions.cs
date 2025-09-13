namespace Gastos.Api.Shared.Extensions;

public static class ExceptionExtensions
{
    /// <summary>
    /// Indica si la excepción (o alguna inner) es por restricción de integridad referencial o de clave foránea.
    /// </summary>
    public static bool IsIntegrityConstraintViolation(this Exception ex)
    {
        while (ex != null)
        {
            var msg = ex.Message;
            if (msg.Contains("FOREIGN KEY", StringComparison.OrdinalIgnoreCase) ||
                msg.Contains("constraint", StringComparison.OrdinalIgnoreCase) ||
                msg.Contains("REFERENCE", StringComparison.OrdinalIgnoreCase) ||
                msg.Contains("violates foreign key", StringComparison.OrdinalIgnoreCase) ||
                msg.Contains("integrity constraint", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            ex = ex.InnerException!;
        }
        return false;
    }
}
