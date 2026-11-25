using AutoMapper;
using GestaoPedidos.Application.DTO.Pessoas;
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
    public class AtualizarPessoaUseCaseTests
    {
        private readonly Mock<IPessoaRepository> _repository = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly Mock<IMapper> _mapper = new();

        private readonly Pessoa _pessoa = new(nome: "Pedro", email: "pedro@exemplo.com",
            cpf: "52998224725", tipoPessoa: TipoPessoa.Cliente);

        private readonly Pessoa _outraPessoa = new(nome: "Maria", email: "maria@exemplo.com",
            cpf: "11144477735", tipoPessoa: TipoPessoa.Cliente);

        [TestInitialize]
        public void Preparar_Pessoas()
        {
            typeof(Pessoa).GetProperty(nameof(Pessoa.Id))!.SetValue(_pessoa, 1);
            typeof(Pessoa).GetProperty(nameof(Pessoa.Id))!.SetValue(_outraPessoa, 2);
        }


        [TestMethod]
        public async Task Deve_Atualizar_Pessoa_Quando_Dados_Forem_Validos()
        {
            var id = 1;
            var dto = new PessoaUpdateDTO
            {
                Id = id,
                Nome = "Pedro",
                Cpf = "52998224999",
                Email = "pedro@gmail.com",
                TipoPessoa = TipoPessoa.Cliente
            };
            var respostaEsperada = new PessoaResponseDTO
            {
                Id = dto.Id,
                Nome = dto.Nome,
                Cpf = dto.Cpf,
                Email = dto.Email,
                TipoPessoa = dto.TipoPessoa
            };

            _repository.Setup(r => r.ObterPorId(id)).ReturnsAsync(_pessoa);
            _repository.Setup(r => r.ObterPorCpf(dto.Cpf)).ReturnsAsync((Pessoa?)null);
            _repository.Setup(r => r.ObterPorEmail(dto.Email)).ReturnsAsync((Pessoa?)null);

            _mapper.Setup(m => m.Map<PessoaResponseDTO>(_pessoa)).Returns(respostaEsperada);
            _repository.Setup(r => r.Atualizar(_pessoa)).Returns(Task.CompletedTask);

            var useCase = new AtualizarPessoaUseCase(_repository.Object, _unitOfWork.Object, _mapper.Object);
            var resposta = await useCase.Executar(dto);

            Assert.AreSame(respostaEsperada, resposta);

            _repository.Verify(r => r.ObterPorId(id), Times.Once());
            _repository.Verify(r => r.ObterPorCpf(dto.Cpf), Times.Once());
            _repository.Verify(r => r.ObterPorEmail(dto.Email), Times.Once());
            _repository.Verify(r => r.Atualizar(_pessoa), Times.Once());
            _unitOfWork.Verify(u => u.SalvarAlteracoes(), Times.Once);

            Assert.AreEqual(dto.Nome, _pessoa.Nome);
            Assert.AreEqual(dto.Cpf, _pessoa.Cpf);
            Assert.AreEqual(dto.Email, _pessoa.Email);
            Assert.AreEqual(dto.TipoPessoa, _pessoa.TipoPessoa);
            Assert.IsTrue(_pessoa.Ativo);
        }

        [TestMethod]
        public async Task Deve_Lancar_Excecao_Quando_Cpf_Ja_Existir()
        {
            var id = 1;

            var dto = new PessoaUpdateDTO
            {
                Id = id,
                Nome = "Pedro",
                Cpf = _outraPessoa.Cpf,
                Email = "pedro@gmail.com",
                TipoPessoa = TipoPessoa.Cliente
            };

            _repository.Setup(r => r.ObterPorId(id)).ReturnsAsync(_pessoa);
            _repository.Setup(r => r.ObterPorCpf(dto.Cpf)).ReturnsAsync(_outraPessoa);

            var useCase = new AtualizarPessoaUseCase(_repository.Object, _unitOfWork.Object, _mapper.Object);

            var excecao = await Assert.ThrowsExceptionAsync<BadRequestException>(() =>
                useCase.Executar(dto));

            Assert.AreEqual(PessoasExceptions.Pessoa_CpfExistente, excecao.Message);
            Assert.AreEqual("52998224725", _pessoa.Cpf);

            _repository.Verify(r => r.Atualizar(It.IsAny<Pessoa>()), Times.Never);
            _unitOfWork.Verify(u => u.SalvarAlteracoes(), Times.Never);
            _mapper.Verify(m => m.Map<PessoaResponseDTO>(It.IsAny<object>()), Times.Never);
        }

        [TestMethod]
        public async Task Deve_Lancar_Excecao_Quando_Email_Ja_Existir()
        {
            var id = 1;
            var dto = new PessoaUpdateDTO
            {
                Id = id,
                Nome = "Pedro",
                Cpf = "12345678910",
                Email = _outraPessoa.Email,
                TipoPessoa = TipoPessoa.Cliente
            };

            _repository.Setup(r => r.ObterPorId(id)).ReturnsAsync(_pessoa);
            _repository.Setup(r => r.ObterPorCpf(dto.Cpf)).ReturnsAsync((Pessoa?)null);
            _repository.Setup(r => r.ObterPorEmail(dto.Email)).ReturnsAsync(_outraPessoa);

            var useCase = new AtualizarPessoaUseCase(_repository.Object, _unitOfWork.Object, _mapper.Object);
            var excecao = await Assert.ThrowsExceptionAsync<BadRequestException>(() =>
                useCase.Executar(dto));

            Assert.AreEqual(PessoasExceptions.Pessoa_EmailExistente, excecao.Message);
            Assert.AreEqual("pedro@exemplo.com", _pessoa.Email);

            _repository.Verify(r => r.ObterPorId(id), Times.Once());
            _repository.Verify(r => r.ObterPorCpf(dto.Cpf), Times.Once());
            _repository.Verify(r => r.ObterPorEmail(dto.Email), Times.Once());
            _repository.Verify(r => r.Atualizar(It.IsAny<Pessoa>()), Times.Never);
            _unitOfWork.Verify(u => u.SalvarAlteracoes(), Times.Never);
            _mapper.Verify(m => m.Map<PessoaResponseDTO>(It.IsAny<object>()), Times.Never);
        }

        [TestMethod]
        public async Task Deve_Lancar_Excecao_Quando_Pessoa_Nao_Existir()
        {
            var id = 1;
            var dto = new PessoaUpdateDTO
            {
                Id = id,
                Nome = "Pedro",
                Cpf = _outraPessoa.Cpf,
                Email = "pedro@gmail.com",
                TipoPessoa = TipoPessoa.Cliente
            };

            _repository.Setup(r => r.ObterPorId(id)).ReturnsAsync((Pessoa?)null);
            var useCase = new AtualizarPessoaUseCase(_repository.Object, _unitOfWork.Object, _mapper.Object);
            var excecao = await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
                useCase.Executar(dto));

            Assert.AreEqual(PessoasExceptions.Pessoa_NaoEncontrado, excecao.Message);

            _repository.Verify(r => r.ObterPorId(id), Times.Once);
            _repository.Verify(r => r.Atualizar(It.IsAny<Pessoa>()), Times.Never);
            _unitOfWork.Verify(u => u.SalvarAlteracoes(), Times.Never);
            _mapper.Verify(m => m.Map<PessoaResponseDTO>(It.IsAny<object>()), Times.Never);
        }


    }
}
