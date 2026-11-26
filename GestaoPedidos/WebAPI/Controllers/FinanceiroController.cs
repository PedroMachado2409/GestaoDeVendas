using GestaoPedidos.Application.DTO.TitulosFinanceiros;
using GestaoPedidos.Application.UseCases.TitulosFinanceiros.Commands;
using GestaoPedidos.Application.UseCases.TitulosFinanceiros.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestaoPedidos.WebAPI.Controllers
{
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    public class FinanceiroController : ControllerBase
    {
        private readonly CadastrarTituloFinanceiroUseCase _cadastrarTitulo;
        private readonly ListarTitulosFinanceirosUseCase _listarTitulos;
        private readonly ObterTituloFinanceiroPorIdUseCase _obterTitulo;
        private readonly CancelarTituloFinanceiroUseCase _cancelarTitulo;

        public FinanceiroController(CadastrarTituloFinanceiroUseCase cadastrarTitulo,
            ListarTitulosFinanceirosUseCase listarTitulos, ObterTituloFinanceiroPorIdUseCase obterTitulo,
            CancelarTituloFinanceiroUseCase cancelarTitulo)
        {
            _cadastrarTitulo = cadastrarTitulo;
            _listarTitulos = listarTitulos;
            _obterTitulo = obterTitulo;
            _cancelarTitulo = cancelarTitulo;
        }

        [HttpPost]
        public async Task<IActionResult> Cadastrar([FromBody] TituloFinanceiroCreateDTO dto)
        {
            var titulo = await _cadastrarTitulo.Executar(dto);
            return CreatedAtAction(nameof(ObterPorId), new { id = titulo.Id }, titulo);
        }

        [HttpGet]
        public async Task<IActionResult> Listar([FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
            => Ok(await _listarTitulos.Executar(pagina, tamanhoPagina));

        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObterPorId(int id)
            => Ok(await _obterTitulo.Executar(id));

        [HttpPut("{id:int}/cancelar")]
        public async Task<IActionResult> Cancelar(int id)
            => Ok(await _cancelarTitulo.Executar(id));

        [HttpPost("{id:int}/baixas")]
        public async Task<IActionResult> Baixar(int id, [FromBody] BaixaTituloFinanceiroCreateDTO dto,
            [FromServices] BaixarTituloFinanceiroUseCase useCase)
            => Ok(await useCase.Executar(id, dto));

        [HttpPut("{id:int}/baixas/{baixaId:int}/estornar")]
        public async Task<IActionResult> EstornarBaixa(int id, int baixaId,
            [FromServices] EstornarBaixaTituloFinanceiroUseCase useCase)
            => Ok(await useCase.Executar(id, baixaId));
    }
}
