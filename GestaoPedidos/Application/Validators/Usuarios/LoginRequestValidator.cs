using FluentValidation;
using GestaoPedidos.Application.DTO.Usuarios;

namespace GestaoPedidos.Application.Validators.Usuarios
{
    public class LoginRequestValidator : AbstractValidator<LoginRequestDTO>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(254);
            RuleFor(x => x.Senha).NotEmpty().MaximumLength(128);
        }
    }
}
