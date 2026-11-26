using FluentValidation;
using GestaoPedidos.Application.DTO.TitulosFinanceiros;
using GestaoPedidos.Domain.Exceptions.TituloFinanceiro;

namespace GestaoPedidos.Application.Validators.TitulosFinanceiros
{
    public class BaixaTituloFinanceiroCreateValidator : AbstractValidator<BaixaTituloFinanceiroCreateDTO>
    {
        public BaixaTituloFinanceiroCreateValidator()
        {
            RuleFor(b => b.OperacaoId).NotEmpty().WithMessage(TItuloExceptions.Baixa_OperacaoObrigatoria);
            RuleFor(b => b.Valor).GreaterThan(0).WithMessage(TItuloExceptions.Titulo_ValorInvalido)
                .PrecisionScale(18, 2, true).WithMessage(TItuloExceptions.Titulo_ValorInvalido);
            RuleFor(b => b.Data).NotEmpty().WithMessage(TItuloExceptions.Baixa_DataInvalida)
                .LessThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.UtcNow))
                .WithMessage(TItuloExceptions.Baixa_DataInvalida);
            RuleFor(b => b.Forma).IsInEnum().WithMessage(TItuloExceptions.Baixa_FormaInvalida);
        }
    }
}
