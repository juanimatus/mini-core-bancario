namespace Banco.Negocio;

/// <summary>
/// Cuenta bancaria simple. El saldo se maneja con decimal (nunca double) para evitar
/// errores de redondeo con dinero, y nunca puede quedar negativo.
/// </summary>
public class Cuenta
{
    public int Id { get; }
    public string Titular { get; }
    public decimal Saldo { get; private set; }

    public Cuenta(int id, string titular, decimal saldoInicial = 0m)
    {
        if (string.IsNullOrWhiteSpace(titular))
            throw new ArgumentException("El titular es obligatorio.", nameof(titular));
        if (saldoInicial < 0m)
            throw new ArgumentOutOfRangeException(nameof(saldoInicial), "El saldo inicial no puede ser negativo.");

        Id = id;
        Titular = titular;
        Saldo = saldoInicial;
    }

    public void Depositar(decimal monto)
    {
        ValidarMonto(monto);
        Saldo += monto;
    }

    public void Retirar(decimal monto)
    {
        ValidarMonto(monto);
        if (monto > Saldo)
            throw new SaldoInsuficienteException(Saldo, monto);
        Saldo -= monto;
    }

    private static void ValidarMonto(decimal monto)
    {
        if (monto <= 0m)
            throw new ArgumentOutOfRangeException(nameof(monto), "El monto debe ser mayor a cero.");
    }
}
