using GestaoPedidos.Application.DTO.Pessoas;
using GestaoPedidos.Application.UseCases.Clientes.Commands;
using GestaoPedidos.Application.UseCases.Pessoas.Commands;
using GestaoPedidos.Application.UseCases.Pessoas.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestaoPedidos.WebAPI.Controllers
{
    [ApiController]
    [Authorize(Roles = "Admin,Vendedor")]
    [Route("api/[controller]")]
    public class PessoaController : ControllerBase
    {
        private readonly ListarPessoasUseCase _listarPessoas;
        private readonly ObterPessoaPorIdUseCase _obterPessoa;
        private readonly CadastrarPessoaUseCase _cadastrarPessoa;
        private readonly AtualizarPessoaUseCase _atualizarPessoa;
        private readonly AtivarPessoaUseCase _ativarPessoa;
        private readonly InativarPessoaUseCase _inativarPessoa;


        public PessoaController(
            ListarPessoasUseCase listarPessoas,
            ObterPessoaPorIdUseCase obterPessoa,
            CadastrarPessoaUseCase cadastrarPessoa,
            AtualizarPessoaUseCase atualizarPessoa,
            AtivarPessoaUseCase ativarPessoa,
            InativarPessoaUseCase inativarPessoa)
        {
            _listarPessoas = listarPessoas;
            _obterPessoa = obterPessoa;
            _cadastrarPessoa = cadastrarPessoa;
            _atualizarPessoa = atualizarPessoa;
            _ativarPessoa = ativarPessoa;
            _inativarPessoa = inativarPessoa;

        }

        [HttpGet]
        public async Task<IActionResult> ListarPessoas()
            => Ok(await _listarPessoas.Executar());

        [HttpGet("{id}")]
        public async Task<IActionResult> ObterPessoaPorId(int id)
            => Ok(await _obterPessoa.Executar(id));

        [HttpPost]
        public async Task<IActionResult> AdicionarPessoa([FromBody] PessoaCreateDTO dto)
        {
            var pessoa = await _cadastrarPessoa.Executar(dto);
            return CreatedAtAction(nameof(ObterPessoaPorId), new { id = pessoa.Id }, pessoa);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarPessoa(int id, [FromBody] PessoaUpdateDTO dto)
        {
            dto.Id = id;
            return Ok(await _atualizarPessoa.Executar(dto));
        }

        [HttpPut("{id}/ativar")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AtivarPessoa(int id)
        {
            await _ativarPessoa.Executar(id);
            return NoContent();
        }

        [HttpPut("{id}/inativar")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> InativarPessoa(int id)
        {
            await _inativarPessoa.Executar(id);
            return NoContent();
        }
    }
}
