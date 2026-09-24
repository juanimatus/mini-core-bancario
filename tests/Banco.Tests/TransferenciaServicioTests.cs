using Banco.Negocio;

namespace Banco.Tests;

public class TransferenciaServicioTests
{
    // Repositorio falso: registra si lo llamaron, sin tocar ninguna base.
    private class RepositorioFalso : ITransferenciaRepositorio
    {
        public int Llamadas { get; private set; }

        public Task<ResultadoTransferencia> TransferirAsync(
            int origenId, int destinoId, decimal monto, string? descripcion, CancellationToken cancellationToken = default)
        {
            Llamadas++;
            return Task.FromResult(new ResultadoTransferencia(0m, 0m));
        }
    }

    [Fact]
    public async Task Transferir_DatosValidos_DelegaEnElRepositorio()
    {
        var repo = new RepositorioFalso();
        var servicio = new TransferenciaServicio(repo);

        await servicio.TransferirAsync(1, 2, 100.50m, "alquiler");

        Assert.Equal(1, repo.Llamadas);
    }

    [Fact]
    public async Task Transferir_MismaCuenta_Lanza_YNoLlegaALaBase()
    {
        var repo = new RepositorioFalso();
        var servicio = new TransferenciaServicio(repo);

        await Assert.ThrowsAsync<TransferenciaInvalidaException>(() => servicio.TransferirAsync(1, 1, 10m, null));

        Assert.Equal(0, repo.Llamadas);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public async Task Transferir_MontoNoPositivo_Lanza(decimal monto)
    {
        var repo = new RepositorioFalso();
        var servicio = new TransferenciaServicio(repo);

        await Assert.ThrowsAsync<TransferenciaInvalidaException>(() => servicio.TransferirAsync(1, 2, monto, null));

        Assert.Equal(0, repo.Llamadas);
    }

    [Fact]
    public async Task Transferir_MasDeDosDecimales_Lanza()
    {
        var servicio = new TransferenciaServicio(new RepositorioFalso());

        await Assert.ThrowsAsync<TransferenciaInvalidaException>(() => servicio.TransferirAsync(1, 2, 10.005m, null));
    }

    [Fact]
    public async Task Transferir_DescripcionMuyLarga_Lanza()
    {
        var servicio = new TransferenciaServicio(new RepositorioFalso());

        await Assert.ThrowsAsync<TransferenciaInvalidaException>(
            () => servicio.TransferirAsync(1, 2, 10m, new string('x', 201)));
    }
}
