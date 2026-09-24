namespace Banco.Negocio;

public class CuentaNoEncontradaException : Exception
{
    public int CuentaId { get; }

    public CuentaNoEncontradaException(int cuentaId)
        : base($"No existe la cuenta {cuentaId}.")
    {
        CuentaId = cuentaId;
    }
}
