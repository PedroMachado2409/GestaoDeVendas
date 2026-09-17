using FluentValidation;
using GestaoPedidos.Application.DTO.Usuarios;

namespace GestaoPedidos.Application.Validators.Usuarios
{
    public class UsuarioRoleUpdateValidator : AbstractValidator<UsuarioRoleUpdateDTO>
    {
        public UsuarioRoleUpdateValidator()
        {
            RuleFor(x => x.Role).IsInEnum();
        }
    }
}
