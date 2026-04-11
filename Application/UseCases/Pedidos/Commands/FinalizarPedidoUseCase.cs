using AutoMapper;
using GestaoPedidos.Application.DTO.Pedidos;
using GestaoPedidos.Application.UseCases.XML;
using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Entities;
using GestaoPedidos.Domain.Enum;
using GestaoPedidos.Domain.Exceptions;
using GestaoPedidos.Domain.Exceptions.Pedidos;
using GestaoPedidos.Domain.Exceptions.Produtos;

namespace GestaoPedidos.Application.UseCases.Pedidos.Commands
{
    public class FinalizarPedidoUseCase
    {
        private readonly IPedidoRepository _repository;
        private readonly IProdutoRepository _produtoRepository;
        private readonly ITitulosRepository _titulosRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly IMovimentacaoEstoqueRepository _movimentacaRepository;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        private readonly GerarXmlPedidoUseCase _gerarXmlPedidoUseCase;

        public FinalizarPedidoUseCase(
            IPedidoRepository repository,
            IMapper mapper,
            IProdutoRepository produtoRepository,
            ITitulosRepository titulosRepository,
            IClienteRepository clienteRepository,
            IMovimentacaoEstoqueRepository movimentacaRepository,
            IEmailService emailService,
            GerarXmlPedidoUseCase gerarXmlPedidoUseCase
            )
        {
            _repository = repository;
            _mapper = mapper;
            _produtoRepository = produtoRepository;
            _titulosRepository = titulosRepository;
            _clienteRepository = clienteRepository;
            _movimentacaRepository = movimentacaRepository;
            _gerarXmlPedidoUseCase = gerarXmlPedidoUseCase;
            _emailService = emailService;
        }

        public async Task<PedidoResponseDTO> Executar(int pedidoId)
        {
            var pedido = await _repository.ObterPorId(pedidoId);
            if (pedido == null)
                throw new NotFoundException(PedidosExceptions.Pedido_NaoEncontrado);

            var cliente = await _clienteRepository.ObterPorId(pedido.ClienteId);
            if (cliente == null)
                throw new NotFoundException("Cliente não encontrado.");

            pedido.Finalizar();

            foreach (var item in pedido.Itens)
            {
                var produto = await _produtoRepository.ObterPorId(item.ProdutoId);
                if (produto == null)
                    throw new NotFoundException(ProdutoExceptions.Produto_NaoEncontrado);

                produto.FinalizarEstoquePedido(pedido.Tipo, item.Quantidade);
                await _produtoRepository.Atualizar(produto);

                var tipoMovimentacao = pedido.Tipo == TipoPedido.Compra ? TipoMovimentacao.Entrada : TipoMovimentacao.Saida;
                var movimentacaoEstoque = new MovimentacaoEstoque(item.ProdutoId, item.Quantidade, pedidoId ,Origem.Pedido, tipoMovimentacao);
                movimentacaoEstoque.ConverterQuantidade();
                await _movimentacaRepository.CadastrarMovimentacao(movimentacaoEstoque);
            }

                await _gerarXmlPedidoUseCase.Executar(pedido);
                var EnvioDeEmail = Task.Run(() => _emailService.EnviarPedidoCriadoAsync(
                cliente.Email,
                cliente.Nome,
                pedido.Id.ToString(),
                pedido.ValorTotal,
                pedido.Itens));
                var tipoTitulo = pedido.Tipo == TipoPedido.Compra ? TipoTitulo.Saída : TipoTitulo.Entrada;
                var titulo = new Titulo(cliente.Nome,pedido.ValorTotal,pedido.ClienteId,string.Empty, tipoTitulo, pedidoId, Origem.Pedido);
                titulo.ConverterValor();
                await _repository.Atualizar(pedido);
                await _titulosRepository.CadastrarTitulo(titulo);
                return _mapper.Map<PedidoResponseDTO>(pedido);
        }
    }
}