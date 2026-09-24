namespace Banco.Negocio;

public interface ITransferenciaRepositorio
{
    /// <summary>
    /// Mueve el monto de una cuenta a otra de forma atomica: o se aplican el debito, el credito
    /// y los dos movimientos, o no se aplica nada.
    /// </summary>
    /// <exception cref="CuentaNoEncontradaException">Alguna de las cuentas no existe.</exception>
    /// <exception cref="SaldoInsuficienteException">La cuenta de origen no tiene saldo suficiente.</exception>
    Task<ResultadoTransferencia> TransferirAsync(
        int origenId,
        int destinoId,
        decimal monto,
        string? descripcion,
        CancellationToken cancellationToken = default);
}
