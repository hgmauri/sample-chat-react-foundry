import { useCallback, useRef, useState } from 'react'
import { conversar, type Mensagem } from '../api/chat'

export function useChat() {
  const [mensagens, setMensagens] = useState<Mensagem[]>([])
  const [respondendo, setRespondendo] = useState(false)
  const [erro, setErro] = useState<string | null>(null)
  const [tokens, setTokens] = useState(0)
  const cancelamento = useRef<AbortController | null>(null)

  const enviar = useCallback(
    async (texto: string) => {
      const pergunta = texto.trim()
      if (!pergunta || respondendo) return

      const historico: Mensagem[] = [...mensagens, { papel: 'user', texto: pergunta }]
      setMensagens([...historico, { papel: 'assistant', texto: '' }])
      setErro(null)
      setRespondendo(true)

      const controller = new AbortController()
      cancelamento.current = controller

      const acrescentar = (trecho: string) =>
        setMensagens((atual) => {
          const copia = [...atual]
          const ultima = copia[copia.length - 1]
          copia[copia.length - 1] = { ...ultima, texto: ultima.texto + trecho }
          return copia
        })

      try {
        for await (const evento of conversar(historico, controller.signal)) {
          if (evento.tipo === 'texto') acrescentar(evento.texto)
          if (evento.tipo === 'uso' && evento.tokens) setTokens((t) => t + evento.tokens!)
          if (evento.tipo === 'erro') setErro(evento.texto)
        }
      } catch (e) {
        if (!controller.signal.aborted) setErro(e instanceof Error ? e.message : 'Erro inesperado.')
      } finally {
        // Resposta vazia (erro antes do primeiro trecho) não fica no histórico
        setMensagens((atual) => (atual.at(-1)?.texto ? atual : atual.slice(0, -1)))
        setRespondendo(false)
        cancelamento.current = null
      }
    },
    [mensagens, respondendo],
  )

  const parar = useCallback(() => cancelamento.current?.abort(), [])

  const limpar = useCallback(() => {
    cancelamento.current?.abort()
    setMensagens([])
    setErro(null)
    setTokens(0)
  }, [])

  return { mensagens, respondendo, erro, tokens, enviar, parar, limpar }
}
