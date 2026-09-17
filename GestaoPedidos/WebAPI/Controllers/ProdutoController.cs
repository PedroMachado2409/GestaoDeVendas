
using GestaoPedidos.Application.DTO.Produtos;
using GestaoPedidos.Application.UseCases.Produtos.Commands;
using GestaoPedidos.Application.UseCases.Produtos.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestaoPedidos.WebAPI.Controllers
{

    [ApiController]
    [Authorize(Roles = "Admin,Vendedor")]
    [Route("api/[controller]")]
    public class ProdutoController : ControllerBase
    {
        private readonly ListarProdutoUseCase _listarProdutoUseCase;
        private readonly CadastrarProdutoUseCase _cadastrarProdutoUseCase;
        private readonly AtualizarProdutoUseCase _atualizarProdutoUseCase;
        private readonly ObterProdutoPorIdUseCase _obterProdutoPorIdUseCase;
        private readonly InativarProdutoUseCase _inativarProdutoUseCase;
        private readonly AtivarProdutoUseCase _ativarProdutoUseCase;
        private readonly AjusteEstoqueUseCase _ajusteEstoqueUseCase;

        public ProdutoController(
            CadastrarProdutoUseCase cadastrarProdutoUseCase,
            ListarProdutoUseCase listarProdutoUseCase,
            AtualizarProdutoUseCase atualizarProdutoUseCase,
            ObterProdutoPorIdUseCase obterProdutoPorIdUseCase,
            InativarProdutoUseCase inativarProdutoUseCase,
            AtivarProdutoUseCase ativarProdutoUseCase,
            AjusteEstoqueUseCase ajusteEstoqueUseCase
        )
        {
            _cadastrarProdutoUseCase = cadastrarProdutoUseCase;
            _listarProdutoUseCase = listarProdutoUseCase;
            _atualizarProdutoUseCase = atualizarProdutoUseCase;
            _obterProdutoPorIdUseCase = obterProdutoPorIdUseCase;
            _inativarProdutoUseCase = inativarProdutoUseCase;
            _ativarProdutoUseCase = ativarProdutoUseCase;
            _ajusteEstoqueUseCase = ajusteEstoqueUseCase;
        }

        [HttpGet]
        public async Task<IActionResult> Listar()
            => Ok(await _listarProdutoUseCase.Executar());


        [HttpGet("{id}")]
        public async Task<IActionResult> ObterPorId(int id)
            => Ok(await _obterProdutoPorIdUseCase.Executar(id));

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Cadastrar([FromBody] ProdutoCreateDTO dto)
        {
            var produto = await _cadastrarProdutoUseCase.Executar(dto);
            return CreatedAtAction(nameof(ObterPorId), new { id = produto.Id }, produto);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Atualizar(int id, [FromBody] ProdutoUpdateDTO dto)
        {
            dto.Id = id;
            return Ok(await _atualizarProdutoUseCase.Executar(dto));
        }

        [HttpPut("{id:int}/ativar")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Ativar(int id)
        {
            await _ativarProdutoUseCase.Executar(id);
            return NoContent();
        }


        [HttpPut("{id:int}/inativar")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Inativar(int id)
        {
            await _inativarProdutoUseCase.Executar(id);
            return NoContent();
        }

        [HttpPost("{id:int}/ajustarEstoque")]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> AjustarEstoque(int id, [FromBody] AjusteEstoqueProdutoDTO dto)
        {
            dto.Id = id;
            return Ok(await _ajusteEstoqueUseCase.Executar(dto));
        }


    }
}
