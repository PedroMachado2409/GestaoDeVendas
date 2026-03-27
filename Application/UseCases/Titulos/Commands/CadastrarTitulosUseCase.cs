using AutoMapper;
using GestaoPedidos.Application.DTO.Titulos;
using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Entities;


namespace GestaoPedidos.Application.UseCases.Titulos.Commands
{
    public class CadastrarTitulosUseCase
    {
        private readonly ITitulosRepository _repository;
        private readonly IMapper _mapper;

        public CadastrarTitulosUseCase (ITitulosRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task <TituloResponseDTO> Executar(CreateTituloDTO dto)
        {
            var novoTitulo = new Titulo(dto.NomeTitulo, dto.ValorTitulo, dto.ClienteId, dto.Observacao, dto.Tipo, null);
            await _repository.CadastrarTitulo(novoTitulo);
            return _mapper.Map<TituloResponseDTO>(novoTitulo);
        }
    }
}
