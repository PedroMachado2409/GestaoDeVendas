using FluentValidation;
using GestaoPedidos.Application.DTO.Usuarios;

namespace GestaoPedidos.Application.Validators.Usuarios
{
    public class UsuarioUpdateValidator : AbstractValidator<UsuarioUpdateDTO>
    {
        public UsuarioUpdateValidator()
        {
            RuleFor(x => x.Nome).NotEmpty().MaximumLength(120);
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(254);
        }
    }
}
