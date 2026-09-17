using FluentValidation;
using GestaoPedidos.Application.DTO.Usuarios;
using GestaoPedidos.Domain.Exceptions.Usuarios;

namespace GestaoPedidos.Application.Validators.Usuarios
{
    public class UsuarioCreateValidator : AbstractValidator<UsuarioCreateDTO>
    {
        public UsuarioCreateValidator()
        {
            RuleFor(u => u.Nome)
                .NotEmpty().WithMessage(UsuariosExceptions.Usuario_NomeObrigatorio)
                .MaximumLength(120);

            RuleFor(u => u.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(UsuariosExceptions.Usuario_EmailObrigatorio)
                .EmailAddress().WithMessage(UsuariosExceptions.Usuario_EmailInvalido)
                .MaximumLength(254);

            RuleFor(u => u.Senha)
                .NotEmpty().WithMessage(UsuariosExceptions.Usuario_SenhaObrigatorio)
                .MinimumLength(10).WithMessage("A senha deve possuir pelo menos 10 caracteres.")
                .MaximumLength(128)
                .Matches("[A-Za-z]").WithMessage("A senha deve conter ao menos uma letra.")
                .Matches("[0-9]").WithMessage("A senha deve conter ao menos um número.");
        }
    }
}
