using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Exceptions;

namespace GestaoPedidos.Application.UseCases.Titulos.Commands
{
    public class BaixarTituloUseCase
    {
        private readonly ITitulosRepository _titulosRepository;
        
        public BaixarTituloUseCase (ITitulosRepository titulosRepository)
        {
            _titulosRepository = titulosRepository;
        }

        public async Task Executar(int id)
        {
            var titulo = await _titulosRepository.ObterTituloPorId(id);
            if (titulo == null)
                throw new BadRequestException("Titulo não encontrado!");
            titulo.BaixarTitulo();
            await _titulosRepository.AtualizarTitulo(titulo);

        }
    }
}
