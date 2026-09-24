namespace Banco.Negocio;

/// <summary>
/// Valida los datos de una transferencia antes de llegar a la base. La atomicidad y el control
/// de saldo se resuelven en el repositorio, dentro de una transaccion.
/// </summary>
public class TransferenciaServicio
{
    private readonly ITransferenciaRepositorio _repositorio;

    public TransferenciaServicio(ITransferenciaRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    public Task<ResultadoTransferencia> TransferirAsync(
        int origenId,
        int destinoId,
        decimal monto,
        string? descripcion,
        CancellationToken cancellationToken = default)
    {
        if (origenId == destinoId)
            throw new TransferenciaInvalidaException("La cuenta de origen y la de destino deben ser distintas.");
        if (monto <= 0m)
            throw new TransferenciaInvalidaException("El monto debe ser mayor a cero.");
        if (decimal.Round(monto, 2) != monto)
            throw new TransferenciaInvalidaException("El monto admite como maximo dos decimales.");
        if (descripcion is { Length: > 200 })
            throw new TransferenciaInvalidaException("La descripcion admite como maximo 200 caracteres.");

        return _repositorio.TransferirAsync(origenId, destinoId, monto, descripcion, cancellationToken);
    }
}
