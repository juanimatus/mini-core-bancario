namespace Banco.Api;

public record TransferenciaRequest(int OrigenId, int DestinoId, decimal Monto, string? Descripcion);
