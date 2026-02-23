using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Exceptions;
using GestaoPedidos.Domain.Exceptions.Clientes;



namespace GestaoPedidos.Application.UseCases.Clientes.Commands
{
    public class AtivarClienteUseCase
       : IUseCase<int, bool>
    {
        private readonly IClienteRepository _repository;

        public AtivarClienteUseCase(IClienteRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Execute(int id)
        {
            var cliente = await _repository.ObterPorId(id)
                ?? throw new NotFoundException(ClientesExceptions.Cliente_NaoEncontrado);

            if (cliente.Ativo == true)
                throw new BadRequestException(ClientesExceptions.Cliente_JaAtivo);

            cliente.Ativar();
            await _repository.Atualizar(cliente);

            return true;
        }
    }
}
