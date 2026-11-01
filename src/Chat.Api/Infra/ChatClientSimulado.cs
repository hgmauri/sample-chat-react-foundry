using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;

namespace Chat.Api.Infra;

/// <summary>
/// Usado quando o endpoint do Foundry não está configurado.
/// Permite rodar e testar o front e a API sem Azure e sem custo.
/// </summary>
public sealed class ChatClientSimulado : IChatClient
{
    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) =>
        Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, Responder(messages))));

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var resposta = Responder(messages);

        foreach (var palavra in resposta.Split(' '))
        {
            await Task.Delay(25, cancellationToken);
            yield return new ChatResponseUpdate(ChatRole.Assistant, palavra + " ");
        }

        yield return new ChatResponseUpdate
        {
            Role = ChatRole.Assistant,
            Contents = [new UsageContent(new UsageDetails { TotalTokenCount = resposta.Length / 4 })]
        };
    }

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public void Dispose() { }

    private static string Responder(IEnumerable<ChatMessage> messages)
    {
        var lista = messages.ToList();
        var pergunta = lista.Last(m => m.Role == ChatRole.User).Text;
        var turnos = lista.Count(m => m.Role == ChatRole.User);

        return $"**Modo simulado** (configure `Foundry:Endpoint` para usar o modelo de verdade). " +
               $"Você perguntou: \"{pergunta}\". Esta conversa tem {turnos} pergunta(s) no contexto.";
    }
}
