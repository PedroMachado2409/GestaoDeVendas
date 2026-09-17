import { z } from 'zod'

import { somenteDigitos } from './utils'

/**
 * Porte do CpfValidator.EhValido do backend. Validar aqui não substitui a
 * validação do servidor — só evita uma ida à API para errar o óbvio.
 */
export function cpfValido(valor: string) {
  const numeros = somenteDigitos(valor)

  if (numeros.length !== 11 || new Set(numeros).size === 1) {
    return false
  }

  const primeiro = calcularDigito(numeros.slice(0, 9), 10)
  const segundo = calcularDigito(numeros.slice(0, 10), 11)

  return Number(numeros[9]) === primeiro && Number(numeros[10]) === segundo
}

function calcularDigito(numeros: string, pesoInicial: number) {
  const soma = [...numeros].reduce(
    (total, digito, indice) => total + Number(digito) * (pesoInicial - indice),
    0,
  )
  const resto = soma % 11
  return resto < 2 ? 0 : 11 - resto
}

// O backend usa EmailAddress() do FluentValidation, que apenas exige texto
// antes e depois do "@". Manter o mesmo critério evita recusar aqui um
// endereço que a API aceitaria.
const formatoDeEmail = /^[^@\s]+@[^@\s]+$/

export const esquemaEmail = z
  .string()
  .trim()
  .min(1, 'Informe o e-mail.')
  .max(254, 'O e-mail deve ter no máximo 254 caracteres.')
  .regex(formatoDeEmail, 'Informe um e-mail válido.')

export const esquemaCpf = z
  .string()
  .trim()
  .min(1, 'Informe o CPF.')
  .refine(cpfValido, 'CPF inválido.')

/** Mesmas regras do UsuarioCreateValidator: 10+ caracteres, letra e número. */
export const esquemaSenhaNova = z
  .string()
  .min(10, 'A senha deve possuir pelo menos 10 caracteres.')
  .max(128, 'A senha deve ter no máximo 128 caracteres.')
  .regex(/[A-Za-z]/, 'A senha deve conter ao menos uma letra.')
  .regex(/[0-9]/, 'A senha deve conter ao menos um número.')

/** Força aproximada, só para dar retorno visual no cadastro. */
export function forcaDaSenha(senha: string) {
  if (!senha) return { nivel: 0, rotulo: 'Vazia' }

  let pontos = 0
  if (senha.length >= 10) pontos += 1
  if (senha.length >= 14) pontos += 1
  if (/[A-Za-z]/.test(senha) && /[0-9]/.test(senha)) pontos += 1
  if (/[^A-Za-z0-9]/.test(senha)) pontos += 1

  const rotulos = ['Fraca', 'Fraca', 'Razoável', 'Boa', 'Forte'] as const
  return { nivel: pontos, rotulo: rotulos[pontos] ?? 'Fraca' }
}
