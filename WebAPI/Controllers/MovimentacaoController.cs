
using GestaoPedidos.Application.UseCases.MovimentacoesEstoque;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestaoPedidos.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MovimentacaoController : ControllerBase
    {
        private readonly ListarMovimentacaoUseCase _movimentacaoUseCase; 

        public MovimentacaoController (ListarMovimentacaoUseCase listarMovimentacaoUseCase)
        {
            _movimentacaoUseCase = listarMovimentacaoUseCase;
        }

        [HttpGet("{produtoId}")]
        public async Task <IActionResult> ListarMovimentacoesPorProduto(int produtoId)
        {
            var movimentacoes = await _movimentacaoUseCase.Executar(produtoId);
            return Ok(movimentacoes);
        }
    }
}
