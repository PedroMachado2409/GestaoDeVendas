using FluentValidation;
using GestaoPedidos.Application.DTO.Pedidos;

namespace GestaoPedidos.Application.Validators.Pedidos
{
    public class CriarPedidoRequestValidator : AbstractValidator<CriarPedidoRequestDTO>
    {
        public CriarPedidoRequestValidator()
        {
            RuleFor(x => x.ClienteId).GreaterThan(0);
            RuleFor(x => x.Itens).NotEmpty();
            RuleForEach(x => x.Itens).SetValidator(new CriarPedidoItemRequestValidator());
            RuleFor(x => x.Itens)
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
