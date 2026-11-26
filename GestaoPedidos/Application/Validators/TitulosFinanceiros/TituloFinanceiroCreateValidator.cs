using FluentValidation;
using GestaoPedidos.Application.DTO.TitulosFinanceiros;
using GestaoPedidos.Domain.Exceptions.TituloFinanceiro;

namespace GestaoPedidos.Application.Validators.TitulosFinanceiros
{
    public class TituloFinanceiroCreateValidator : AbstractValidator<TituloFinanceiroCreateDTO>
    {
        public TituloFinanceiroCreateValidator()
        {
            RuleFor(t => t.PessoaId).GreaterThan(0)
                .WithMessage(TItuloExceptions.Titulo_PessoaObrigatoria);
            RuleFor(t => t.Descricao).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(TItuloExceptions.Titulo_DescricaoInvalida)
                .Must(descricao => descricao.Trim().Length <= 200)
                .WithMessage(TItuloExceptions.Titulo_DescricaoInvalida);
            RuleFor(t => t.Tipo).IsInEnum()
                .WithMessage(TItuloExceptions.Titulo_TipoInvalido);
            RuleFor(t => t.ValorOriginal).GreaterThan(0)
                .WithMessage(TItuloExceptions.Titulo_ValorInvalido)
                .PrecisionScale(18, 2, true)
                .WithMessage(TItuloExceptions.Titulo_ValorInvalido);
            RuleFor(t => t.DataVencimento).NotEmpty()
                .WithMessage(TItuloExceptions.Titulo_VencimentoObrigatorio);
        }
    }
}
