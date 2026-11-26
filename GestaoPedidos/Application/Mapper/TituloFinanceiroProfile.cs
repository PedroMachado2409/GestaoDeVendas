using AutoMapper;
using GestaoPedidos.Application.DTO.TitulosFinanceiros;
using GestaoPedidos.Domain.Entities;

namespace GestaoPedidos.Application.Mapper
{
    public class TituloFinanceiroProfile : Profile
    {
        public TituloFinanceiroProfile()
        {
            CreateMap<TituloFinanceiro, TituloFinanceiroResponseDTO>();
            CreateMap<BaixaTituloFinanceiro, BaixaTituloFinanceiroResponseDTO>();
        }
    }
}
