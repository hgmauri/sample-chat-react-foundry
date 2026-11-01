using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;

namespace Chat.Api.Chat;

public sealed record MensagemDto(string Papel, string Texto);

public sealed record ChatRequest(IReadOnlyList<MensagemDto>? Mensagens)
{
    public const int MaximoMensagens = 20;
    public const int MaximoCaracteres = 4000;

    public Dictionary<string, string[]>? Validar()
    {
        if (Mensagens is null || Mensagens.Count == 0)
            return Erro("mensagens", "Envie ao menos uma mensagem.");

        if (Mensagens[^1].Papel != "user")
            return Erro("mensagens", "A última mensagem precisa ser do usuário.");

        if (Mensagens.Any(m => m.Papel is not ("user" or "assistant")))
            return Erro("mensagens", "Papel inválido. Use apenas 'user' ou 'assistant'.");

        if (Mensagens.Any(m => string.IsNullOrWhiteSpace(m.Texto) || m.Texto.Length > MaximoCaracteres))
            return Erro("mensagens", $"Cada mensagem deve ter entre 1 e {MaximoCaracteres} caracteres.");

        return null;
    }

    /// <summary>
    /// O prompt de sistema é sempre do servidor. O cliente só manda a conversa,
    /// e só as últimas mensagens entram no contexto (custo e latência sob controle).
    /// </summary>
    public List<ChatMessage> ParaConversa(string promptDoSistema)
    {
        var conversa = new List<ChatMessage> { new(ChatRole.System, promptDoSistema) };

        conversa.AddRange(Mensagens!
            .TakeLast(MaximoMensagens)
            .Select(m => new ChatMessage(m.Papel == "user" ? ChatRole.User : ChatRole.Assistant, m.Texto)));

        return conversa;
    }

    private static Dictionary<string, string[]> Erro(string campo, string mensagem) => new() { [campo] = [mensagem] };
}

/// <summary>Evento enviado ao front via Server-Sent Events.</summary>
public sealed record EventoChat(
    string Tipo,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Texto = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] long? Tokens = null)
{
    public static EventoChat Trecho(string texto) => new("texto", texto);
    public static EventoChat Uso(long? tokens) => new("uso", Tokens: tokens);
    public static EventoChat Falha(string mensagem) => new("erro", mensagem);
}
