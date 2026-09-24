using System.Data;
using Banco.Negocio;
using Microsoft.Data.SqlClient;

namespace Banco.Datos;

/// <summary>
/// Transferencia atomica con una transaccion SQL (ACID). Las reglas de dinero (saldo suficiente,
/// monto positivo) se reutilizan de <see cref="Cuenta"/>; aca solo se coordina la persistencia.
/// </summary>
public class TransferenciaRepositorio : ITransferenciaRepositorio
{
    private readonly string _connectionString;

    public TransferenciaRepositorio(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<ResultadoTransferencia> TransferirAsync(
        int origenId,
        int destinoId,
        decimal monto,
        string? descripcion,
        CancellationToken cancellationToken = default)
    {
        await using var conexion = new SqlConnection(_connectionString);
        await conexion.OpenAsync(cancellationToken);
        await using var transaccion = (SqlTransaction)await conexion.BeginTransactionAsync(cancellationToken);

        try
        {
            // Se bloquean las dos filas SIEMPRE en orden de Id. Si dos transferencias cruzadas
            // (A->B y B->A) tomaran los bloqueos en orden distinto, podrian trabarse entre si (deadlock).
            var cuentas = new Dictionary<int, Cuenta>();
            foreach (var id in new[] { origenId, destinoId }.OrderBy(i => i))
            {
                cuentas[id] = await ObtenerBloqueadaAsync(conexion, transaccion, id, cancellationToken)
                    ?? throw new CuentaNoEncontradaException(id);
            }

            var origen = cuentas[origenId];
            var destino = cuentas[destinoId];

            origen.Retirar(monto);   // lanza SaldoInsuficienteException si no alcanza
            destino.Depositar(monto);

            await ActualizarSaldoAsync(conexion, transaccion, origen, cancellationToken);
            await ActualizarSaldoAsync(conexion, transaccion, destino, cancellationToken);
            await InsertarMovimientoAsync(conexion, transaccion, origen.Id, "TRANSFER_OUT", monto, descripcion, cancellationToken);
            await InsertarMovimientoAsync(conexion, transaccion, destino.Id, "TRANSFER_IN", monto, descripcion, cancellationToken);

            await transaccion.CommitAsync(cancellationToken);
            return new ResultadoTransferencia(origen.Saldo, destino.Saldo);
        }
        catch
        {
            // Cualquier fallo deshace todo: no queda un debito sin su credito.
            await transaccion.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    // UPDLOCK: nadie mas puede modificar la fila hasta que termine la transaccion, asi el saldo
    // que leemos no cambia entre la lectura y la escritura (evita transferencias simultaneas
    // que dejen el saldo mal).
    private static async Task<Cuenta?> ObtenerBloqueadaAsync(
        SqlConnection conexion, SqlTransaction transaccion, int id, CancellationToken ct)
    {
        const string sql = "SELECT Id, Titular, Saldo FROM dbo.Cuentas WITH (UPDLOCK, ROWLOCK) WHERE Id = @Id";

        await using var comando = new SqlCommand(sql, conexion, transaccion);
        comando.Parameters.Add("@Id", SqlDbType.Int).Value = id;

        await using var lector = await comando.ExecuteReaderAsync(ct);
        if (!await lector.ReadAsync(ct))
            return null;

        return new Cuenta(lector.GetInt32(0), lector.GetString(1), lector.GetDecimal(2));
    }

    private static async Task ActualizarSaldoAsync(
        SqlConnection conexion, SqlTransaction transaccion, Cuenta cuenta, CancellationToken ct)
    {
        const string sql = "UPDATE dbo.Cuentas SET Saldo = @Saldo WHERE Id = @Id";

        await using var comando = new SqlCommand(sql, conexion, transaccion);
        comando.Parameters.Add("@Saldo", SqlDbType.Decimal).Value = cuenta.Saldo;
        comando.Parameters["@Saldo"].Precision = 18;
        comando.Parameters["@Saldo"].Scale = 2;
        comando.Parameters.Add("@Id", SqlDbType.Int).Value = cuenta.Id;
        await comando.ExecuteNonQueryAsync(ct);
    }

    private static async Task InsertarMovimientoAsync(
        SqlConnection conexion, SqlTransaction transaccion, int cuentaId, string tipo,
        decimal monto, string? descripcion, CancellationToken ct)
    {
        const string sql = """
            INSERT INTO dbo.Movimientos (CuentaId, Tipo, Monto, Descripcion)
            VALUES (@CuentaId, @Tipo, @Monto, @Descripcion)
            """;

        await using var comando = new SqlCommand(sql, conexion, transaccion);
        comando.Parameters.Add("@CuentaId", SqlDbType.Int).Value = cuentaId;
        comando.Parameters.Add("@Tipo", SqlDbType.VarChar, 12).Value = tipo;
        comando.Parameters.Add("@Monto", SqlDbType.Decimal).Value = monto;
        comando.Parameters["@Monto"].Precision = 18;
        comando.Parameters["@Monto"].Scale = 2;
        comando.Parameters.Add("@Descripcion", SqlDbType.NVarChar, 200).Value = (object?)descripcion ?? DBNull.Value;
        await comando.ExecuteNonQueryAsync(ct);
    }
}
