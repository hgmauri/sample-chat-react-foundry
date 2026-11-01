using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

namespace Chat.Api.Chat;

public static class ChatEndpoints
{
    public static void MapChat(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/chat", Conversar)
            .RequireRateLimiting("chat");
    }

    private static Results<ValidationProblem, ServerSentEventsResult<EventoChat>> Conversar(
        ChatRequest request,
        IChatClient chat,
        IOptions<FoundryOptions> opcoes,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        if (request.Validar() is { } erros)
            return TypedResults.ValidationProblem(erros);

        var conversa = request.ParaConversa(PromptDoSistema.Texto);
        var chatOptions = new ChatOptions { MaxOutputTokens = opcoes.Value.MaxTokensResposta };
        var logger = loggerFactory.CreateLogger("Chat");

        return TypedResults.ServerSentEvents(Transmitir(chat, conversa, chatOptions, logger, cancellationToken));
    }

    private static async IAsyncEnumerable<EventoChat> Transmitir(
        IChatClient chat,
        List<ChatMessage> conversa,
        ChatOptions chatOptions,
        ILogger logger,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using var atualizacoes = chat
            .GetStreamingResponseAsync(conversa, chatOptions, cancellationToken)
            .GetAsyncEnumerator(cancellationToken);

        long? tokens = null;

        while (true)
        {
            EventoChat? evento = null;
            var falhou = false;

            try
            {
                if (!await atualizacoes.MoveNextAsync())
                    break;

                var atualizacao = atualizacoes.Current;

                if (atualizacao.Contents.OfType<UsageContent>().FirstOrDefault() is { } uso)
                    tokens = uso.Details.TotalTokenCount;

                if (!string.IsNullOrEmpty(atualizacao.Text))
                    evento = EventoChat.Trecho(atualizacao.Text);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // O detalhe vai para o log. O usuário recebe uma mensagem que não expõe nada interno.
                logger.LogError(ex, "Falha ao gerar resposta do chat");
                falhou = true;
            }

            if (falhou)
            {
                yield return EventoChat.Falha("Não consegui responder agora. Tente novamente em instantes.");
                yield break;
            }

            if (evento is not null)
                yield return evento;
        }

        yield return EventoChat.Uso(tokens);
    }
}
