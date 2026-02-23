using GestaoPedidos.Application.DTO.Clientes;
using GestaoPedidos.Application.UseCases.Clientes.Commands;
using GestaoPedidos.Application.UseCases.Clientes.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestaoPedidos.WebAPI.Controllers
{
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    public class ClienteController : ControllerBase
    {
        private readonly ListarClientesUseCase _listarClientes;
        private readonly ObterClientePorIdUseCase _obterCliente;
        private readonly CadastrarClienteUseCase _cadastrarCliente;
        private readonly AtualizarClienteUseCase _atualizarCliente;
        private readonly AtivarClienteUseCase _ativarCliente;
        private readonly InativarClienteUseCase _inativarCliente;
 

        public ClienteController(
            ListarClientesUseCase listarClientes,
            ObterClientePorIdUseCase obterCliente,
            CadastrarClienteUseCase cadastrarCliente,
            AtualizarClienteUseCase atualizarCliente,
            AtivarClienteUseCase ativarCliente,
            InativarClienteUseCase inativarCliente)
        {
            _listarClientes = listarClientes;
            _obterCliente = obterCliente;
            _cadastrarCliente = cadastrarCliente;
            _atualizarCliente = atualizarCliente;
            _ativarCliente = ativarCliente;
            _inativarCliente = inativarCliente;

        }

        [HttpGet]
        public async Task<IActionResult> ListarClientes()
            => Ok(await _listarClientes.Execute());

        [HttpGet("{id}")]
        public async Task<IActionResult> ObterClientePorId(int id)
            => Ok(await _obterCliente.Execute(id));

        [HttpPost]
        public async Task<IActionResult> AdicionarCliente([FromBody] ClienteCreateDTO dto)
        {
            var cliente = await _cadastrarCliente.Execute(dto);
            return CreatedAtAction(nameof(ObterClientePorId), new { id = cliente.Id }, cliente);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarCliente(int id, [FromBody] ClienteUpdateDTO dto)
        {
            dto.Id = id;
            return Ok(await _atualizarCliente.Execute(dto));
        }

        [HttpPut("{id}/ativar")]
        public async Task<IActionResult> AtivarCliente(int id)
        {
            await _ativarCliente.Execute(id);
            return NoContent();
        }

        [HttpPut("{id}/inativar")]
        public async Task<IActionResult> InativarCliente(int id)
        {
            await _inativarCliente.Execute(id);
            return NoContent();
        }
    }
}
