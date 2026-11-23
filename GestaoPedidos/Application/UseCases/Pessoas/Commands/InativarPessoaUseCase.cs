using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Exceptions;
using GestaoPedidos.Domain.Exceptions.Pessoas;

namespace GestaoPedidos.Application.UseCases.Clientes.Commands
{
    public class InativarPessoaUseCase
    {
        private readonly IPessoaRepository _repository;

        public InativarPessoaUseCase(IPessoaRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Executar(int id)
        {
            var cliente = await _repository.ObterPorId(id)
                ?? throw new NotFoundException(PessoasExceptions.Pessoa_NaoEncontrado);

            if (cliente.Ativo == false)
            {
                throw new BadRequestException(PessoasExceptions.Pessoa_JaInativo);
            }

            cliente.Inativar();
            await _repository.Atualizar(cliente);
            return true;
        }
    }
}
