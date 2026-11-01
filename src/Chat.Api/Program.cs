using System.Threading.RateLimiting;
using Chat.Api.Chat;
using Chat.Api.Infra;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddChatInteligente(builder.Configuration);
builder.Services.AddProblemDetails();

// Cada mensagem custa tokens. Limite por IP evita abuso e conta surpresa no fim do mês.
builder.Services.AddRateLimiter(opcoes =>
{
    opcoes.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    opcoes.AddPolicy("chat", contexto => RateLimitPartition.GetFixedWindowLimiter(
        contexto.Connection.RemoteIpAddress?.ToString() ?? "anonimo",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1) }));
});

var app = builder.Build();

app.UseRateLimiter();
app.MapChat();

app.Run();
