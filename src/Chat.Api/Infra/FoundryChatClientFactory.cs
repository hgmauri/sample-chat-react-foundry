using System.ClientModel.Primitives;
using Azure.Identity;
using Chat.Api.Chat;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;

namespace Chat.Api.Infra;

public static class FoundryChatClientFactory
{
    /// <summary>
    /// Cria o IChatClient apontando para o endpoint v1 do Foundry, autenticando com Entra ID.
    /// Sem chave de API no código, no appsettings ou em variável de ambiente.
    /// </summary>
    public static IChatClient Criar(FoundryOptions opcoes)
    {
        // Local: usa o login do az CLI / Visual Studio. Em produção: Managed Identity.
        var politica = new BearerTokenPolicy(new DefaultAzureCredential(), "https://ai.azure.com/.default");

#pragma warning disable OPENAI001 // construtor com AuthenticationPolicy ainda marcado como experimental
        var chatClient = new ChatClient(
            model: opcoes.Deployment,
            authenticationPolicy: politica,
            options: new OpenAIClientOptions { Endpoint = new Uri(new Uri(opcoes.Endpoint!), "openai/v1/") });
#pragma warning restore OPENAI001

        return chatClient.AsIChatClient();
    }
}
