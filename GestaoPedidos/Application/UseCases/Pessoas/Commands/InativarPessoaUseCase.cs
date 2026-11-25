using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Exceptions;
using GestaoPedidos.Domain.Exceptions.Pessoas;

namespace GestaoPedidos.Application.UseCases.Pessoas.Commands
{
    public class InativarPessoaUseCase
    {
        private readonly IPessoaRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public InativarPessoaUseCase(IPessoaRepository repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task Executar(int id)
        {
            var pessoa = await _repository.ObterPorId(id)
                ?? throw new NotFoundException(PessoasExceptions.Pessoa_NaoEncontrado);

            if (!pessoa.Ativo)
            {
                throw new BadRequestException(PessoasExceptions.Pessoa_JaInativo);
            }

            pessoa.Inativar();
            await _repository.Atualizar(pessoa);
            await _unitOfWork.SalvarAlteracoes();
        }
    }
}
