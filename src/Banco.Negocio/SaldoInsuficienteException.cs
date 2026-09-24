namespace Banco.Negocio;

public class SaldoInsuficienteException : Exception
{
    public decimal SaldoDisponible { get; }
    public decimal MontoSolicitado { get; }

    public SaldoInsuficienteException(decimal saldoDisponible, decimal montoSolicitado)
        : base($"Saldo insuficiente: disponible {saldoDisponible}, solicitado {montoSolicitado}.")
    {
        SaldoDisponible = saldoDisponible;
        MontoSolicitado = montoSolicitado;
    }
}
