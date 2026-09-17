using AutoMapper;
using GestaoPedidos.Application.DTO.Pedidos;
using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Exceptions;
using GestaoPedidos.Domain.Exceptions.Pedidos;

namespace GestaoPedidos.Application.UseCases.Pedidos.Queries
{
    public class ListarPedidosUseCase
    {
        private readonly IPedidoRepository _pedidoRepository;
        private readonly IMapper _mapper;

        public ListarPedidosUseCase (IPedidoRepository pedidoRepository, IMapper mapper)
        {
            _pedidoRepository = pedidoRepository;
            _mapper = mapper;
        }

        public async Task <List<PedidoResponseDTO>> Executar()
        {
            var pedidos = await _pedidoRepository.Listar();
            if (pedidos == null)
            {
                throw new NotFoundException(PedidosExceptions.Pedido_NaoEncontrado);
            }

            return _mapper.Map<List<PedidoResponseDTO>>(pedidos);
        }

    }
}
