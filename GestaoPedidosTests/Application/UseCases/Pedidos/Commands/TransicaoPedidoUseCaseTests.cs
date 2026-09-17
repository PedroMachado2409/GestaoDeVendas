using AutoMapper;
using FluentAssertions;
using GestaoPedidos.Application.DTO.Pedidos;
using GestaoPedidos.Application.Mapper;
using GestaoPedidos.Application.UseCases.Pedidos.Commands;
using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Entities;
using GestaoPedidos.Domain.Entities.Pedidos;
using GestaoPedidos.Domain.Enum;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace GestaoPedidosTests.Application.UseCases.Pedidos.Commands
{
    [TestClass]
    public class TransicaoPedidoUseCaseTests
    {
        private Mock<IPedidoRepository> _pedidoRepository = null!;
        private Mock<IProdutoRepository> _produtoRepository = null!;
        private Mock<IMovimentacaoEstoqueRepository> _movimentacaoEstoqueRepository = null!;
        private Mock<IUnitOfWork> _unitOfWork = null!;
        private IMapper _mapper = null!;

        [TestInitialize]
        public void Setup()
        {
            _pedidoRepository = new Mock<IPedidoRepository>();
            _produtoRepository = new Mock<IProdutoRepository>();
            _unitOfWork = new Mock<IUnitOfWork>();
            _unitOfWork
                .Setup(x => x.ExecutarEmTransacao(
                    It.IsAny<Func<Task<PedidoResponseDTO>>>()))
                .Returns((Func<Task<PedidoResponseDTO>> operacao) => operacao());
            _unitOfWork.Setup(x => x.SalvarAlteracoes()).ReturnsAsync(1);

            var config = new MapperConfiguration(
                cfg => cfg.AddProfile<PedidoProfile>());
            _mapper = config.CreateMapper();
        }

        [TestMethod]
        public async Task Cancelar_Duas_Vezes_Nao_Deve_Devolver_Estoque_Duas_Vezes()
        {
            var (pedido, produto) = CriarPedidoAbertoComReserva();
            _pedidoRepository.Setup(x => x.ObterPorId(1)).ReturnsAsync(pedido);
            _produtoRepository
                .Setup(x => x.ObterPorIds(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync([produto]);

            var useCase = new CancelarPedidoUseCase(
                _mapper,
                _pedidoRepository.Object,
                _produtoRepository.Object,
                _unitOfWork.Object);

            var primeiraResposta = await useCase.Executar(1);
            var segundaResposta = await useCase.Executar(1);

            primeiraResposta.Status.Should().Be(nameof(StatusPedido.Cancelado));
            segundaResposta.Status.Should().Be(nameof(StatusPedido.Cancelado));
            produto.Estoque.Should().Be(10);
            produto.QuantidadeReservada.Should().Be(0);
            _produtoRepository.Verify(
                x => x.ObterPorIds(It.IsAny<IEnumerable<int>>()),
                Times.Once);
            _unitOfWork.Verify(x => x.SalvarAlteracoes(), Times.Once);
        }

        [TestMethod]
        public async Task Finalizar_Duas_Vezes_Nao_Deve_Consumir_Reserva_Duas_Vezes()
        {
            var (pedido, produto) = CriarPedidoAbertoComReserva();
            _pedidoRepository.Setup(x => x.ObterPorId(1)).ReturnsAsync(pedido);
            _produtoRepository
                .Setup(x => x.ObterPorIds(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync([produto]);

            var useCase = new FinalizarPedidoUseCase(
                _pedidoRepository.Object,
                _produtoRepository.Object,
                _movimentacaoEstoqueRepository.Object,
                _unitOfWork.Object,
                _mapper);

            var primeiraResposta = await useCase.Executar(1);
            var segundaResposta = await useCase.Executar(1);

            primeiraResposta.Status.Should().Be(nameof(StatusPedido.Finalizado));
            segundaResposta.Status.Should().Be(nameof(StatusPedido.Finalizado));
            produto.Estoque.Should().Be(8);
            produto.QuantidadeReservada.Should().Be(0);
            _produtoRepository.Verify(
                x => x.ObterPorIds(It.IsAny<IEnumerable<int>>()),
                Times.Once);
            _unitOfWork.Verify(x => x.SalvarAlteracoes(), Times.Once);
        }

        private static (Pedido Pedido, Produto Produto) CriarPedidoAbertoComReserva()
        {
            var produto = new Produto("Produto", "Marca", 10, 25);
            produto.ReservarQuantidadeVenda(2);
            var pedido = new Pedido(1, [new PedidoItem(produto.Id, produto.Preco, 2)], 1, 50, TipoMovimentacao.Saida);

            return (pedido, produto);
        }
    }
}
