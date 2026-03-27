using AutoMapper;
using GestaoPedidos.Application.DTO.Produtos;
using GestaoPedidos.Domain.Entities;

namespace GestaoPedidos.Application.Mapper
{
    public class ProdutoProfile : Profile
    {
        public ProdutoProfile()
        {
            CreateMap<Produto, ProdutoCreateDTO>().ReverseMap();
            CreateMap<Produto, ProdutoResponseDTO>().ReverseMap();
            CreateMap<Produto, ProdutoUpdateDTO>().ReverseMap();
        }
    }
}
