using GestaoPedidos.Domain.Enum;
using GestaoPedidos.Domain.Exceptions;
using GestaoPedidos.Domain.Exceptions.TituloFinanceiro;

namespace GestaoPedidos.Domain.Entities
{
    public class TituloFinanceiro
    {
        public int Id { get; private set; }
        public int PessoaId { get; private set; }
        public string Descricao { get; private set; } = string.Empty;
        public TipoTituloFinanceiro Tipo { get; private set; }
        public decimal ValorOriginal { get; private set; }
        public DateOnly DataVencimento { get; private set; }
        public DateTime DataCadastro { get; private set; } = DateTime.UtcNow;
        public StatusTituloFinanceiro Status { get; private set; } = StatusTituloFinanceiro.Aberto;
        public Guid Versao { get; private set; } = Guid.NewGuid();

        protected TituloFinanceiro() { }

        public TituloFinanceiro(int pessoaId, string descricao, TipoTituloFinanceiro tipo,
            decimal valorOriginal, DateOnly dataVencimento)
        {

            var descricaoNormalizada = descricao?.Trim();
            if (string.IsNullOrWhiteSpace(descricaoNormalizada) || descricaoNormalizada.Length > 200)
            {
                throw new BadRequestException(TItuloExceptions.Titulo_DescricaoInvalida);
            }

            if (!System.Enum.IsDefined(tipo))
            {
                throw new BadRequestException(TItuloExceptions.Titulo_TipoInvalido);
            }

            if (valorOriginal <= 0 || valorOriginal > 9_999_999_999_999_999.99m
                || decimal.Round(valorOriginal, 2) != valorOriginal)
            {
                throw new BadRequestException(TItuloExceptions.Titulo_ValorInvalido);
            }

            PessoaId = pessoaId;
            Descricao = descricaoNormalizada;
            Tipo = tipo;
            ValorOriginal = valorOriginal;
            DataVencimento = dataVencimento;
        }

        public void Cancelar()
        {
            if (Status == StatusTituloFinanceiro.Cancelado)
            {
                throw new ConflictException(TItuloExceptions.Titulo_JaCancelado);
            }

            if (Status != StatusTituloFinanceiro.Aberto)
            {
                throw new ConflictException(TItuloExceptions.Titulo_SemEstarAbertoNaoPodeCancelar);
            }

            Status = StatusTituloFinanceiro.Cancelado;
            Versao = Guid.NewGuid();
        }
    }
}
