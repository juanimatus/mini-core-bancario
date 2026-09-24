using Banco.Negocio;

namespace Banco.Tests;

public class CuentaTests
{
    [Fact]
    public void Depositar_SumaAlSaldo()
    {
        var cuenta = new Cuenta(1, "Ana", 100m);

        cuenta.Depositar(50m);

        Assert.Equal(150m, cuenta.Saldo);
    }

    [Fact]
    public void Retirar_RestaDelSaldo()
    {
        var cuenta = new Cuenta(1, "Ana", 100m);

        cuenta.Retirar(30m);

        Assert.Equal(70m, cuenta.Saldo);
    }

    [Fact]
    public void Retirar_MasQueElSaldo_LanzaSaldoInsuficiente_YNoModificaElSaldo()
    {
        var cuenta = new Cuenta(1, "Ana", 100m);

        Assert.Throws<SaldoInsuficienteException>(() => cuenta.Retirar(100.01m));

        Assert.Equal(100m, cuenta.Saldo);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Depositar_MontoNoPositivo_Lanza(decimal monto)
    {
        var cuenta = new Cuenta(1, "Ana");

        Assert.Throws<ArgumentOutOfRangeException>(() => cuenta.Depositar(monto));
    }

    [Fact]
    public void Constructor_SaldoInicialNegativo_Lanza()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Cuenta(1, "Ana", -1m));
    }

    [Fact]
    public void Decimal_NoTieneErrorDeRedondeoConCentavos()
    {
        var cuenta = new Cuenta(1, "Ana");

        cuenta.Depositar(0.10m);
        cuenta.Depositar(0.20m);

        Assert.Equal(0.30m, cuenta.Saldo);
    }
}
