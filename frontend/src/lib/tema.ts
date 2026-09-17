export type Tema = 'claro' | 'escuro' | 'sistema'

const CHAVE = 'gp.tema'

export function lerTema(): Tema {
  const salvo = localStorage.getItem(CHAVE)
  return salvo === 'claro' || salvo === 'escuro' || salvo === 'sistema' ? salvo : 'sistema'
}

export function aplicarTema(tema: Tema) {
  const escuro =
    tema === 'escuro' ||
    (tema === 'sistema' && window.matchMedia('(prefers-color-scheme: dark)').matches)

  document.documentElement.classList.toggle('dark', escuro)
  localStorage.setItem(CHAVE, tema)
}

/** Reage à troca de tema do sistema operacional enquanto a opção for "sistema". */
export function observarTemaDoSistema(aoMudar: () => void) {
  const consulta = window.matchMedia('(prefers-color-scheme: dark)')
  consulta.addEventListener('change', aoMudar)
  return () => consulta.removeEventListener('change', aoMudar)
}
