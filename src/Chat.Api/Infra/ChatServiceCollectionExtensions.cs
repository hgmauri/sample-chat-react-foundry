using Chat.Api.Chat;
using Microsoft.Extensions.AI;

namespace Chat.Api.Infra;

public static class ChatServiceCollectionExtensions
{
    public static IServiceCollection AddChatInteligente(this IServiceCollection services, IConfiguration configuration)
    {
        var secao = configuration.GetSection("Foundry");
        services.Configure<FoundryOptions>(secao);

        var opcoes = secao.Get<FoundryOptions>() ?? new FoundryOptions();

        IChatClient cliente = string.IsNullOrWhiteSpace(opcoes.Endpoint)
            ? new ChatClientSimulado()
            : FoundryChatClientFactory.Criar(opcoes);

        // Pipeline do Microsoft.Extensions.AI: dá para empilhar log, cache, telemetria e rate limit
        services.AddChatClient(cliente)
            .UseLogging();

        return services;
    }
}
