using GestaoPedidos.Application.DTO.Pedidos;
using GestaoPedidos.Application.UseCases.Pedidos.Commands;
using GestaoPedidos.Application.UseCases.Pedidos.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestaoPedidos.WebAPI.Controllers
{

    [ApiController]
    [Authorize(Roles = "Admin,Vendedor")]
    [Route("api/[controller]")]
    public class PedidoController : ControllerBase
    {
        private readonly CadastrarPedidoUseCase _cadastrarPedidoUseCase;
        private readonly ObterPedidoPorIdUseCase _obterPedidoPorIdUseCase;
        private readonly FinalizarPedidoUseCase _finalizarPedidoUseCase;
        private readonly CancelarPedidoUseCase _cancelarPedidoUseCase;
        private readonly ListarPedidosUseCase _listarPedidosUseCase;
        public PedidoController(CadastrarPedidoUseCase cadastrarPedidoUseCase,
            ObterPedidoPorIdUseCase obterPedidoPorIdUseCase,
            FinalizarPedidoUseCase finalizarPedidoUseCase,
            CancelarPedidoUseCase cancelarPedidoUseCase,
            ListarPedidosUseCase listarPedidosUseCase
            )
        {
            _cadastrarPedidoUseCase = cadastrarPedidoUseCase;
            _obterPedidoPorIdUseCase = obterPedidoPorIdUseCase;
            _finalizarPedidoUseCase = finalizarPedidoUseCase;
            _cancelarPedidoUseCase = cancelarPedidoUseCase;
            _listarPedidosUseCase = listarPedidosUseCase;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObterPorId(int id)
        {
            var pedido = await _obterPedidoPorIdUseCase.Executar(id);
            return Ok(pedido);
        }

        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var pedidos = await _listarPedidosUseCase.Executar();
            return Ok(pedidos);
        }

        [HttpPost]
        public async Task<IActionResult> CadastrarPedido([FromBody] CriarPedidoRequestDTO dto)
        {
            var pedido = await _cadastrarPedidoUseCase.Executar(dto);
            return CreatedAtAction(nameof(ObterPorId), new { id = pedido.Id }, pedido);
        }

        [HttpPut("{id:int}/finalizar")]
        public async Task<IActionResult> FinalizarPedido(int id)
        {
            var pedido = await _finalizarPedidoUseCase.Executar(id);
            return Ok(pedido);
        }

        [HttpPut("{id:int}/cancelar")]
        public async Task<IActionResult> CancelarPedido(int id)
        {
            var pedido = await _cancelarPedidoUseCase.Executar(id);
            return Ok(pedido);
        }
    }
}
