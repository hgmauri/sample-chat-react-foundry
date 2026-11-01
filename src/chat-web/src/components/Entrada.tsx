import { useState, type KeyboardEvent } from 'react'

interface Props {
  respondendo: boolean
  onEnviar: (texto: string) => void
  onParar: () => void
}

export function Entrada({ respondendo, onEnviar, onParar }: Props) {
  const [texto, setTexto] = useState('')

  const enviar = () => {
    if (!texto.trim()) return
    onEnviar(texto)
    setTexto('')
  }

  // Enter envia, Shift+Enter quebra linha
  const aoTeclar = (e: KeyboardEvent<HTMLTextAreaElement>) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault()
      enviar()
    }
  }

  return (
    <div className="entrada">
      <textarea
        value={texto}
        onChange={(e) => setTexto(e.target.value)}
        onKeyDown={aoTeclar}
        placeholder="Pergunte sobre .NET, Azure ou React"
        rows={1}
        maxLength={4000}
        aria-label="Mensagem"
      />
      {respondendo ? (
        <button type="button" className="botao botao-parar" onClick={onParar}>
          Parar
        </button>
      ) : (
        <button type="button" className="botao" onClick={enviar} disabled={!texto.trim()}>
          Enviar
        </button>
      )}
    </div>
  )
}
