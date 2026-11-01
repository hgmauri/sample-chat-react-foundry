namespace Chat.Api.Chat;

public sealed class FoundryOptions
{
    /// <summary>Endpoint do recurso no Foundry. Ex: https://meu-recurso.openai.azure.com/</summary>
    public string? Endpoint { get; set; }

    /// <summary>Nome do deployment do modelo no Foundry.</summary>
    public string Deployment { get; set; } = "gpt-5.4-mini";

    /// <summary>Teto de tokens por resposta. Em modelos de raciocínio, inclui os tokens de raciocínio.</summary>
    public int MaxTokensResposta { get; set; } = 4096;
}
