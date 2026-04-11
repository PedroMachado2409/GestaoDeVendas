using GestaoPedidos.Application.DTO.Configuracao;
using GestaoPedidos.Application.UseCases.Configuracao.Commands;
using GestaoPedidos.Application.UseCases.Configuracao.Queries;
using Microsoft.AspNetCore.Mvc;

namespace GestaoPedidos.WebAPI.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class ConfiguracaoController : ControllerBase
    {
        private readonly AtualizarConfiguracaoUseCase _atualizarConfiguracaoUseCase;
        private readonly ObterConfiguracaoUseCase _obterConfiguracaoUseCase;
        
        public ConfiguracaoController (AtualizarConfiguracaoUseCase atualizarConfiguracaoUseCase, ObterConfiguracaoUseCase obterConfiguracaoUseCase)
        {
            _atualizarConfiguracaoUseCase = atualizarConfiguracaoUseCase;
            _obterConfiguracaoUseCase = obterConfiguracaoUseCase;   
        }

        [HttpGet]
        public async Task <IActionResult> ObterConfiguracao()
        {
            var configuracao = await _obterConfiguracaoUseCase.Executar();
            return Ok(configuracao);
        }

        [HttpPut]
        public async Task <IActionResult> AtualizarConfiguracao([FromBody] UpdateConfiguracaoDTO dto)
        {
            var configuracao = await _atualizarConfiguracaoUseCase.Executar(dto);
            return Ok(configuracao);
        }

    }
}
