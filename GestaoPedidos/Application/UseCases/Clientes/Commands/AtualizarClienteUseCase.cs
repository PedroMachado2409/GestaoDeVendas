using AutoMapper;
using GestaoPedidos.Application.DTO.Clientes;
using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Exceptions.Clientes;

namespace GestaoPedidos.Application.UseCases.Clientes.Commands
{
    public class AtualizarClienteUseCase
        : IUseCase<ClienteUpdateDTO, ClienteResponseDTO>
    {
        private readonly IClienteRepository _repository;
        private readonly IMapper _mapper;

        public AtualizarClienteUseCase(IClienteRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ClienteResponseDTO> Execute(ClienteUpdateDTO dto)
        {
            var cliente = await _repository.ObterPorId(dto.Id)
                ?? throw new BadHttpRequestException(ClientesExceptions.Cliente_NaoEncontrado);

            var clienteComCpfExistente = await _repository.ObterPorCpf(dto.Cpf);
            if (clienteComCpfExistente != null && clienteComCpfExistente.Id != dto.Id)
                throw new BadHttpRequestException(ClientesExceptions.Cliente_CpfExistente);

            var clienteComEmailExistente = await _repository.ObterPorEmail(dto.Email);
            if (clienteComEmailExistente != null && clienteComEmailExistente.Id != dto.Id)
                throw new BadHttpRequestException(ClientesExceptions.Cliente_EmailExistente);

            cliente.Atualizar(dto.Nome, dto.Email, dto.Cpf);

            await _repository.Atualizar(cliente);

            return _mapper.Map<ClienteResponseDTO>(cliente);
        }
    }
}
