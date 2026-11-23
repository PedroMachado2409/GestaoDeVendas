/** Mesma normalização usada pelo cadastro de fornecedores na API. */
export function normalizarCpfCnpj(valor: string) {
  return valor.replace(/[\s./-]/g, '').toUpperCase()
}

export function formatarCpfCnpj(valor: string) {
  const documento = normalizarCpfCnpj(valor)

  if (/^\d{11}$/.test(documento)) {
    return documento.replace(/^(\d{3})(\d{3})(\d{3})(\d{2})$/, '$1.$2.$3-$4')
  }

  if (/^[A-Z0-9]{12}\d{2}$/.test(documento)) {
    return documento.replace(/^(.{2})(.{3})(.{3})(.{4})(.{2})$/, '$1.$2.$3/$4-$5')
  }

  return documento
}

/** CNPJ numérico ou alfanumérico: dois verificadores numéricos e módulo 11. */
export function cnpjValido(valor: string) {
  const documento = normalizarCpfCnpj(valor)
  if (!/^[A-Z0-9]{12}\d{2}$/.test(documento) || new Set(documento).size === 1) {
    return false
  }

  return (
    Number(documento[12]) === calcularDigitoCnpj(documento.slice(0, 12)) &&
    Number(documento[13]) === calcularDigitoCnpj(documento.slice(0, 13))
  )
}

function calcularDigitoCnpj(documento: string) {
  let soma = 0
  let peso = 2

  for (let indice = documento.length - 1; indice >= 0; indice -= 1) {
    soma += (documento.charCodeAt(indice) - 48) * peso
    peso = peso === 9 ? 2 : peso + 1
  }

  const resto = soma % 11
  return resto < 2 ? 0 : 11 - resto
}
