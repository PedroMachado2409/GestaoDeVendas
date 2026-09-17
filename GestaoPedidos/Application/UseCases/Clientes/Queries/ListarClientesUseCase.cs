using AutoMapper;
using GestaoPedidos.Application.DTO.Clientes;
using GestaoPedidos.Domain.Abstractions;

namespace GestaoPedidos.Application.UseCases.Clientes.Queries
{
    public class ListarClientesUseCase
    {
        private readonly IClienteRepository _repository;
        private readonly IMapper _mapper;

        public ListarClientesUseCase(IClienteRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<ClienteResponseDTO>> Executar()
        {
            var clientes = await _repository.Listar();
            return _mapper.Map<List<ClienteResponseDTO>>(clientes);
        }
    }
}
