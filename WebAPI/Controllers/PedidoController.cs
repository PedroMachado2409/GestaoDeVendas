using GestaoPedidos.Application.DTO.Clientes;
using GestaoPedidos.Application.DTO.Pedidos;
using GestaoPedidos.Application.UseCases.Pedidos.Commands;
using GestaoPedidos.Application.UseCases.Pedidos.Queries;
using Microsoft.AspNetCore.Mvc;

namespace GestaoPedidos.WebAPI.Controllers
{

    [ApiController]
    [Route("/api/[controller]")]
    public class PedidoController : ControllerBase
    {
        private readonly CadastrarPedidoUseCase _cadastrarPedidoUseCase;
        private readonly ObterPedidoPorIdUseCase _obterPedidoPorIdUseCase;
        private readonly FinalizarPedidoUseCase _finalizarPedidoUseCase;
        private readonly CancelarPedidoUseCase _cancelarPedidoUseCase;
        private readonly AtualizarPedidoUseCase _atualizarPedidoUseCase;
        public PedidoController(CadastrarPedidoUseCase cadastrarPedidoUseCase,
            ObterPedidoPorIdUseCase obterPedidoPorIdUseCase,
            FinalizarPedidoUseCase finalizarPedidoUseCase,
            CancelarPedidoUseCase cancelarPedidoUseCase,
            AtualizarPedidoUseCase atualizarPedidoUseCase
            )
        {
            _cadastrarPedidoUseCase = cadastrarPedidoUseCase;
            _obterPedidoPorIdUseCase = obterPedidoPorIdUseCase;
            _finalizarPedidoUseCase = finalizarPedidoUseCase;
            _cancelarPedidoUseCase= cancelarPedidoUseCase;
            _atualizarPedidoUseCase = atualizarPedidoUseCase;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObterPorId(int id)
        {
            var pedido = await _obterPedidoPorIdUseCase.Executar(id);
            return Ok(pedido);
        }

        [HttpPost]
        public async Task<IActionResult> CadastrarPedido([FromBody] CriarPedidoRequestDTO dto)
        {
            var pedido = await _cadastrarPedidoUseCase.Executar(dto);
            return Ok(pedido);
        }

        [HttpPut("finalizar/{id}")]
        public async Task<IActionResult> FinalizarPedido(int id)
        {
            var pedido = await _finalizarPedidoUseCase.Executar(id);
            return Ok(pedido);
        }


        [HttpPut("cancelar/{id}")]
        public async Task<IActionResult> CancelarPedido(int id)
        {
            var pedido = await _cancelarPedidoUseCase.Executar(id);
            return Ok(pedido);
        }
    }
}
