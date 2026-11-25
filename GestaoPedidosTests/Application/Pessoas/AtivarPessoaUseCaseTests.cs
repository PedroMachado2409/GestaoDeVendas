

using GestaoPedidos.Application.UseCases.Pessoas.Commands;
using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Entities;
using GestaoPedidos.Domain.Enum;
using GestaoPedidos.Domain.Exceptions;
using GestaoPedidos.Domain.Exceptions.Pessoas;
using Moq;

namespace GestaoPedidosTests.Application.Pessoas
{
    [TestClass]
    public class AtivarPessoaUseCaseTests
    {
        private readonly Pessoa _pessoa = new(nome: "Pedro", email: "pedro@exemplo.com",
            cpf: "52998224725", tipoPessoa: TipoPessoa.Cliente);

        private readonly Mock<IPessoaRepository> _repository = new();

        private readonly Mock<IUnitOfWork> _unitOfWork = new();

        [TestMethod]
        public async Task Deve_Ativar_A_Pessoa_Quando_Inativa()
        {
            var id = 1;

            _pessoa.Inativar();
            _repository.Setup(r => r.ObterPorId(id)).ReturnsAsync(_pessoa);
            _repository.Setup(r => r.Atualizar(_pessoa)).Returns(Task.CompletedTask);

            var useCase = new AtivarPessoaUseCase(_repository.Object, _unitOfWork.Object);

            await useCase.Executar(id);
            Assert.IsTrue(_pessoa.Ativo);

            _repository.Verify(r => r.ObterPorId(id), Times.Once);
            _repository.Verify(r => r.Atualizar(_pessoa), Times.Once);
            _unitOfWork.Verify(u => u.SalvarAlteracoes(), Times.Once);
        }

        [TestMethod]
        public async Task Deve_Lancar_Excecao_Quando_Nao_Localizar_A_Pessoa()
        {
            var id = 999;

            _repository.Setup(r => r.ObterPorId(id)).ReturnsAsync((Pessoa?)null);

            var useCase = new AtivarPessoaUseCase(_repository.Object, _unitOfWork.Object);

            var excecao = await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
                useCase.Executar(id));

            Assert.AreEqual(PessoasExceptions.Pessoa_NaoEncontrado, excecao.Message);

            _repository.Verify(r => r.ObterPorId(id), Times.Once);
            _repository.Verify(r => r.Atualizar(It.IsAny<Pessoa>()), Times.Never);
            _unitOfWork.Verify(u => u.SalvarAlteracoes(), Times.Never);
        }

        [TestMethod]
        public async Task Deve_Lancar_Excecao_Quando_Pessoa_Ja_Estiver_Ativa()
        {
            var id = 1;


            _repository.Setup(r => r.ObterPorId(id)).ReturnsAsync(_pessoa);

            var useCase = new AtivarPessoaUseCase(_repository.Object, _unitOfWork.Object);

            var excecao = await Assert.ThrowsExceptionAsync<BadRequestException>(() =>
                useCase.Executar(id));

            Assert.AreEqual(PessoasExceptions.Pessoa_JaAtivo, excecao.Message);
            Assert.IsTrue(_pessoa.Ativo);

            _repository.Verify(r => r.ObterPorId(id), Times.Once);
            _repository.Verify(r => r.Atualizar(It.IsAny<Pessoa>()), Times.Never);
            _unitOfWork.Verify(u => u.SalvarAlteracoes(), Times.Never);
        }
    }
}
