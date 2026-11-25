using FluentValidation;
using GestaoPedidos.Application.DTO.Produtos;
using GestaoPedidos.Domain.Exceptions.Produtos;


namespace GestaoPedidos.Application.Validators.Produtos
{
    public class ProdutoUpdateValidator : AbstractValidator<ProdutoUpdateDTO>
    {
        public ProdutoUpdateValidator()
        {
            RuleFor(p => p.Nome).NotEmpty().WithMessage(ProdutoExceptions.Produto_NomeObrigatorio)
                .MaximumLength(160);
            RuleFor(p => p.Marca).NotEmpty().WithMessage(ProdutoExceptions.Produto_MarcaObrigatorio)
                .MaximumLength(120);
            RuleFor(p => p.Estoque).GreaterThanOrEqualTo(0).WithMessage(ProdutoExceptions.Produto_EstoqueObrigatorio);
            RuleFor(p => p.Preco).GreaterThan(0).WithMessage(ProdutoExceptions.Produto_PrecoInvalido)
                .PrecisionScale(18, 2, true);
        }
    }
}
