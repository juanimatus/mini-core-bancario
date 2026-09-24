namespace Banco.Negocio;

/// <summary>Los datos de la transferencia no cumplen las reglas (monto, cuentas iguales, etc.).</summary>
public class TransferenciaInvalidaException : Exception
{
    public TransferenciaInvalidaException(string mensaje) : base(mensaje)
    {
    }
}
