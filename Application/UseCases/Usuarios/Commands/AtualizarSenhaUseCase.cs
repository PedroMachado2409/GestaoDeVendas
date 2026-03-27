using AutoMapper;
using GestaoPedidos.Application.DTO.Usuarios;
using GestaoPedidos.Application.UseCases.Usuarios.Queries;
using GestaoPedidos.Domain.Abstractions.Usuarios;
using GestaoPedidos.Domain.Entities;
using GestaoPedidos.Domain.Exceptions.Usuarios;
using GestaoPedidos.Infrastructure.Security;

namespace GestaoPedidos.Application.UseCases.Usuarios.Commands
{
    public class AtualizarSenhaUseCase
    {
        private readonly IUsuarioRepository _repository;
        private readonly ObterUsuarioAutenticadoUseCase _obterUsuarioAutenticadoUseCase;
        private readonly IMapper _mapper;

        public AtualizarSenhaUseCase (IUsuarioRepository repository, ObterUsuarioAutenticadoUseCase obterUsuarioAutenticadoUseCase, IMapper mapper)
        {
            _repository = repository;
            _obterUsuarioAutenticadoUseCase = obterUsuarioAutenticadoUseCase;
            _mapper = mapper;
        }

        public async Task<UsuarioDTO> Executar(UsuarioUpdateSenhaDTO dto)
        {
            var usuarioDto = await _obterUsuarioAutenticadoUseCase.Executar()
                ?? throw new BadHttpRequestException(UsuariosExceptions.Usuario_NaoEncontrado);

            var usuario = await _repository.ObterPorId(usuarioDto.Id)
                ?? throw new BadHttpRequestException(UsuariosExceptions.Usuario_NaoEncontrado);
            dto.Id = usuario.Id;

            if (!PasswordHelper.VerificarSenha(dto.SenhaAntiga, usuario.Senha))
                throw new BadHttpRequestException(UsuariosExceptions.Usuario_AtualSenhaIncorreta);

            usuario.Senha = PasswordHelper.HashPassword(dto.NovaSenha);
            
            await _repository.Atualizar(usuario);
            return _mapper.Map<UsuarioDTO>(usuario);
        }

    }
}
    