using AutoMapper;
using GestaoPedidos.Application.DTO.Clientes;
using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Exceptions.Clientes;

namespace GestaoPedidos.Application.UseCases.Clientes.Queries
{
    public class ObterClientePorIdUseCase
    {
        private readonly IClienteRepository _repository;
        private readonly IMapper _mapper;

        public ObterClientePorIdUseCase(IClienteRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ClienteResponseDTO> Executar(int id)
        {
            var cliente = await _repository.ObterPorId(id)
                ?? throw new(ClientesExceptions.Cliente_NaoEncontrado);

            return _mapper.Map<ClienteResponseDTO>(cliente);
        }
    }
}
