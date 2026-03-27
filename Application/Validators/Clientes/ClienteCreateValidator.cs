using FluentValidation;
using GestaoPedidos.Application.DTO.Clientes;
using GestaoPedidos.Domain.Exceptions.Clientes;

namespace GestaoPedidos.Application.Validators.Clientes
{
    public class ClienteCreateValidator : AbstractValidator<ClienteCreateDTO>
    {
        public ClienteCreateValidator()
        {
            RuleFor(c => c.Nome).NotEmpty().WithMessage(ClientesExceptions.Cliente_NomeObrigatorio);
            RuleFor(c => c.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(ClientesExceptions.Cliente_EmailObrigatorio)
                .EmailAddress().WithMessage(ClientesExceptions.Cliente_EmailInvalido);

            RuleFor(c => c.Cpf)
                 .Cascade(CascadeMode.Stop)
                 .NotEmpty().WithMessage(ClientesExceptions.Cliente_CpfObrigatorio)
                 .Length(11).WithMessage(ClientesExceptions.Cliente_CpfInvalido);

        }
    }
}
