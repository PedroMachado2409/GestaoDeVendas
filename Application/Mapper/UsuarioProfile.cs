using AutoMapper;
using GestaoPedidos.Application.DTO.Usuarios;
using GestaoPedidos.Domain.Entities;

namespace GestaoPedidos.Application.Mapper
{
    public class UsuarioProfile : Profile
    {
        public UsuarioProfile()
        {
            CreateMap<Usuario, UsuarioCreateDTO>().ReverseMap();
            
            CreateMap<Usuario, UsuarioDTO>().ReverseMap();
        }
    }
}
