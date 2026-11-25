export const LIMITE_INTEIRO = 2_147_483_647

export function precoValido(texto: string) {
  const valor = Number(texto)
  return /^\d+(\.\d{1,2}0*)?$/.test(texto)
    && Number.isFinite(valor)
    && valor > 0
    && valor * 100 <= Number.MAX_SAFE_INTEGER
}
