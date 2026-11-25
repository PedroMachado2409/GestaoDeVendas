using AutoMapper;
using GestaoPedidos.Application.DTO.Produtos;
using GestaoPedidos.Application.UseCases.Produtos.Queries;
using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Entities;
using GestaoPedidos.Domain.Exceptions;
using Moq;

namespace GestaoPedidosTests.Application.Produtos
{
    [TestClass]
    public class ListarProdutoUseCaseTests
    {
        private readonly Mock<IProdutoRepository> _repository = new();
        private readonly Mock<IMapper> _mapper = new();

        [TestMethod]
        [DataRow(0, 20)]
        [DataRow(-1, 20)]
        [DataRow(1, 0)]
        [DataRow(1, -1)]
        [DataRow(1, 101)]
        [DataRow(1, int.MaxValue)]
        [DataRow(int.MaxValue, 100)]
        public async Task Deve_Rejeitar_Paginacao_Invalida_Sem_Consultar_Repositorio(int pagina, int tamanhoPagina)
        {
            var useCase = new ListarProdutoUseCase(_mapper.Object, _repository.Object);

            await Assert.ThrowsExceptionAsync<BadRequestException>(() =>
                useCase.Executar(pagina, tamanhoPagina));

            _repository.Verify(r => r.Listar(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [TestMethod]
        [DataRow(1, 1)]
        [DataRow(2, 100)]
        public async Task Deve_Listar_Produtos_Quando_Paginacao_For_Valida(int pagina, int tamanhoPagina)
        {
            var produtos = new List<Produto> { new("Mouse", "Redragon", 10, 20m) };
            var respostaEsperada = new List<ProdutoResponseDTO> { new() { Nome = "Mouse" } };
            _repository.Setup(r => r.Listar(pagina, tamanhoPagina)).ReturnsAsync(produtos);
            _mapper.Setup(m => m.Map<List<ProdutoResponseDTO>>(produtos)).Returns(respostaEsperada);
            var useCase = new ListarProdutoUseCase(_mapper.Object, _repository.Object);

            var resposta = await useCase.Executar(pagina, tamanhoPagina);

            Assert.AreSame(respostaEsperada, resposta);
            _repository.Verify(r => r.Listar(pagina, tamanhoPagina), Times.Once);
        }
    }
}
