using AutoMapper;
using GestaoPedidos.Application.DTO.Pedidos;
using GestaoPedidos.Domain.Entities.Pedidos;

namespace GestaoPedidos.Application.Mapper
{
    public class PedidoProfile : Profile
    {
        public PedidoProfile ()
        {
            CreateMap<Pedido, CriarPedidoRequestDTO>().ReverseMap();
            CreateMap<PedidoItem, CriarPedidoItemRequestDTO>().ReverseMap();
            CreateMap<Pedido, PedidoResponseDTO>().ReverseMap();
            CreateMap<PedidoItem, PedidoItemResponseDTO>()
               .ForMember(dest => dest.SubTotal,
                          opt => opt.MapFrom(src => src.SubTotal));

            CreateMap<Pedido, PedidoResponseDTO>()
                .ForMember(dest => dest.Status,
                           opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.ValorTotal,
                           opt => opt.MapFrom(src => src.ValorTotal))
                .ForMember(dest => dest.Itens,
                           opt => opt.MapFrom(src => src.Itens));
        }
    }
}
