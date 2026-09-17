using GestaoPedidos.Application.DTO.MovimentacaoEstoque;
using GestaoPedidos.Application.UseCases.MovimentacoesEstoque;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestaoPedidos.WebAPI.Controllers
{
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    public class MovimentacaoEstoqueController : ControllerBase
    {
        private readonly ListarMovimentacaoEstoqueUseCase _listarMovimentacaoEstoque;

        public MovimentacaoEstoqueController(ListarMovimentacaoEstoqueUseCase listarMovimentacaoEstoque)
        {
            _listarMovimentacaoEstoque = listarMovimentacaoEstoque;
        }

        /// <summary>
        /// Histórico de movimentações de um produto, da mais recente para a mais
        /// antiga. A rota diz "produto" porque o identificador é do produto, e
        /// não da movimentação.
        /// </summary>
        [HttpGet("produto/{produtoId:int}")]
        [ProducesResponseType(typeof(List<MovimentacaoEstoqueResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ListarPorProduto(int produtoId)
            => Ok(await _listarMovimentacaoEstoque.Executar(produtoId));
    }
}
