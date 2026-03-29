using GestaoPedidos.Application.DTO.Titulos;
using GestaoPedidos.Application.UseCases.Titulos.Commands;
using GestaoPedidos.Application.UseCases.Titulos.Queries;
using GestaoPedidos.Migrations;
using Microsoft.AspNetCore.Mvc;

namespace GestaoPedidos.WebAPI.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class TituloController : ControllerBase
    {
        private readonly CadastrarTitulosUseCase _cadastrarTitulosUseCase;
        private readonly ListarTitulosUseCase _listarTitulosUseCase;
        private readonly BaixarTituloUseCase _baixarTituloUseCase;
        private readonly RemoverBaixaUseCase  _removerBaixaUseCase;

        public TituloController (CadastrarTitulosUseCase cadastrarTitulosUseCase, ListarTitulosUseCase listarTitulosUseCase, BaixarTituloUseCase baixarTituloUseCase, RemoverBaixaUseCase removerBaixaUseCase)
        {
            _cadastrarTitulosUseCase = cadastrarTitulosUseCase;
            _listarTitulosUseCase = listarTitulosUseCase;
            _baixarTituloUseCase = baixarTituloUseCase;
            _removerBaixaUseCase = removerBaixaUseCase;
        }

        [HttpGet]
        public async Task<IActionResult> ListarTitulos()
            => Ok(await _listarTitulosUseCase.Executar());

        [HttpPost]
        public async Task<IActionResult> CadastrarTitulo([FromBody] CreateTituloDTO dto)
        {
            var novoTitulo = await _cadastrarTitulosUseCase.Executar(dto);
            return Ok(novoTitulo);
        }

        [HttpPut("baixar/{id}")]
        public async Task<IActionResult> BaixarTitulo(int id)
        {
            await _baixarTituloUseCase.Executar(id);
            return NoContent();
        }
        [HttpPut("removerBaixa/{id}")]
        public async Task<IActionResult> RemoverBaixa(int id)
        {
            await _removerBaixaUseCase.Executar(id);
            return NoContent();
        }


    }
}
