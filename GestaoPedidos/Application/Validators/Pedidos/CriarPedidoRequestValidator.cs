using FluentValidation;
using GestaoPedidos.Application.DTO.Pedidos;

namespace GestaoPedidos.Application.Validators.Pedidos
{
    public class CriarPedidoRequestValidator : AbstractValidator<CriarPedidoRequestDTO>
    {
        public CriarPedidoRequestValidator()
        {
            RuleFor(x => x.PessoaId).GreaterThan(0);
            RuleFor(x => x.tipoMovimentacao).IsInEnum()
                .WithMessage("Tipo de movimentação inválido.");
            RuleForEach(x => x.Itens).SetValidator(new CriarPedidoItemRequestValidator());
            RuleFor(x => x.Itens)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(itens => itens.All(item => item is not null))
                .WithMessage("Os itens do pedido não podem ser nulos.")
                .Must(itens => itens.Select(i => i.ProdutoId).Distinct().Count() == itens.Count)
                .WithMessage("Não é permitido repetir o mesmo produto no pedido.");
        }
    }

    public class CriarPedidoItemRequestValidator : AbstractValidator<CriarPedidoItemRequestDTO>
    {
        public CriarPedidoItemRequestValidator()
        {
            RuleFor(x => x.ProdutoId).GreaterThan(0);
            RuleFor(x => x.Quantidade).GreaterThan(0);
        }
    }
}
