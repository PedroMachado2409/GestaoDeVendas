using AutoMapper;
using GestaoPedidos.Application.DTO.Produtos;
using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Entities;

namespace GestaoPedidos.Application.UseCases.Produtos.Commands
{
    public class CadastrarProdutoUseCase
    {
        private readonly IProdutoRepository _repository;
        private readonly IMapper _mapper;

        public CadastrarProdutoUseCase(IProdutoRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ProdutoResponseDTO> Executar(ProdutoCreateDTO dto)
        {
            var novoProduto = new Produto(dto.Nome, dto.Marca, dto.Estoque, dto.Preco);
            await _repository.Cadastrar(novoProduto);
            return _mapper.Map<ProdutoResponseDTO>(novoProduto);
        }

    }
}
