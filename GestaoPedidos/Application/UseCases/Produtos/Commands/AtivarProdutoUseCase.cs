using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Exceptions;
using GestaoPedidos.Domain.Exceptions.Produtos;

namespace GestaoPedidos.Application.UseCases.Produtos.Commands
{
    public class AtivarProdutoUseCase
    {
        private readonly IProdutoRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public AtivarProdutoUseCase(IProdutoRepository repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task Executar(int id)
        {
            var produto = await _repository.ObterPorId(id)
                ?? throw new NotFoundException(ProdutoExceptions.Produto_NaoEncontrado);

            if (produto.Ativo)
            {
                throw new BadRequestException(ProdutoExceptions.Produto_jaAtivo);
            }

            produto.Ativar();
            await _repository.Atualizar(produto);
            await _unitOfWork.SalvarAlteracoes();
        }
    }
}
