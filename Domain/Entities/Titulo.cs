using GestaoPedidos.Domain.Enum;
using GestaoPedidos.Domain.Exceptions;

namespace GestaoPedidos.Domain.Entities
{
    public class Titulo
    {
        public int Id { get; private set; }
        public string NomeTitulo { get; private set; } = string.Empty;
        public decimal ValorTitulo { get; private set; }
        public int ClienteId { get; private set; }
        public string Observacao { get; private set; } = string.Empty;
        public TipoTitulo Tipo { get; private set; }
        public bool StBaixado { get; private set; } = false;
        public DateTime DataCadastro { get; private set; } = DateTime.UtcNow;
        public int? IdOrigem { get; private set; } = null;
        public Origem? NomeOrigem { get; private set; } = null;

        protected Titulo() { }

        public Titulo(string nomeTitulo, decimal valorTitulo, int clienteId, string observacao, TipoTitulo tipo, int? idOrigem, Origem? nomeOrigem)
        {
            NomeTitulo = nomeTitulo;
            ValorTitulo = valorTitulo;
            ClienteId = clienteId;
            Observacao = observacao;
            Tipo = tipo;
            IdOrigem = idOrigem;
            NomeOrigem = nomeOrigem;
        }

        public void ConverterValor()
        {
            if (Tipo == TipoTitulo.Saída)
                ValorTitulo = -Math.Abs(ValorTitulo);
            else
                ValorTitulo = +Math.Abs(ValorTitulo);
        }

        public void BaixarTitulo()
        {
            if (StBaixado)
                throw new BadRequestException("O título já está baixado.");
            StBaixado = true;
        }

        public void RemoverBaixaTitulo()
        {
            if (!StBaixado)
                throw new BadRequestException("O título já está sem baixa.");
            StBaixado = false;
        }
    }
}