using FluentValidation;
using GestaoPedidos.Application.DTO.Produtos;
using GestaoPedidos.Domain.Exceptions.Produtos;

namespace GestaoPedidos.Application.Validators.Produtos
{
    public class AjusteEstoqueProdutoValidator : AbstractValidator<AjusteEstoqueProdutoDTO>
    {
        public AjusteEstoqueProdutoValidator()
        {
            RuleFor(p => p.Quantidade).NotEqual(0)
                .WithMessage(ProdutoExceptions.Produto_AjusteNaoPodeSerZero);
        }
    }
}
