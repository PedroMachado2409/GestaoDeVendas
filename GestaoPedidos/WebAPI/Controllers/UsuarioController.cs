using GestaoPedidos.Application.DTO.Usuarios;
using GestaoPedidos.Application.UseCases.Usuarios.Commands;
using GestaoPedidos.Application.UseCases.Usuarios.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace GestaoPedidos.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly RegistrarUsuarioUseCase _registrarUsuario;
        private readonly AutenticarUseCase _autenticar;
        private readonly ListarUsuariosUseCase _listarUsuarios;
        private readonly AtivarUsuarioUseCase _ativarUsuario;
        private readonly InativarUsuarioUseCase _inativarUsuario;
        private readonly AtualizarSenhaUseCase _atualizarSenha;
        private readonly ObterUsuarioAutenticadoUseCase _obterUsuarioAutenticado;
        private readonly AtualizarUsuarioUseCase _atualizarUsuario;
        private readonly AlterarRoleUsuarioUseCase _alterarRole;

        public UsuarioController(
            RegistrarUsuarioUseCase registrarUsuario,
            AutenticarUseCase autenticar,
            ListarUsuariosUseCase listarUsuarios,
            AtivarUsuarioUseCase ativarUsuario,
            InativarUsuarioUseCase inativarUsuario,
            AtualizarSenhaUseCase atualizarSenha,
            ObterUsuarioAutenticadoUseCase obterUsuarioAutenticado,
            AtualizarUsuarioUseCase atualizarUsuario,
            AlterarRoleUsuarioUseCase alterarRole)
        {
            _registrarUsuario = registrarUsuario;
            _autenticar = autenticar;
            _listarUsuarios = listarUsuarios;
            _ativarUsuario = ativarUsuario;
            _inativarUsuario = inativarUsuario;
            _atualizarSenha = atualizarSenha;
            _obterUsuarioAutenticado = obterUsuarioAutenticado;
            _atualizarUsuario = atualizarUsuario;
            _alterarRole = alterarRole;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Listar()
            => Ok(await _listarUsuarios.Executar());

        [Authorize]
        [HttpGet("autenticado")]
        public async Task<IActionResult> ObterAutenticado()
            => Ok(await _obterUsuarioAutenticado.Executar());

        [AllowAnonymous]
        [EnableRateLimiting("login")]
        [HttpPost("registrar")]
        public async Task<IActionResult> Registrar([FromBody] UsuarioCreateDTO dto)
        {
            var usuario = await _registrarUsuario.Executar(dto);
            return StatusCode(StatusCodes.Status201Created, usuario);
        }

        [AllowAnonymous]
        [EnableRateLimiting("login")]
        [HttpPost("autenticar")]
        public async Task<IActionResult> Autenticar([FromBody] LoginRequestDTO dto)
            => Ok(await _autenticar.Executar(dto));

        [Authorize]
        [HttpPut("senha")]
        public async Task<IActionResult> AtualizarSenha([FromBody] UsuarioUpdateSenhaDTO dto)
            => Ok(await _atualizarSenha.Executar(dto));

        [Authorize]
        [HttpPut("perfil")]
        public async Task<IActionResult> AtualizarUsuario([FromBody] UsuarioUpdateDTO dto)
            => Ok(await _atualizarUsuario.Executar(dto));

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}/role")]
        public async Task<IActionResult> AlterarRole(int id, [FromBody] UsuarioRoleUpdateDTO dto)
            => Ok(await _alterarRole.Executar(id, dto.Role));

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}/inativar")]
        public async Task<IActionResult> Inativar(int id)
        {
            await _inativarUsuario.Executar(id);
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}/ativar")]
        public async Task<IActionResult> Ativar(int id)
        {
            await _ativarUsuario.Executar(id);
            return NoContent();
        }
    }
}
