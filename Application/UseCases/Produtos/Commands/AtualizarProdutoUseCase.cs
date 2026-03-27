using AutoMapper;
using GestaoPedidos.Application.DTO.Produtos;
using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Exceptions.Produtos;

namespace GestaoPedidos.Application.UseCases.Produtos.Commands
{
    public class AtualizarProdutoUseCase
    {
        private readonly IMapper _mapper;
        private readonly IProdutoRepository _repository;

        public AtualizarProdutoUseCase (IMapper mapper, IProdutoRepository repository)
        {
            _mapper = mapper;
            _repository = repository;
        }

        public async Task<ProdutoResponseDTO> Executar(ProdutoUpdateDTO dto)
        {
            var produto = await _repository.ObterPorId(dto.Id);
            if (produto == null)
                throw new BadHttpRequestException(ProdutoExceptions.Produto_NaoEncontrado);

            produto.Atualizar(dto.Nome, dto.Marca, dto.Estoque, dto.Preco);
            await _repository.Atualizar(produto);

            return _mapper.Map<ProdutoResponseDTO>(produto);
        }

    }
}
