using AutoMapper;
using GestaoPedidos.Application.DTO.Pedidos;
using GestaoPedidos.Application.UseCases.Usuarios.Queries;
using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Enum;
using GestaoPedidos.Domain.Exceptions;
using GestaoPedidos.Domain.Exceptions.Pedidos;

namespace GestaoPedidos.Application.UseCases.Pedidos.Queries
{
    public class ObterPedidoPorIdUseCase
    {
        private readonly IMapper _mapper;
        private readonly IPedidoRepository _repository;
        private readonly ObterUsuarioAutenticadoUseCase _usuarioAutenticadoUseCase;
        public ObterPedidoPorIdUseCase(IPedidoRepository repository, IMapper mapper, ObterUsuarioAutenticadoUseCase obterUsuarioAutenticadoUseCase)
        {
            _mapper = mapper;
            _repository = repository;
            _usuarioAutenticadoUseCase = obterUsuarioAutenticadoUseCase;
        }

        public async Task<PedidoResponseDTO> Executar(int id)
        {
            var usuario = await _usuarioAutenticadoUseCase.Executar();

            var pedido = await _repository.ObterPorId(id);
            if (pedido?.UsuarioId != usuario.Id && usuario.Role != UserRole.Admin || pedido == null)
            {
                throw new NotFoundException(PedidosExceptions.Pedido_NaoEncontrado);
            }

            return _mapper.Map<PedidoResponseDTO>(pedido);
        }
    }
}
