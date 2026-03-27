using AutoMapper;
using GestaoPedidos.Application.DTO.Titulos;
using GestaoPedidos.Domain.Entities;

namespace GestaoPedidos.Application.Mapper
{
    public class TitulosProfile : Profile
    {
        public TitulosProfile()
        {
            CreateMap<Titulo, CreateTituloDTO>().ReverseMap();
            CreateMap<Titulo, TituloResponseDTO>().ReverseMap();
        }
    }
}
