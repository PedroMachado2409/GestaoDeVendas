using AutoMapper;
using GestaoPedidos.Application.DTO.Usuarios;
using GestaoPedidos.Application.UseCases.Usuarios.Queries;
using GestaoPedidos.Domain.Abstractions.Usuarios;
using GestaoPedidos.Domain.Exceptions;
using GestaoPedidos.Domain.Exceptions.Usuarios;

namespace GestaoPedidos.Application.UseCases.Usuarios.Commands
{
    public class AtualizarUsuarioUseCase
    {
        private readonly IUsuarioRepository _repository;
        private readonly IMapper _mapper;
        private readonly ObterUsuarioAutenticadoUseCase _obterUsuarioAutenticado;

        public AtualizarUsuarioUseCase(
            IUsuarioRepository repository,
            IMapper mapper,
            ObterUsuarioAutenticadoUseCase obterUsuarioAutenticado)
        {
            _repository = repository;
            _mapper = mapper;
            _obterUsuarioAutenticado = obterUsuarioAutenticado;
        }

        public async Task<UsuarioDTO> Executar(UsuarioUpdateDTO dto)
        {
            var usuarioAutenticado = await _obterUsuarioAutenticado.ObterEntidade();
            var email = dto.Email.Trim().ToLowerInvariant();
            var usuarioComEmail = await _repository.ObterPorEmail(email);

            if (usuarioComEmail is not null && usuarioComEmail.Id != usuarioAutenticado.Id)
            {
                throw new ConflictException(UsuariosExceptions.Usuario_JaExistente);
            }

            usuarioAutenticado.AtualizarPerfil(dto.Nome, email);
            await _repository.Atualizar(usuarioAutenticado);

            return _mapper.Map<UsuarioDTO>(usuarioAutenticado);
        }
    }
}
