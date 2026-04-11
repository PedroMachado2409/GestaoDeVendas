using AutoMapper;
using GestaoPedidos.Application.DTO.Configuracao;
using GestaoPedidos.Domain.Entities;

namespace GestaoPedidos.Application.Mapper
{
    public class ConfiguracaoProfile : Profile
    {
        public ConfiguracaoProfile()
        {
            CreateMap<Configuracao, UpdateConfiguracaoDTO>().ReverseMap();
            CreateMap<Configuracao, ConfiguracaoRespoonseDTO>().ReverseMap();

        }
    }
}
