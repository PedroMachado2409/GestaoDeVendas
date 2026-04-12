using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Entities;
using GestaoPedidos.Infrastructure.Services;

namespace GestaoPedidos.Application.UseCases.Produtos.Commands
{
    public class ImportarProdutosTxtUseCase
    {
        private readonly ImportacaoProdutoTxtService _service;
        private readonly IProdutoRepository _repository;

        public ImportarProdutosTxtUseCase(
            ImportacaoProdutoTxtService service,
            IProdutoRepository repository)
        {
            _service = service;
            _repository = repository;
        }

        public async Task<List<Produto>> Executar(Stream stream)
        {
            var produtos = _service.Importar(stream);
            await _repository.AdicionarEmLote(produtos);

            return produtos;
        }
    }
}

