using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Exceptions.Produtos;


namespace GestaoPedidos.Application.UseCases.Produtos.Commands
{
    public class AtivarProdutoUseCase
    {
        private readonly IProdutoRepository _repository;

        public AtivarProdutoUseCase(IProdutoRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Executar(int id)
        {
            var produto = await _repository.ObterPorId(id);
            if (produto == null)
            {
                throw new GestaoPedidos.Domain.Exceptions.NotFoundException(ProdutoExceptions.Produto_NaoEncontrado);
            }

            if (produto.Ativo == true)
            {
                throw new GestaoPedidos.Domain.Exceptions.BadRequestException(ProdutoExceptions.Produto_jaAtivo);
            }

            produto.Ativar();
            await _repository.Atualizar(produto);
            return true;
        }

    }
}
