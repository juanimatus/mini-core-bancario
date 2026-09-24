using Banco.Api;
using Banco.Datos;
using Banco.Negocio;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("Banco")
    ?? throw new InvalidOperationException("Falta la cadena de conexion 'Banco'.");
builder.Services.AddSingleton<ICuentaRepositorio>(new CuentaRepositorio(connectionString));
builder.Services.AddSingleton<ITransferenciaRepositorio>(new TransferenciaRepositorio(connectionString));
builder.Services.AddSingleton<TransferenciaServicio>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Ok(new { estado = "ok" }));

app.MapGet("/cuentas/{id:int}/saldo", async (int id, ICuentaRepositorio repositorio, CancellationToken ct) =>
{
    var cuenta = await repositorio.ObtenerPorIdAsync(id, ct);
    return cuenta is null
        ? Results.NotFound(new { error = $"No existe la cuenta {id}." })
        : Results.Ok(new { cuenta.Id, cuenta.Titular, cuenta.Saldo });
});

app.MapPost("/transferencias", async (TransferenciaRequest pedido, TransferenciaServicio servicio, CancellationToken ct) =>
{
    try
    {
        var resultado = await servicio.TransferirAsync(
            pedido.OrigenId, pedido.DestinoId, pedido.Monto, pedido.Descripcion, ct);
        return Results.Ok(resultado);
    }
    catch (TransferenciaInvalidaException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (CuentaNoEncontradaException ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
    catch (SaldoInsuficienteException ex)
    {
        return Results.UnprocessableEntity(new { error = ex.Message });
    }
});

app.Run();
