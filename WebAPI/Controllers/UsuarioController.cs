using GestaoPedidos.Application.DTO.Usuarios;
using GestaoPedidos.Application.UseCases.Usuarios.Commands;
using GestaoPedidos.Application.UseCases.Usuarios.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestaoPedidos.WebAPI.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly RegistrarUsuarioUseCase _registrarUsuarioUseCase;
        private readonly AutenticarUseCase _autenticarUseCase;
        private readonly ListarUsuariosUseCase _listarUsuariosUseCase;
        private readonly AtivarUsuarioUseCase _ativarUsuarioUseCase;
        private readonly InativarUsuarioUseCase _inativarUsuarioUseCase;
        private readonly AtualizarSenhaUseCase _atualizarSenhaUseCase;
        private readonly ObterUsuarioAutenticadoUseCase _obterUsuarioAutenticadoUseCase;
        private readonly AtualizarUsuarioUseCase _atualizarUsuarioUseCase;

        public UsuarioController (
            RegistrarUsuarioUseCase registrarUsuarioUseCase,
            AutenticarUseCase autenticarUseCase,
            ListarUsuariosUseCase listarUsuarioUseCase,
            AtivarUsuarioUseCase ativarUsuarioUseCase,
            InativarUsuarioUseCase inativarUsuarioUseCase,
            AtualizarSenhaUseCase atualizarSenhaUseCase,
            ObterUsuarioAutenticadoUseCase obterUsuarioAutenticadoUseCase,
            AtualizarUsuarioUseCase atualizarUsuarioUseCase)
        {
            _registrarUsuarioUseCase = registrarUsuarioUseCase;
            _autenticarUseCase = autenticarUseCase;
            _listarUsuariosUseCase = listarUsuarioUseCase;
            _ativarUsuarioUseCase = ativarUsuarioUseCase;
            _inativarUsuarioUseCase = inativarUsuarioUseCase;
            _atualizarSenhaUseCase = atualizarSenhaUseCase;
            _obterUsuarioAutenticadoUseCase = obterUsuarioAutenticadoUseCase;
            _atualizarUsuarioUseCase = atualizarUsuarioUseCase;
        }

        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var usuarios = await _listarUsuariosUseCase.Executar();
            return Ok(usuarios);
        }

        [Authorize]
        [HttpGet("Autenticado")]
        public async Task<IActionResult> ObterAutenticado()
        {
            var usuario = await _obterUsuarioAutenticadoUseCase.Executar();
            return Ok(usuario);
        }

        [HttpPost("Registrar")]
        public async Task<IActionResult> Registrar([FromBody] UsuarioCreateDTO dto)
        {
            var usuario = await _registrarUsuarioUseCase.Executar(dto);
            return Ok(usuario);
        }

        [HttpPost("Autenticar")]
        public async Task<IActionResult> Autenticar([FromBody] LoginRequestDTO dto)
        {
            var usuario = await _autenticarUseCase.Executar(dto);
            return Ok(usuario);
        }

        [Authorize]
        [HttpPut("AtualizarSenha")]
        public async Task<IActionResult> AtualizarSenha([FromBody] UsuarioUpdateSenhaDTO dto)
        {
            var usuario = await _atualizarSenhaUseCase.Executar(dto);
            return Ok(usuario);
        }

        [Authorize]
        [HttpPut("AtualizarUsuario")]
        public async Task<IActionResult> AtualizarUsuario([FromBody] UsuarioUpdateDTO dto)
        {
            var usuario = await _atualizarUsuarioUseCase.Executar(dto);
            return Ok(usuario);
        }

        [HttpPut("inativar/{id}")]
        public async Task<IActionResult> Inativar(int id)
        {
            var usuario = await _inativarUsuarioUseCase.Executar(id);
            return NoContent();
        }


        [HttpPut("ativar/{id}")]
        public async Task<IActionResult> Ativar(int id)
        {
            var usuario = await _ativarUsuarioUseCase.Executar(id);
            return NoContent();
        }

    }
}
