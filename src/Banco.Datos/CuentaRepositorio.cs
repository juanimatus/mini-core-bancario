using Banco.Negocio;
using Microsoft.Data.SqlClient;

namespace Banco.Datos;

/// <summary>
/// Implementacion con ADO.NET puro. Las consultas son parametrizadas: el valor nunca se
/// concatena al texto SQL, lo que evita SQL injection.
/// </summary>
public class CuentaRepositorio : ICuentaRepositorio
{
    private readonly string _connectionString;

    public CuentaRepositorio(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<Cuenta?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT Id, Titular, Saldo FROM dbo.Cuentas WHERE Id = @Id";

        await using var conexion = new SqlConnection(_connectionString);
        await conexion.OpenAsync(cancellationToken);

        await using var comando = new SqlCommand(sql, conexion);
        comando.Parameters.AddWithValue("@Id", id);

        await using var lector = await comando.ExecuteReaderAsync(cancellationToken);
        if (!await lector.ReadAsync(cancellationToken))
            return null;

        return new Cuenta(
            id: lector.GetInt32(0),
            titular: lector.GetString(1),
            saldoInicial: lector.GetDecimal(2));
    }
}
