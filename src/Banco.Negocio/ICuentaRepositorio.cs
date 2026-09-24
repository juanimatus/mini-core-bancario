namespace Banco.Negocio;

/// <summary>
/// Contrato de acceso a datos. Vive en la capa de negocio y la implementa la capa de datos,
/// asi el negocio no depende de SQL Server (inversion de dependencias).
/// </summary>
public interface ICuentaRepositorio
{
    Task<Cuenta?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
}
