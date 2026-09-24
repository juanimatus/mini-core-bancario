using Banco.Datos;
using Banco.Negocio;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("Banco")
    ?? throw new InvalidOperationException("Falta la cadena de conexion 'Banco'.");
builder.Services.AddSingleton<ICuentaRepositorio>(new CuentaRepositorio(connectionString));

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

app.Run();
