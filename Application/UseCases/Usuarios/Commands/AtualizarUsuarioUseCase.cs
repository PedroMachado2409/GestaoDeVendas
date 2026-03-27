using AutoMapper;
using GestaoPedidos.Application.DTO.Usuarios;
using GestaoPedidos.Application.UseCases.Usuarios.Queries;
using GestaoPedidos.Domain.Abstractions.Usuarios;
using GestaoPedidos.Domain.Exceptions.Usuarios;

namespace GestaoPedidos.Application.UseCases.Usuarios.Commands
{
    public class AtualizarUsuarioUseCase
    {
        private readonly IUsuarioRepository _repository;
        private readonly IMapper _mapper;
        private readonly ObterUsuarioAutenticadoUseCase _ObterUsuarioAutenticado;

        public AtualizarUsuarioUseCase(IUsuarioRepository repository, IMapper mapper, ObterUsuarioAutenticadoUseCase obterUsuarioAutenticado)
        {
            _repository = repository;
            _mapper = mapper;
            _ObterUsuarioAutenticado = obterUsuarioAutenticado;
        }

        public async Task<UsuarioDTO> Executar(UsuarioUpdateDTO dto)
        {
            var usuarioDto = await _ObterUsuarioAutenticado.Executar();
             if (usuarioDto == null)
                throw new BadHttpRequestException(UsuariosExceptions.Usuario_NaoEncontrado);

             var usuario = await _repository.ObterPorId(usuarioDto.Id);
            if (usuario == null)
                throw new BadHttpRequestException(UsuariosExceptions.Usuario_NaoEncontrado);

            dto.Id = usuario.Id;
            usuario.Atualizar(dto.Nome, dto.Email, dto.Role);
            await _repository.Atualizar(usuario);
            return _mapper.Map<UsuarioDTO>(usuario);

        }


    }
}
