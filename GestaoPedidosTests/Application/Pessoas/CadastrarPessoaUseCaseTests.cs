using AutoMapper;
using GestaoPedidos.Application.DTO.Pessoas;
using GestaoPedidos.Application.UseCases.Pessoas.Commands;
using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Entities;
using GestaoPedidos.Domain.Enum;
using GestaoPedidos.Domain.Exceptions;
using GestaoPedidos.Domain.Exceptions.Pessoas;
using Microsoft.VisualBasic;
using Moq;


namespace GestaoPedidosTests.Application.Pessoas
{
    [TestClass]
    public class CadastrarPessoaUseCaseTests
    {
        private readonly Pessoa _pessoa = new(
            nome: "Pedro",
            email: "pedro@exemplo.com",
            cpf: "52998224725",
            tipoPessoa: TipoPessoa.Cliente
        );

        private readonly Mock<IPessoaRepository> _repository = new();
        private readonly Mock<IMapper> _mapper = new();

        [TestMethod]
        public async Task Deve_Lancar_Excecao_E_Nao_Cadastrar_Pessoa_Quando_Cpf_Ja_Existir()
        {
            var dto = new PessoaCreateDTO
            {Nome = "Maria", Email = "maria@exemplo.com", Cpf = _pessoa.Cpf, TipoPessoa = TipoPessoa.Cliente};

            _repository.Setup(r => r.ObterPorCpf(dto.Cpf)).ReturnsAsync(_pessoa);

            var useCase = new CadastrarPessoaUseCase(_repository.Object,_mapper.Object);

            var excecao = await Assert.ThrowsExceptionAsync<BadRequestException>(
                () => useCase.Executar(dto));

            Assert.AreEqual(PessoasExceptions.Pessoa_CpfExistente, excecao.Message);

            _repository.Verify(r => r.Cadastrar(It.IsAny<Pessoa>()),Times.Never);
        }

        [TestMethod]
        public async Task Deve_Lancar_Excecao_E_Nao_Cadastrar_Pessoa_Quando_Email_Ja_Existir()
        {
            var dto = new PessoaCreateDTO
            { Nome = "Maria", Email = _pessoa.Email, Cpf = "12345678910", TipoPessoa = TipoPessoa.Cliente };

            _repository.Setup(r => r.ObterPorEmail(dto.Email)).ReturnsAsync(_pessoa);

            var useCase = new CadastrarPessoaUseCase(_repository.Object, _mapper.Object);

            var excecao = await Assert.ThrowsExceptionAsync<BadRequestException>(
                () => useCase.Executar(dto));

            Assert.AreEqual(PessoasExceptions.Pessoa_EmailExistente, excecao.Message);

            _repository.Verify(r => r.Cadastrar(It.IsAny<Pessoa>()), Times.Never);
        }

        [TestMethod]
        public async Task Cadastrar_Pessoa_Com_Dados_Validos_Deve_Cadastrar_Corretamente()
        {
            var dto = new PessoaCreateDTO { Nome = _pessoa.Nome, Email = _pessoa.Email, Cpf = _pessoa.Cpf, TipoPessoa = TipoPessoa.Cliente };
            var respostaEsperada = new PessoaResponseDTO
            { Nome = _pessoa.Nome, Email = _pessoa.Email, Cpf = _pessoa.Cpf, TipoPessoa = TipoPessoa.Cliente };

            _repository.Setup(r => r.ObterPorCpf(dto.Cpf)).ReturnsAsync((Pessoa?)null);
            _repository.Setup(r => r.ObterPorEmail(dto.Email)).ReturnsAsync((Pessoa?)null);
            _repository.Setup(r => r.Cadastrar(It.IsAny<Pessoa>())).Returns(Task.CompletedTask);
            _mapper.Setup(m => m.Map<PessoaResponseDTO>(It.IsAny<object>())).Returns(respostaEsperada);

            var useCase = new CadastrarPessoaUseCase(_repository.Object, _mapper.Object);

            var resposta = await useCase.Executar(dto);

            Assert.AreSame(respostaEsperada, resposta);
            _repository.Verify(r => r.Cadastrar(It.IsAny<Pessoa>()), Times.Once());

            _repository.Verify(r => r.Cadastrar(It.Is<Pessoa>(p => p.Nome == dto.Nome && p.Email == dto.Email
                 && p.Cpf == dto.Cpf && p.TipoPessoa == dto.TipoPessoa && p.Ativo)),Times.Once);

        }
    }
}
