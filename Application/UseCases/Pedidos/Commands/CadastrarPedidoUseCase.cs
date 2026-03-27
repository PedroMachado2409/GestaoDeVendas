using AutoMapper;
using GestaoPedidos.Application.DTO.Pedidos;
using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Entities.Pedidos;
using GestaoPedidos.Domain.Exceptions;
using GestaoPedidos.Domain.Exceptions.Clientes;
using GestaoPedidos.Domain.Exceptions.Pedidos;
using GestaoPedidos.Domain.Exceptions.Produtos;


namespace GestaoPedidos.Application.UseCases.Pedidos.Commands
{
    public class CadastrarPedidoUseCase
    {
        private readonly IPedidoRepository _pedidoRepository;
        private readonly IProdutoRepository _produtoRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly IMapper _mapper;

        public CadastrarPedidoUseCase (IPedidoRepository pedidoRepository, 
            IProdutoRepository produtoRepository,
            IClienteRepository clienteRepository,
            IMapper mapper)
        {
            _pedidoRepository = pedidoRepository;
            _produtoRepository = produtoRepository;
            _clienteRepository = clienteRepository;
            _mapper = mapper;
        }

        public async Task <PedidoResponseDTO> Executar(CriarPedidoRequestDTO dto)
        {
            var cliente = await _clienteRepository.ObterPorId(dto.ClienteId);
                if(cliente == null)
                    throw new NotFoundException(ClientesExceptions.Cliente_NaoEncontrado);
                if(cliente.Ativo == false)
                    throw new BadRequestException(ClientesExceptions.Cliente_Inativo);

            var itens = new List<PedidoItem>();

            foreach (var item in dto.Itens)
            {
                var produto = await _produtoRepository.ObterPorId(item.ProdutoId);
                if (produto == null)
                    throw new NotFoundException(ProdutoExceptions.Produto_NaoEncontrado);
                if (produto.Ativo == false)
                    throw new BadRequestException(ProdutoExceptions.Produto_Inativo);

                itens.Add(new PedidoItem(produto.Id, produto.Preco, item.Quantidade));
                produto.ReservarQuantidade(item.Quantidade);
            }

            var pedido = new Pedido(dto.ClienteId, itens);
            await _pedidoRepository.Cadastrar(pedido);
            return _mapper.Map<PedidoResponseDTO>(pedido);
        }
    }
}
