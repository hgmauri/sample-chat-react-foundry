# sample-chat-react-foundry

Chat inteligente com **React**, **.NET 10** e **Microsoft Foundry** usando o modelo **gpt-5.4-mini**, com resposta em streaming.

- Front em React + TypeScript (Vite), renderizando Markdown conforme a resposta chega
- API em ASP.NET Core Minimal API, com `IChatClient` do Microsoft.Extensions.AI
- Streaming com Server-Sent Events nativo do .NET 10 (`TypedResults.ServerSentEvents`)
- Autenticação no Foundry com Entra ID (`DefaultAzureCredential`), sem chave de API
- Prompt de sistema no servidor, validação de entrada, limite de histórico e rate limit por IP
- Modo simulado: sem configurar o Foundry, a aplicação roda inteira localmente, sem custo

Artigo completo: [Criando um chat inteligente com React, .NET e Microsoft Foundry](https://henriquemauri.net/criando-um-chat-inteligente-com-react-net-e-microsoft-foundry/)

## Estrutura

```
src/
  Chat.Api/                 API .NET 10
    Chat/                   Endpoint, contratos, prompt de sistema
    Infra/                  Criação do IChatClient (Foundry ou simulado)
  chat-web/                 Front React + TypeScript (Vite)
    src/api/chat.ts         Cliente SSE com fetch + ReadableStream
    src/hooks/useChat.ts    Estado da conversa, streaming e cancelamento
```

## Rodando sem Azure (modo simulado)

```bash
# terminal 1
dotnet run --project src/Chat.Api

# terminal 2
cd src/chat-web
npm install
npm run dev
```

Abra http://localhost:5173. Sem `Foundry:Endpoint` configurado, a API usa um `IChatClient` simulado.

## Conectando no Microsoft Foundry

1. Crie um recurso do Foundry e faça o deploy do modelo `gpt-5.4-mini` (portal em https://ai.azure.com ou via CLI).
2. Dê ao seu usuário a role **Cognitive Services OpenAI User** no recurso.
3. Faça login com `az login`.
4. Configure o endpoint com user secrets:

```bash
cd src/Chat.Api
dotnet user-secrets init
dotnet user-secrets set "Foundry:Endpoint" "https://SEU-RECURSO.openai.azure.com/"
dotnet user-secrets set "Foundry:Deployment" "gpt-5.4-mini"
```

Exemplo de deploy via CLI (ajuste região, nome e versão do modelo conforme a disponibilidade da sua assinatura):

```bash
az cognitiveservices account deployment create \
  --name SEU-RECURSO --resource-group SEU-RG \
  --deployment-name gpt-5.4-mini \
  --model-name gpt-5.4-mini --model-version "2026-03-17" --model-format OpenAI \
  --sku-name GlobalStandard --sku-capacity 10
```

## Produção

- Use **Managed Identity** no App Service ou Container Apps, com a mesma role.
- Faça o build do front (`npm run build`) e sirva os arquivos estáticos pela própria API ou por um Static Web App.
- Ajuste o rate limit e o `MaxTokensResposta` de acordo com o seu orçamento.
- Coloque autenticação de usuário na frente do endpoint `/api/chat`.

## Licença

MIT

---

Feito por [Henrique Mauri](https://henriquemauri.net) | [Dotcode](https://dotcode.com.br)
