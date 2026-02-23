using AutoMapper;
using GestaoPedidos.Application.DTO.Pedidos;
using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Exceptions;
using GestaoPedidos.Domain.Exceptions.Pedidos;

namespace GestaoPedidos.Application.UseCases.Pedidos.Queries
{
    public class ObterPedidoPorIdUseCase
    {
        private readonly IMapper _mapper;
        private readonly IPedidoRepository _repository;

        public ObterPedidoPorIdUseCase (IPedidoRepository repository, IMapper mapper)
        {
            _mapper = mapper;
            _repository = repository;
        }

        public async Task <PedidoResponseDTO> Executar(int id)
        {
            var pedido = await _repository.ObterPorId(id);
            if (pedido == null)
                throw new BadRequestException(PedidosExceptions.Pedido_NaoEncontrado);

            return _mapper.Map<PedidoResponseDTO>(pedido);  

        }


    }
}
