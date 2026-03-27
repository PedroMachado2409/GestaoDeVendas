using AutoMapper;
using GestaoPedidos.Application.DTO.Clientes;
using GestaoPedidos.Domain.Entities;

namespace GestaoPedidos.Application.Mapper
{
    public class ClienteProfile : Profile
    {
       public ClienteProfile()
        {
            CreateMap<Cliente, ClienteResponseDTO>().ReverseMap();
            CreateMap<Cliente, ClienteCreateDTO>().ReverseMap();
            CreateMap<Cliente, ClienteUpdateDTO>().ReverseMap();
        }
    }
}
