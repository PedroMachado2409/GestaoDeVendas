using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Exceptions.Produtos;
namespace GestaoPedidos.Application.UseCases.Produtos.Commands
{
    public class InativarProdutoUseCase
    {
        private readonly IProdutoRepository _repository;

        public InativarProdutoUseCase(IProdutoRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Executar(int id)
        {
            var produto = await _repository.ObterPorId(id);
            if (produto == null)
                throw new BadHttpRequestException(ProdutoExceptions.Produto_NaoEncontrado);

            if (produto.Ativo == false)
                throw new BadHttpRequestException(ProdutoExceptions.Produto_JaInativo);

            produto.Inativar();
            await _repository.Atualizar(produto);
            return true;
        }
    }
}
