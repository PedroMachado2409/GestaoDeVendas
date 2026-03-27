using FluentValidation;
using GestaoPedidos.Application.DTO.Usuarios;
using GestaoPedidos.Domain.Exceptions.Usuarios;

namespace GestaoPedidos.Application.Validators.Usuarios
{
    public class UsuarioCreateValidator : AbstractValidator<UsuarioCreateDTO>
    {
        public UsuarioCreateValidator()
        {
            RuleFor(u => u.Nome).NotEmpty().WithMessage(UsuariosExceptions.Usuario_NomeObrigatorio);
            RuleFor(u => u.Email)
               .Cascade(CascadeMode.Stop)
               .NotEmpty().WithMessage(UsuariosExceptions.Usuario_EmailObrigatorio)
               .EmailAddress().WithMessage(UsuariosExceptions.Usuario_EmailInvalido);
            //RuleFor(u => u.Role).NotEmpty().WithMessage(UsuariosExceptions.Usuario_RoleObrigatoria);
            RuleFor(u => u.Senha).NotEmpty().WithMessage(UsuariosExceptions.Usuario_SenhaObrigatorio);
        }
    }
}
