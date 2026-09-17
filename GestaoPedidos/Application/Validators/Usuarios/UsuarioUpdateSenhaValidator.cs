using FluentValidation;
using GestaoPedidos.Application.DTO.Usuarios;

namespace GestaoPedidos.Application.Validators.Usuarios
{
    public class UsuarioUpdateSenhaValidator : AbstractValidator<UsuarioUpdateSenhaDTO>
    {
        public UsuarioUpdateSenhaValidator()
        {
            RuleFor(x => x.SenhaAntiga).NotEmpty().MaximumLength(128);
            RuleFor(x => x.NovaSenha)
                .NotEmpty()
                .MinimumLength(10)
                .MaximumLength(128)
                .Matches("[A-Za-z]").WithMessage("A nova senha deve conter ao menos uma letra.")
                .Matches("[0-9]").WithMessage("A nova senha deve conter ao menos um número.")
                .NotEqual(x => x.SenhaAntiga).WithMessage("A nova senha deve ser diferente da senha atual.");
        }
    }
}
