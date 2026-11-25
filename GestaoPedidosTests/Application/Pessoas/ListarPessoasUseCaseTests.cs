using AutoMapper;
using GestaoPedidos.Application.DTO.Pessoas;
using GestaoPedidos.Application.UseCases.Pessoas.Queries;
using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Entities;
using GestaoPedidos.Domain.Enum;
using GestaoPedidos.Domain.Exceptions;
using Moq;

namespace GestaoPedidosTests.Application.Pessoas
{
    [TestClass]
    public class ListarPessoasUseCaseTests
    {
        private readonly Mock<IPessoaRepository> _repository = new();
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
            var useCase = new ListarPessoasUseCase(_repository.Object, _mapper.Object);

            await Assert.ThrowsExceptionAsync<BadRequestException>(() =>
                useCase.Executar(pagina, tamanhoPagina));

            _repository.Verify(r => r.Listar(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [TestMethod]
        [DataRow(1, 1)]
        [DataRow(2, 100)]
        public async Task Deve_Listar_Pessoas_Quando_Paginacao_For_Valida(int pagina, int tamanhoPagina)
        {
            var pessoas = new List<Pessoa> { new("Pedro", "pedro@exemplo.com", "52998224725", TipoPessoa.Cliente) };
            var respostaEsperada = new List<PessoaResponseDTO> { new() { Nome = "Pedro" } };
            _repository.Setup(r => r.Listar(pagina, tamanhoPagina)).ReturnsAsync(pessoas);
            _mapper.Setup(m => m.Map<List<PessoaResponseDTO>>(pessoas)).Returns(respostaEsperada);
            var useCase = new ListarPessoasUseCase(_repository.Object, _mapper.Object);

            var resposta = await useCase.Executar(pagina, tamanhoPagina);

            Assert.AreSame(respostaEsperada, resposta);
            _repository.Verify(r => r.Listar(pagina, tamanhoPagina), Times.Once);
        }
    }
}
