
using GestaoPedidos.Application.DTO.Produtos;
using GestaoPedidos.Application.UseCases.Produtos.Commands;
using GestaoPedidos.Application.UseCases.Produtos.Queries;
using Microsoft.AspNetCore.Mvc;

namespace GestaoPedidos.WebAPI.Controllers
{

    [ApiController]
    [Route("/api/[controller]")]
    public class ProdutoController : ControllerBase
    {
        private readonly ListarProdutoUseCase _listarProdutoUseCase;
        private readonly CadastrarProdutoUseCase _cadastrarProdutoUseCase;
        private readonly AtualizarProdutoUseCase _atualizarProdutoUseCase;
        private readonly ObterProdutoPorIdUseCase _obterProdutoPorIdUseCase;
        private readonly InativarProdutoUseCase _inativarProdutoUseCase;
        private readonly AtivarProdutoUseCase _ativarProdutoUseCase;
        private readonly ImportarProdutosTxtUseCase _importarProdutosTxtUseCase;

        public ProdutoController(
            CadastrarProdutoUseCase cadastrarProdutoUseCase,
            ListarProdutoUseCase listarProdutoUseCase,
            AtualizarProdutoUseCase atualizarProdutoUseCase,
            ObterProdutoPorIdUseCase obterProdutoPorIdUseCase,
            InativarProdutoUseCase inativarProdutoUseCase,
            AtivarProdutoUseCase ativarProdutoUseCase,
            ImportarProdutosTxtUseCase importarProdutosTxtUseCase

        )
        {
            _cadastrarProdutoUseCase = cadastrarProdutoUseCase;
            _listarProdutoUseCase = listarProdutoUseCase;
            _atualizarProdutoUseCase = atualizarProdutoUseCase;
            _obterProdutoPorIdUseCase = obterProdutoPorIdUseCase;
            _inativarProdutoUseCase = inativarProdutoUseCase;
            _ativarProdutoUseCase = ativarProdutoUseCase;
            _importarProdutosTxtUseCase = importarProdutosTxtUseCase;
        }

        [HttpGet]
        public async Task<IActionResult> Listar()
            => Ok(await _listarProdutoUseCase.Executar());


        [HttpGet("{id}")]
        public async Task<IActionResult>ObterPorId(int id)
            => Ok(await _obterProdutoPorIdUseCase.Executar(id));

        [HttpPost]
        public async Task<IActionResult> Cadastrar([FromBody] ProdutoCreateDTO dto)
        {
            var produto = await _cadastrarProdutoUseCase.Executar(dto);
            return Ok(produto);
        }

        [HttpPost("importar-txt")]
        public async Task<IActionResult> ImportarTxt(IFormFile file)
        {
            using var stream = file.OpenReadStream();

            var produtos = await _importarProdutosTxtUseCase.Executar(stream);

            return Ok(produtos);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> Atualizar(int id, [FromBody] ProdutoUpdateDTO dto)
        {
            dto.Id = id;
            return Ok(await _atualizarProdutoUseCase.Executar(dto));
        }

        [HttpPut("{id}/Ativar")]
        public async Task<IActionResult> Ativar(int id)
        {
            await _ativarProdutoUseCase.Executar(id);
            return NoContent();
        }

        
        [HttpPut("{id}/Inativar")]
        public async Task<IActionResult> Inativar(int id)
        {
            await _inativarProdutoUseCase.Executar(id);
            return NoContent();
        }
    }
}
