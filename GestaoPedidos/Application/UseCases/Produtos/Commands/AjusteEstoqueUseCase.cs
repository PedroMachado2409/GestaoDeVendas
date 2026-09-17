using AutoMapper;
using GestaoPedidos.Application.DTO.Produtos;
using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Entities;
using GestaoPedidos.Domain.Exceptions;
using GestaoPedidos.Domain.Exceptions.Produtos;
using GestaoPedidos.Domain.Enum;
using GestaoPedidos.Application.UseCases.Usuarios.Queries;

namespace GestaoPedidos.Application.UseCases.Produtos.Commands
{
    public class AjusteEstoqueUseCase
    {
        private readonly IProdutoRepository _produtoRepository;
        private readonly IMapper _mapper;
        private readonly IMovimentacaoEstoqueRepository _movimentacaoEstoqueRepository;
        private readonly ObterUsuarioAutenticadoUseCase _obterUsuarioAutenticadoUseCase;
        private readonly IUnitOfWork _unitOfWork;

        public AjusteEstoqueUseCase (IProdutoRepository produtoRepository, IMapper mapper, IMovimentacaoEstoqueRepository movimentacaoEstoqueRepository, IUnitOfWork unitOfWork, ObterUsuarioAutenticadoUseCase obterUsuarioAutenticadoUseCase)
        {
            _produtoRepository = produtoRepository;
            _mapper = mapper;
            _movimentacaoEstoqueRepository = movimentacaoEstoqueRepository;
            _unitOfWork = unitOfWork;
            _obterUsuarioAutenticadoUseCase = obterUsuarioAutenticadoUseCase;
        }

        public Task<ProdutoResponseDTO> Executar(AjusteEstoqueProdutoDTO dto)
            => _unitOfWork.ExecutarEmTransacao(async () =>
        {
            var produto = await _produtoRepository.ObterPorId(dto.Id);
            if (produto == null)
            {
                throw new NotFoundException(ProdutoExceptions.Produto_NaoEncontrado);
            }

            var usuario = await _obterUsuarioAutenticadoUseCase.Executar();

            var tipo = dto.Quantidade > 0
                ? TipoMovimentacao.Entrada
                : TipoMovimentacao.Saida;

            var novaMovimentacaoEstoque = new MovimentacaoEstoque(
                produto.Id,
                produto.Nome,
                Math.Abs(dto.Quantidade),
                usuario.Id,
                OrigemMovimentacao.AjusteManual,
                tipo,
                "Ajuste manual de estoque");

            produto.AjusteEstoque(dto.Quantidade);
            await _movimentacaoEstoqueRepository.Cadastrar(novaMovimentacaoEstoque);
            await _unitOfWork.SalvarAlteracoes();

            return _mapper.Map<ProdutoResponseDTO>(produto);

        });

    }
}
