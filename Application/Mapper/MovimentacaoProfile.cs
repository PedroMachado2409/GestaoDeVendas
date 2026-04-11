using AutoMapper;
using GestaoPedidos.Application.DTO.MovimentacoesEstoque;
using GestaoPedidos.Domain.Entities;

namespace GestaoPedidos.Application.Mapper
{
    public class MovimentacaoProfile : Profile
    {
        public MovimentacaoProfile()
        {
            CreateMap<MovimentacaoEstoque, CreateMovimentacaoDTO>().ReverseMap();
            CreateMap<MovimentacaoEstoque, MovimentacaoResponseDTO>().ReverseMap();
        }
    }
}
