using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Exceptions;
using GestaoPedidos.Domain.Exceptions.Pessoas;



namespace GestaoPedidos.Application.UseCases.Pessoas.Commands
{
    public class AtivarPessoaUseCase
    {
        private readonly IPessoaRepository _repository;

        public AtivarPessoaUseCase(IPessoaRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Executar(int id)
        {
            var pessoa = await _repository.ObterPorId(id)
                ?? throw new NotFoundException(PessoasExceptions.Pessoa_NaoEncontrado);

            if (pessoa.Ativo == true)
            {
                throw new BadRequestException(PessoasExceptions.Pessoa_JaAtivo);
            }

            pessoa.Ativar();
            await _repository.Atualizar(pessoa);

            return true;
        }
    }
}
