import Markdown from 'react-markdown'
import type { Mensagem } from '../api/chat'

export function Bolha({ mensagem, digitando }: { mensagem: Mensagem; digitando: boolean }) {
  if (mensagem.papel === 'user') {
    return <div className="bolha bolha-usuario">{mensagem.texto}</div>
  }

  return (
    <div className="bolha bolha-assistente">
      {mensagem.texto ? <Markdown>{mensagem.texto}</Markdown> : <span className="pensando">Pensando</span>}
      {digitando && mensagem.texto && <span className="cursor" aria-hidden />}
    </div>
  )
}
