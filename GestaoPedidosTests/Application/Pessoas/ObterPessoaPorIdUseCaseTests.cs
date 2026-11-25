

using AutoMapper;
using GestaoPedidos.Application.DTO.Pessoas;
using GestaoPedidos.Application.UseCases.Pessoas.Queries;
using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Entities;
using GestaoPedidos.Domain.Enum;
using GestaoPedidos.Domain.Exceptions;
using GestaoPedidos.Domain.Exceptions.Pessoas;
using Moq;

namespace GestaoPedidosTests.Application.Pessoas
{
    [TestClass]
    public class ObterPessoaPorIdUseCaseTests
    {
        private readonly Mock<IPessoaRepository> _repository = new();
        private readonly Mock<IMapper> _mapper = new();

        private readonly Pessoa _pessoa = new(nome: "Pedro", email: "pedro@exemplo.com",
            cpf: "52998224725", tipoPessoa: TipoPessoa.Cliente);

        [TestMethod]
        public async Task Deve_Obter_Pessoa_Pelo_Id_Corretamente()
        {
            var id = 1;

            var respostaEsperada = new PessoaResponseDTO
            {
                Id = id,
                Nome = _pessoa.Nome,
                Cpf = _pessoa.Cpf,
                Email = _pessoa.Email,
                Ativo = _pessoa.Ativo,
                DataCadastro =
                _pessoa.DataCadastro,
                TipoPessoa = _pessoa.TipoPessoa
            };

            _repository.Setup(r => r.ObterPorId(id)).ReturnsAsync(_pessoa);
            _mapper.Setup(m => m.Map<PessoaResponseDTO>(_pessoa)).Returns(respostaEsperada);

            var useCase = new ObterPessoaPorIdUseCase(_repository.Object, _mapper.Object);
            var resposta = await useCase.Executar(id);

            Assert.AreSame(respostaEsperada, resposta);

            _repository.Verify(r => r.ObterPorId(id), Times.Once);
            _mapper.Verify(m => m.Map<PessoaResponseDTO>(_pessoa), Times.Once);

        }

        [TestMethod]
        public async Task Deve_Lancar_Excecao_Ao_Buscar_Id_Inexistente()
        {
            var id = 999;
            _repository.Setup(r => r.ObterPorId(id)).ReturnsAsync((Pessoa?)null);

            var useCase = new ObterPessoaPorIdUseCase(_repository.Object, _mapper.Object);
            var excecao = await Assert.ThrowsExceptionAsync<NotFoundException>(() => useCase.Executar(id));

            Assert.AreEqual(PessoasExceptions.Pessoa_NaoEncontrado, excecao.Message);

            _repository.Verify(r => r.ObterPorId(id), Times.Once);
            _mapper.Verify(m => m.Map<PessoaResponseDTO>(_pessoa), Times.Never);

        }
    }
}
