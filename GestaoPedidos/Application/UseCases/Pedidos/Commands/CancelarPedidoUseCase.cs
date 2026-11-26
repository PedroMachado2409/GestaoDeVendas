using AutoMapper;
using GestaoPedidos.Application.DTO.Pedidos;
using GestaoPedidos.Application.UseCases.Usuarios.Queries;
using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Enum;
using GestaoPedidos.Domain.Exceptions;
using GestaoPedidos.Domain.Exceptions.Pedidos;
using GestaoPedidos.Domain.Exceptions.Produtos;

namespace GestaoPedidos.Application.UseCases.Pedidos.Commands
{
    public class CancelarPedidoUseCase
    {
        private readonly IMapper _mapper;
        private readonly IPedidoRepository _pedidoRepository;
        private readonly IProdutoRepository _produtoRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITituloFinanceiroRepository _tituloFinanceiroRepository;
        private readonly ObterUsuarioAutenticadoUseCase _obterUsuarioAutenticadoUseCase;

        public CancelarPedidoUseCase(
            IMapper mapper,
            IPedidoRepository pedidoRepository,
            IProdutoRepository produtoRepository,
            IUnitOfWork unitOfWork,
            ITituloFinanceiroRepository tituloFinanceiroRepository,
            ObterUsuarioAutenticadoUseCase obterUsuarioAutenticadoUseCase)
        {
            _mapper = mapper;
            _pedidoRepository = pedidoRepository;
            _produtoRepository = produtoRepository;
            _unitOfWork = unitOfWork;
            _obterUsuarioAutenticadoUseCase = obterUsuarioAutenticadoUseCase;
            _tituloFinanceiroRepository = tituloFinanceiroRepository;
        }

        public Task<PedidoResponseDTO> Executar(int pedidoId)
            => _unitOfWork.ExecutarEmTransacao(async () =>
            {
                var usuario = await _obterUsuarioAutenticadoUseCase.Executar();
                var pedido = await _pedidoRepository.ObterPorId(pedidoId);
                var titulo = await _tituloFinanceiroRepository.ObterPorOrigem(pedido?.Id, OrigemTitulo.Pedido);
                
                if (pedido is null || (pedido.UsuarioId != usuario.Id && usuario.Role != UserRole.Admin))
                {
                    throw new NotFoundException(PedidosExceptions.Pedido_NaoEncontrado);
                }

                if (pedido.TipoMovimentacao == TipoMovimentacao.Entrada && usuario.Role != UserRole.Admin)
                {
                    throw new ForbiddenException("Somente um administrador pode cancelar pedidos");
                }

                if (pedido.Status == StatusPedido.Cancelado)
                {
                    return _mapper.Map<PedidoResponseDTO>(pedido);
                }

                if (pedido.Status != StatusPedido.Aberto)
                {
                    throw new ConflictException(PedidosExceptions.Pedido_NaoPodeCancelar);
                }

                var produtos = await _produtoRepository.ObterPorIds(
                    pedido.Itens.Select(i => i.ProdutoId));
                var produtosPorId = produtos.ToDictionary(p => p.Id);

                foreach (var item in pedido.Itens)
                {
                    if (!produtosPorId.TryGetValue(item.ProdutoId, out var produto))
                    {
                        throw new NotFoundException(ProdutoExceptions.Produto_NaoEncontrado);
                    }

                    if (pedido.TipoMovimentacao == TipoMovimentacao.Saida)
                    {
                        produto.CancelarReserva(item.Quantidade);
                    }
                    else
                    {
                        produto.CancelarCompraPendente(item.Quantidade);
                    }
                }
                if(titulo is not null)
                {
                    titulo.Cancelar();
                    await _tituloFinanceiroRepository.Atualizar(titulo);
                }

                
                pedido.Cancelar();
                await _pedidoRepository.Atualizar(pedido);
                await _unitOfWork.SalvarAlteracoes();

                return _mapper.Map<PedidoResponseDTO>(pedido);
            });
    }
}
