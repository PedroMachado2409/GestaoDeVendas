using GestaoPedidos.Domain.Enum;
using GestaoPedidos.Domain.Exceptions;
using GestaoPedidos.Domain.Exceptions.TituloFinanceiro;

namespace GestaoPedidos.Domain.Entities
{
    public class TituloFinanceiro
    {
        private readonly List<BaixaTituloFinanceiro> _baixas = new();

        public int Id { get; private set; }
        public int PessoaId { get; private set; }
        public string Descricao { get; private set; } = string.Empty;
        public TipoTituloFinanceiro Tipo { get; private set; }
        public decimal ValorOriginal { get; private set; }
        public DateTime DataVencimento { get; private set; }
        public DateTime DataCadastro { get; private set; } = DateTime.UtcNow;
        public StatusTituloFinanceiro Status { get; private set; } = StatusTituloFinanceiro.Aberto;
        public Guid Versao { get; private set; } = Guid.NewGuid();
        public IReadOnlyCollection<BaixaTituloFinanceiro> Baixas => _baixas.AsReadOnly();
        public decimal ValorBaixado => _baixas.Where(b => !b.Estornada).Sum(b => b.Valor);
        public decimal Saldo => ValorOriginal - ValorBaixado;

        public int? IdOrigem {  get; private set; }
        public OrigemTitulo OrigemTitulo { get; private set; }

        protected TituloFinanceiro() { }

        public TituloFinanceiro(int pessoaId, string descricao, TipoTituloFinanceiro tipo,
            decimal valorOriginal, DateTime dataVencimento, int idOrigem, OrigemTitulo origemTitulo )
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
            DataVencimento = dataVencimento.Kind == DateTimeKind.Local
                ? dataVencimento.ToUniversalTime()
                : DateTime.SpecifyKind(dataVencimento, DateTimeKind.Utc);
            IdOrigem = idOrigem;
            OrigemTitulo = origemTitulo;

        }

        public void Baixar(Guid operacaoId, decimal valor, DateOnly data, FormaPagamento forma, int usuarioId)
        {
            if (operacaoId == Guid.Empty)
            {
                throw new BadRequestException(TItuloExceptions.Baixa_OperacaoObrigatoria);
            }

            var existente = _baixas.FirstOrDefault(b => b.OperacaoId == operacaoId);
            if (existente is not null)
            {
                if (existente.Valor != valor || existente.Data != data || existente.Forma != forma
                    || existente.UsuarioId != usuarioId)
                {
                    throw new ConflictException(TItuloExceptions.Baixa_OperacaoReutilizada);
                }
                return;
            }

            if (Status != StatusTituloFinanceiro.Aberto && Status != StatusTituloFinanceiro.ParcialmenteBaixado)
            {
                throw new ConflictException(TItuloExceptions.Baixa_TituloIndisponivel);
            }

            if (valor <= 0 || decimal.Round(valor, 2) != valor)
            {
                throw new BadRequestException(TItuloExceptions.Titulo_ValorInvalido);
            }

            if (valor > Saldo)
            {
                throw new ConflictException(TItuloExceptions.Baixa_ValorAcimaDoSaldo);
            }

            if (data == default || data > DateOnly.FromDateTime(DateTime.UtcNow))
            {
                throw new BadRequestException(TItuloExceptions.Baixa_DataInvalida);
            }

            if (!System.Enum.IsDefined(forma))
            {
                throw new BadRequestException(TItuloExceptions.Baixa_FormaInvalida);
            }

            if (usuarioId <= 0)
            {
                throw new BadRequestException(TItuloExceptions.Baixa_UsuarioObrigatorio);
            }

            _baixas.Add(new BaixaTituloFinanceiro(operacaoId, valor, data, forma, usuarioId));
            AtualizarSituacao();
        }

        public void EstornarBaixa(int baixaId, int usuarioId)
        {
            var baixa = _baixas.FirstOrDefault(b => b.Id == baixaId)
                ?? throw new NotFoundException(TItuloExceptions.Baixa_NaoEncontrada);

            if (baixa.Estornada) return;

            if (usuarioId <= 0)
            {
                throw new BadRequestException(TItuloExceptions.Baixa_UsuarioObrigatorio);
            }

            baixa.Estornar(usuarioId);
            AtualizarSituacao();
        }

        private void AtualizarSituacao()
        {
            Status = ValorBaixado == 0 ? StatusTituloFinanceiro.Aberto
                : Saldo == 0 ? StatusTituloFinanceiro.Baixado : StatusTituloFinanceiro.ParcialmenteBaixado;
            Versao = Guid.NewGuid();
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
