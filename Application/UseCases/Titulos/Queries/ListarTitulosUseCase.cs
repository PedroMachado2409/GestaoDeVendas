using AutoMapper;
using GestaoPedidos.Application.DTO.Titulos;
using GestaoPedidos.Domain.Abstractions;

namespace GestaoPedidos.Application.UseCases.Titulos.Queries
{
    public class ListarTitulosUseCase
    {
        private readonly ITitulosRepository _titulosRepository;
        private readonly IMapper _mapper;

        public ListarTitulosUseCase (ITitulosRepository titulosRepository, IMapper mapper )
        {
            _titulosRepository = titulosRepository;
            _mapper = mapper;
        }

        public async Task<List<TituloResponseDTO>> Executar()
        {
            var titulos = await _titulosRepository.ListarTitulos();
            return _mapper.Map<List<TituloResponseDTO>>( titulos );
        }

    }
}
