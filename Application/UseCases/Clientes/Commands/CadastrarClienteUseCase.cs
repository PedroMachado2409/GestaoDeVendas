using AutoMapper;
using GestaoPedidos.Application.DTO.Clientes;
using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Entities;
using GestaoPedidos.Domain.Exceptions;
using GestaoPedidos.Domain.Exceptions.Clientes;

namespace GestaoPedidos.Application.UseCases.Clientes.Commands
{
    public class CadastrarClienteUseCase : IUseCase<ClienteCreateDTO, ClienteResponseDTO>
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IMapper _mapper;

        public CadastrarClienteUseCase (IClienteRepository clienteRepository, IMapper mapper)
        {
            _clienteRepository = clienteRepository;
            _mapper = mapper;
        }

        public async Task<ClienteResponseDTO> Execute(ClienteCreateDTO dto)
        {
            var clienteExistente = await _clienteRepository.ObterPorCpf(dto.Cpf);
            if (clienteExistente != null)
                throw new BadRequestException(ClientesExceptions.Cliente_CpfExistente);
            
            var emailExistente = await _clienteRepository.ObterPorEmail(dto.Email);
            if (emailExistente != null)
                throw new BadRequestException(ClientesExceptions.Cliente_EmailExistente);

            var novoCliente = new Cliente(dto.Nome, dto.Email, dto.Cpf);

            await _clienteRepository.Cadastrar(novoCliente);

            return _mapper.Map<ClienteResponseDTO>(novoCliente);
        }
    }
}
