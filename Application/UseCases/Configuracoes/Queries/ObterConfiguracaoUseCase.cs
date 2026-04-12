using AutoMapper;
using GestaoPedidos.Application.DTO.Configuracao;
using GestaoPedidos.Domain.Abstractions;

namespace GestaoPedidos.Application.UseCases.Configuracoes.Queries
{
    public class ObterConfiguracaoUseCase
    {
        private readonly IConfiguracaoRepository _configuracaoRepository;
        private readonly IMapper _mapper;

        public ObterConfiguracaoUseCase (IConfiguracaoRepository configuracaoRepository, IMapper mapper)
        {
            _configuracaoRepository = configuracaoRepository;
            _mapper = mapper;
        }

        public async Task<ConfiguracaoRespoonseDTO> Executar()
        {
            var configuracao = await _configuracaoRepository.ObterConfiguracao();
            return _mapper.Map<ConfiguracaoRespoonseDTO>(configuracao);
        }
    }
}
