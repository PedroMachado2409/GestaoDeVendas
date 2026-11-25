using System.Security.Claims;
using AutoMapper;
using GestaoPedidos.Application.DTO.Produtos;
using GestaoPedidos.Application.DTO.Usuarios;
using GestaoPedidos.Application.UseCases.Produtos.Commands;
using GestaoPedidos.Application.UseCases.Usuarios.Queries;
using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Abstractions.Usuarios;
using GestaoPedidos.Domain.Entities;
using GestaoPedidos.Domain.Enum;
using GestaoPedidos.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Moq;

namespace GestaoPedidosTests.Application.Produtos
{
    [TestClass]
    public class AtualizarProdutoUseCaseTests
    {
        private readonly Produto _produto = new(nome: "Mouse", marca: "Redragon", estoque: 10, preco: 20m);
        private readonly Mock<IProdutoRepository> _repository = new();
        private readonly Mock<IMovimentacaoEstoqueRepository> _movimentacoes = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly Mock<IMapper> _mapper = new();
        private ObterUsuarioAutenticadoUseCase _usuarioAutenticado = null!;

        [TestInitialize]
        public void Preparar_Usuario_E_Transacao()
        {
            var usuarios = new Mock<IUsuarioRepository>();
            var usuario = new Usuario("Pedro", "pedro@exemplo.com", "hash", UserRole.Admin);
            usuarios.Setup(r => r.ObterPorId(1)).ReturnsAsync(usuario);
            _mapper.Setup(m => m.Map<UsuarioDTO>(usuario)).Returns(new UsuarioDTO { Id = 1, Role = UserRole.Admin });
            var contexto = new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }))
                }
            };
            _usuarioAutenticado = new ObterUsuarioAutenticadoUseCase(usuarios.Object, contexto, _mapper.Object);
            _unitOfWork.Setup(u => u.ExecutarEmTransacao(It.IsAny<Func<Task<ProdutoResponseDTO>>>()))
                .Returns((Func<Task<ProdutoResponseDTO>> operacao) => operacao());
        }

        [TestMethod]
        [DataRow(12, TipoMovimentacao.Entrada)]
        [DataRow(8, TipoMovimentacao.Saida)]
        public async Task Deve_Salvar_Produto_E_Movimentacao_Quando_Estoque_For_Alterado(int estoque, TipoMovimentacao tipo)
        {
            var dto = new ProdutoUpdateDTO { Id = 1, Nome = "Mouse novo", Marca = "Redragon", Estoque = estoque, Preco = 25m };
            var respostaEsperada = new ProdutoResponseDTO { Id = 1, Nome = dto.Nome, Estoque = estoque, Preco = dto.Preco };
            _repository.Setup(r => r.ObterPorId(dto.Id)).ReturnsAsync(_produto);
            _mapper.Setup(m => m.Map<ProdutoResponseDTO>(_produto)).Returns(respostaEsperada);
            var useCase = new AtualizarProdutoUseCase(_mapper.Object, _repository.Object,
                _movimentacoes.Object, _unitOfWork.Object, _usuarioAutenticado);

            var resposta = await useCase.Executar(dto);

            Assert.AreSame(respostaEsperada, resposta);
            Assert.AreEqual(dto.Nome, _produto.Nome);
            Assert.AreEqual(dto.Estoque, _produto.Estoque);
            _repository.Verify(r => r.Atualizar(_produto), Times.Once);
            _movimentacoes.Verify(r => r.Cadastrar(It.Is<MovimentacaoEstoque>(m =>
                m.Quantidade == 2 && m.TipoMovimentacao == tipo && m.IdOrigem == 1
                && m.OrigemMovimentacao == OrigemMovimentacao.AjusteManual)), Times.Once);
            _unitOfWork.Verify(u => u.SalvarAlteracoes(), Times.Once);
        }

        [TestMethod]
        public async Task Deve_Salvar_Sem_Movimentacao_Quando_Estoque_Nao_For_Alterado()
        {
            var dto = new ProdutoUpdateDTO { Id = 1, Nome = "Mouse novo", Marca = "Redragon", Estoque = 10, Preco = 25m };
            _repository.Setup(r => r.ObterPorId(dto.Id)).ReturnsAsync(_produto);
            var useCase = new AtualizarProdutoUseCase(_mapper.Object, _repository.Object,
                _movimentacoes.Object, _unitOfWork.Object, _usuarioAutenticado);

            await useCase.Executar(dto);

            _movimentacoes.Verify(r => r.Cadastrar(It.IsAny<MovimentacaoEstoque>()), Times.Never);
            _unitOfWork.Verify(u => u.SalvarAlteracoes(), Times.Once);
        }

        [TestMethod]
        public async Task Deve_Lancar_Excecao_Sem_Salvar_Quando_Produto_Nao_Existir()
        {
            _repository.Setup(r => r.ObterPorId(999)).ReturnsAsync((Produto?)null);
            var useCase = new AtualizarProdutoUseCase(_mapper.Object, _repository.Object,
                _movimentacoes.Object, _unitOfWork.Object, _usuarioAutenticado);

            await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
                useCase.Executar(new ProdutoUpdateDTO { Id = 999 }));

            _repository.Verify(r => r.Atualizar(It.IsAny<Produto>()), Times.Never);
            _movimentacoes.Verify(r => r.Cadastrar(It.IsAny<MovimentacaoEstoque>()), Times.Never);
            _unitOfWork.Verify(u => u.SalvarAlteracoes(), Times.Never);
        }
    }
}
