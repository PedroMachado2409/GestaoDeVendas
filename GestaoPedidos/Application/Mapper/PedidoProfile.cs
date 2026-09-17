using AutoMapper;
using GestaoPedidos.Application.DTO.Pedidos;
using GestaoPedidos.Domain.Entities.Pedidos;

namespace GestaoPedidos.Application.Mapper
{
    public class PedidoProfile : Profile
    {
        public PedidoProfile()
        {
            CreateMap<PedidoItem, PedidoItemResponseDTO>()
               .ForMember(dest => dest.SubTotal,
                          opt => opt.MapFrom(src => src.SubTotal));

            CreateMap<Pedido, PedidoResponseDTO>()
                .ForMember(dest => dest.Status,
                           opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.ValorTotal,
                           opt => opt.MapFrom(src => src.ValorTotal))
                .ForMember(dest => dest.ClienteNome,
                             opt => opt.MapFrom(src => src.Cliente != null ? src.Cliente.Nome : string.Empty))
                    .ForMember(dest => dest.UsuarioNome,
                        opt => opt.MapFrom(src => src.Usuario != null ? src.Usuario.Nome : string.Empty));

        }
    }
}
