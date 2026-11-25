using AutoMapper;
using GestaoPedidos.Application.DTO.Produtos;
using GestaoPedidos.Application.UseCases.Usuarios.Queries;
using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Entities;
using GestaoPedidos.Domain.Enum;
using GestaoPedidos.Domain.Exceptions;
using GestaoPedidos.Domain.Exceptions.Produtos;

namespace GestaoPedidos.Application.UseCases.Produtos.Commands
{
    public class AtualizarProdutoUseCase
    {
        private readonly IMapper _mapper;
        private readonly IProdutoRepository _repository;
        private readonly IMovimentacaoEstoqueRepository _movimentacaoRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ObterUsuarioAutenticadoUseCase _obterUsuarioAutenticado;

        public AtualizarProdutoUseCase(
            IMapper mapper,
            IProdutoRepository repository,
            IMovimentacaoEstoqueRepository movimentacaoRepository,
            IUnitOfWork unitOfWork,
            ObterUsuarioAutenticadoUseCase obterUsuarioAutenticado)
        {
            _mapper = mapper;
            _repository = repository;
            _movimentacaoRepository = movimentacaoRepository;
            _unitOfWork = unitOfWork;
            _obterUsuarioAutenticado = obterUsuarioAutenticado;
        }

        public Task<ProdutoResponseDTO> Executar(ProdutoUpdateDTO dto)
            => _unitOfWork.ExecutarEmTransacao(async () =>
        {
            var produto = await _repository.ObterPorId(dto.Id);
            if (produto == null)
            {
                throw new NotFoundException(ProdutoExceptions.Produto_NaoEncontrado);
            }

            var usuario = await _obterUsuarioAutenticado.Executar();
            if (usuario.Role != UserRole.Admin)
            {
                throw new ForbiddenException("Somente um administrador pode atualizar produtos.");
            }

            var estoqueAnterior = produto.Estoque;
            produto.Atualizar(dto.Nome, dto.Marca, dto.Estoque, dto.Preco);

            var diferencaEstoque = produto.Estoque - estoqueAnterior;
            if (diferencaEstoque != 0)
            {
                var movimentacao = new MovimentacaoEstoque(
                    produto.Id, produto.Nome, Math.Abs(diferencaEstoque), usuario.Id,
                    OrigemMovimentacao.AjusteManual,
                    diferencaEstoque > 0 ? TipoMovimentacao.Entrada : TipoMovimentacao.Saida,
                    "Ajuste de estoque na atualização do produto.");

                await _movimentacaoRepository.Cadastrar(movimentacao);
            }

            await _repository.Atualizar(produto);
            await _unitOfWork.SalvarAlteracoes();

            return _mapper.Map<ProdutoResponseDTO>(produto);
        });

    }
}
