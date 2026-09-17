using GestaoPedidos.Domain.Exceptions;

namespace GestaoPedidos.Domain.Entities.PrazoPagamento
{
    public class PrazoPagamento
    {
        public int Id { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public bool Ativo { get; private set; } = true;
        public Guid Versao { get; private set; } = Guid.NewGuid();

        private readonly List<PrazoPagamentoParcela> _parcelas = new();
        public IReadOnlyCollection<PrazoPagamentoParcela> Parcelas => _parcelas.AsReadOnly();

        protected PrazoPagamento() { }

        public PrazoPagamento(string nome, IEnumerable<(int Dias, decimal Percentual)> parcelas)
        {
            Nome = nome.Trim();
           
        }

        public void DefinirParcelas(IEnumerable<(int Dias, decimal Percentual)> parcelas)
        {
            var lista = parcelas.ToList();

            if (lista.Count == 0)
            {
                throw new BadRequestException("O prazo deve ter ao menos uma parcela.");
            }

            if (lista.Any(p => p.Percentual <= 0))
            {
                throw new BadRequestException("O Percentual da parcela deve ser maior que 0");
            }

            if (lista.Any(p => p.Dias < 0))
            {
                throw new BadRequestException("Os dias não podem ser negativos.");
            }

            for (var i = 1; i < lista.Count; i++)
            {
                if (lista[i].Dias <= lista[i - 1].Dias)
                {
                    throw new BadRequestException("Os dias das parcelas devem ser crescentes.");
                }
            }

            if (lista.Sum(p => p.Percentual) != 100m)
            {
                throw new BadRequestException("A soma dos percentuais deve ser 100%.");
            }

            _parcelas.Clear();

            for (var i = 0; i < lista.Count; i++)
            {
                _parcelas.Add(new PrazoPagamentoParcela(i + 1, lista[i].Dias, lista[i].Percentual));
            }

            AtualizarVersao();
     
        }
        public void Ativar()
        {
            Ativo = true;
            AtualizarVersao();
        }

        public void Inativar()
        {
            Ativo = false;
            AtualizarVersao();
        }

        private void AtualizarVersao() => Versao = Guid.NewGuid();

    }
}
