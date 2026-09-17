using AutoMapper;
using GestaoPedidos.Application.DTO.Usuarios;
using GestaoPedidos.Application.UseCases.Usuarios.Queries;
using GestaoPedidos.Domain.Abstractions.Usuarios;
using GestaoPedidos.Domain.Exceptions;
using GestaoPedidos.Domain.Exceptions.Usuarios;

namespace GestaoPedidos.Application.UseCases.Usuarios.Commands
{
    public class AtualizarSenhaUseCase
    {
        private readonly IUsuarioRepository _repository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ObterUsuarioAutenticadoUseCase _obterUsuarioAutenticado;
        private readonly IMapper _mapper;

        public AtualizarSenhaUseCase(
            IUsuarioRepository repository,
            IPasswordHasher passwordHasher,
            ObterUsuarioAutenticadoUseCase obterUsuarioAutenticado,
            IMapper mapper)
        {
            _repository = repository;
            _passwordHasher = passwordHasher;
            _obterUsuarioAutenticado = obterUsuarioAutenticado;
            _mapper = mapper;
        }

        public async Task<UsuarioDTO> Executar(UsuarioUpdateSenhaDTO dto)
        {
            var usuario = await _obterUsuarioAutenticado.ObterEntidade();

            if (!_passwordHasher.Verificar(dto.SenhaAntiga, usuario.Senha))
            {
                throw new BadRequestException(UsuariosExceptions.Usuario_AtualSenhaIncorreta);
            }

            usuario.AlterarSenha(_passwordHasher.Hash(dto.NovaSenha));
            await _repository.Atualizar(usuario);

            return _mapper.Map<UsuarioDTO>(usuario);
        }
    }
}
