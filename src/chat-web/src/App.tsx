import { useEffect, useRef } from 'react'
import { Bolha } from './components/Bolha'
import { Entrada } from './components/Entrada'
import { useChat } from './hooks/useChat'

const sugestoes = [
  'Quando usar Minimal API em vez de Controllers?',
  'Como proteger segredos de uma aplicação no Azure?',
  'Explique o useEffect com um exemplo curto.',
]

export default function App() {
  const { mensagens, respondendo, erro, tokens, enviar, parar, limpar } = useChat()
  const fim = useRef<HTMLDivElement>(null)

  useEffect(() => {
    fim.current?.scrollIntoView({ behavior: 'smooth', block: 'end' })
  }, [mensagens])

  return (
    <div className="app">
      <header className="topo">
        <div>
          <h1>Assistente Dotcode</h1>
          <span className="modelo">gpt-5.4-mini via Microsoft Foundry</span>
        </div>
        <div className="topo-acoes">
          {tokens > 0 && <span className="tokens">{tokens.toLocaleString('pt-BR')} tokens</span>}
          {mensagens.length > 0 && (
            <button type="button" className="botao-texto" onClick={limpar}>
              Nova conversa
            </button>
          )}
        </div>
      </header>

      <main className="conversa">
        {mensagens.length === 0 ? (
          <div className="vazio">
            <p>Sobre o que vamos conversar?</p>
            <div className="sugestoes">
              {sugestoes.map((s) => (
                <button key={s} type="button" onClick={() => enviar(s)}>
                  {s}
                </button>
              ))}
            </div>
          </div>
        ) : (
          mensagens.map((m, i) => (
            <Bolha key={i} mensagem={m} digitando={respondendo && i === mensagens.length - 1} />
          ))
        )}
        {erro && <div className="erro">{erro}</div>}
        <div ref={fim} />
      </main>

      <Entrada respondendo={respondendo} onEnviar={enviar} onParar={parar} />
    </div>
  )
}
