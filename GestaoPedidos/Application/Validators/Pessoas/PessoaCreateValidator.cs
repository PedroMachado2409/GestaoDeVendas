using FluentValidation;
using GestaoPedidos.Application.DTO.Pessoas;
using GestaoPedidos.Domain.Exceptions.Pessoas;

namespace GestaoPedidos.Application.Validators.Pessoas
{
    public class PessoaCreateValidator : AbstractValidator<PessoaCreateDTO>
    {
        public PessoaCreateValidator()
        {
            RuleFor(c => c.TipoPessoa).IsInEnum().WithMessage("Tipo de pessoa inválido.");
            RuleFor(c => c.Nome).NotEmpty().WithMessage(PessoasExceptions.Pessoa_NomeObrigatorio)
                .MaximumLength(120);
            RuleFor(c => c.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(PessoasExceptions.Pessoa_EmailObrigatorio)
                .EmailAddress().WithMessage(PessoasExceptions.Pessoa_EmailInvalido)
                .MaximumLength(254);

            RuleFor(c => c.Cpf)
                 .Cascade(CascadeMode.Stop)
                 .NotEmpty().WithMessage(PessoasExceptions.Pessoa_CpfObrigatorio)
                 .Must(CpfValidator.EhValido).WithMessage(PessoasExceptions.Pessoa_CpfInvalido);

        }
    }
}
