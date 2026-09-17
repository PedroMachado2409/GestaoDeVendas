namespace GestaoPedidos.Application.Validators.Clientes
{
    public static class CpfValidator
    {
        public static bool EhValido(string cpf)
        {
            var numeros = new string(cpf.Where(char.IsDigit).ToArray());

            if (numeros.Length != 11 || numeros.Distinct().Count() == 1)
            {
                return false;
            }

            var primeiroDigito = CalcularDigito(numeros[..9], 10);
            var segundoDigito = CalcularDigito(numeros[..10], 11);

            return numeros[9] - '0' == primeiroDigito
                && numeros[10] - '0' == segundoDigito;
        }

        private static int CalcularDigito(string numeros, int pesoInicial)
        {
            var soma = numeros
                .Select((numero, indice) => (numero - '0') * (pesoInicial - indice))
                .Sum();
            var resto = soma % 11;

            return resto < 2 ? 0 : 11 - resto;
        }
    }
}
