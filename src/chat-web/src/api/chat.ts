export type Papel = 'user' | 'assistant'

export interface Mensagem {
  papel: Papel
  texto: string
}

export type EventoChat =
  | { tipo: 'texto'; texto: string }
  | { tipo: 'uso'; tokens?: number }
  | { tipo: 'erro'; texto: string }

/**
 * Envia a conversa para a API e devolve os eventos conforme chegam (Server-Sent Events).
 * Usa fetch em vez de EventSource porque precisamos de POST com corpo e de cancelamento.
 */
export async function* conversar(mensagens: Mensagem[], signal: AbortSignal): AsyncGenerator<EventoChat> {
  const resposta = await fetch('/api/chat', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ mensagens }),
    signal,
  })

  if (resposta.status === 429) throw new Error('Muitas mensagens em pouco tempo. Aguarde um minuto e tente de novo.')
  if (!resposta.ok || !resposta.body) throw new Error('Não foi possível falar com o servidor.')

  const leitor = resposta.body.pipeThrough(new TextDecoderStream()).getReader()
  let buffer = ''

  while (true) {
    const { value, done } = await leitor.read()
    if (done) break

    buffer += value
    const blocos = buffer.split('\n\n')
    buffer = blocos.pop() ?? '' // o último bloco pode estar incompleto

    for (const bloco of blocos) {
      const dados = bloco
        .split('\n')
        .filter((linha) => linha.startsWith('data:'))
        .map((linha) => linha.slice(5).trimStart())
        .join('\n')

      if (dados) yield JSON.parse(dados) as EventoChat
    }
  }
}
