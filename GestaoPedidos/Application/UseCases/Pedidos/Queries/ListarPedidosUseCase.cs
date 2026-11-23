using AutoMapper;
using GestaoPedidos.Application.DTO.Pedidos;
using GestaoPedidos.Application.UseCases.Usuarios.Queries;
using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Enum;

namespace GestaoPedidos.Application.UseCases.Pedidos.Queries
{
    public class ListarPedidosUseCase
    {
        private readonly IPedidoRepository _pedidoRepository;
        private readonly ObterUsuarioAutenticadoUseCase _usuarioAutenticadoUseCase;
        private readonly IMapper _mapper;

        public ListarPedidosUseCase (IPedidoRepository pedidoRepository, IMapper mapper, ObterUsuarioAutenticadoUseCase usuarioAutenticadoUseCase)
        {
            _pedidoRepository = pedidoRepository;
            _mapper = mapper;
            _usuarioAutenticadoUseCase = usuarioAutenticadoUseCase;
        }

        public async Task <List<PedidoResponseDTO>> Executar()
        {
            var usuario = await _usuarioAutenticadoUseCase.Executar();

            var pedidos = usuario.Role == UserRole.Admin
            ? await _pedidoRepository.Listar()
            : await _pedidoRepository.ListarPorVendedor(usuario.Id);

            return _mapper.Map<List<PedidoResponseDTO>>(pedidos);
        }

    }
}

