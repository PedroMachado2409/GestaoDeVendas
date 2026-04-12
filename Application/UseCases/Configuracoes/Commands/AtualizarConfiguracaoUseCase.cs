using AutoMapper;
using GestaoPedidos.Application.DTO.Configuracao;
using GestaoPedidos.Domain.Abstractions;

namespace GestaoPedidos.Application.UseCases.Configuracoes.Commands
{
    public class AtualizarConfiguracaoUseCase
    {
        private readonly IConfiguracaoRepository _configuracaoRepository;
        private readonly IMapper _mapper;

        public AtualizarConfiguracaoUseCase (IConfiguracaoRepository configuracaoRepository, IMapper mapper)
        {
            _configuracaoRepository = configuracaoRepository;
            _mapper = mapper;
        }

        public async Task <ConfiguracaoRespoonseDTO> Executar(UpdateConfiguracaoDTO dto)
        {
            var configuracao = await _configuracaoRepository.ObterConfiguracao();
            configuracao.Atualizar(dto.NomeLoja, dto.EmailOrigem, dto.Smtp, dto.Usuario,
                dto.Senha, dto.Porta, dto.ConexaoSSl, dto.PermiteEstoqueNegativo, 
                dto.HabilitaEnvioDeEmail, dto.EnviaXMLPorEmail, dto.GeraXmlDoPedido, dto.DiretorioXML);

            await _configuracaoRepository.AtualizarConfiguracao(configuracao);
            return _mapper.Map<ConfiguracaoRespoonseDTO>(configuracao);
        }
    }
}
