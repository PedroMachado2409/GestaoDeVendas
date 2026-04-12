using AutoMapper;
using GestaoPedidos.Application.DTO.Pedidos;
using GestaoPedidos.Application.UseCases.XML;
using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Entities;
using GestaoPedidos.Domain.Entities.Pedidos;
using GestaoPedidos.Domain.Enum;
using GestaoPedidos.Domain.Exceptions;
using GestaoPedidos.Domain.Exceptions.Pedidos;
using GestaoPedidos.Domain.Exceptions.Produtos;

namespace GestaoPedidos.Application.UseCases.Pedidos.Commands
{
    public class FinalizarPedidoUseCase
    {
        private readonly IPedidoRepository _pedidoRepository;
        private readonly IProdutoRepository _produtoRepository;
        private readonly ITitulosRepository _titulosRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly IMovimentacaoEstoqueRepository _movimentacaoRepository;
        private readonly IEmailService _emailService;
        private readonly IConfiguracaoRepository _configuracaoRepository;
        private readonly IMapper _mapper;
        private readonly GerarXmlPedidoUseCase _gerarXmlPedidoUseCase;

        public FinalizarPedidoUseCase(
            IPedidoRepository pedidoRepository,
            IProdutoRepository produtoRepository,
            ITitulosRepository titulosRepository,
            IClienteRepository clienteRepository,
            IMovimentacaoEstoqueRepository movimentacaoRepository,
            IConfiguracaoRepository configuracaoRepository,
            IEmailService emailService,
            IMapper mapper,
            GerarXmlPedidoUseCase gerarXmlPedidoUseCase)
        {
            _pedidoRepository = pedidoRepository;
            _produtoRepository = produtoRepository;
            _titulosRepository = titulosRepository;
            _clienteRepository = clienteRepository;
            _movimentacaoRepository = movimentacaoRepository;
            _configuracaoRepository = configuracaoRepository;
            _emailService = emailService;
            _mapper = mapper;
            _gerarXmlPedidoUseCase = gerarXmlPedidoUseCase;
        }

        public async Task<PedidoResponseDTO> Executar(int pedidoId)
        {
            var pedido = await ObterPedido(pedidoId);
            var cliente = await ObterCliente(pedido.ClienteId);
            var configuracao = await _configuracaoRepository.ObterConfiguracao();

            pedido.Finalizar();

            await ProcessarEstoque(pedido);
            await GerarSaidas(pedido, cliente, configuracao);

            await _pedidoRepository.Atualizar(pedido);

            return _mapper.Map<PedidoResponseDTO>(pedido);
        }

        private async Task ProcessarEstoque(Pedido pedido)
        {
            foreach (var item in pedido.Itens)
            {
                var produto = await ObterProduto(item.ProdutoId);

                produto.FinalizarEstoquePedido(pedido.Tipo, item.Quantidade);

                var movimentacao = CriarMovimentacao(pedido, item);
                await _movimentacaoRepository.CadastrarMovimentacao(movimentacao);

                await _produtoRepository.Atualizar(produto);
            }
        }

        private async Task<Produto> ObterProduto(int produtoId)
        {
            var produto = await _produtoRepository.ObterPorId(produtoId);

            if (produto == null)
                throw new NotFoundException(ProdutoExceptions.Produto_NaoEncontrado);

            return produto;
        }

        private MovimentacaoEstoque CriarMovimentacao(Pedido pedido, PedidoItem item)
        {
            var tipoMovimentacao = pedido.Tipo == TipoPedido.Compra
                ? TipoMovimentacao.Entrada
                : TipoMovimentacao.Saida;

            var movimentacao = new MovimentacaoEstoque(
                item.ProdutoId,
                item.Quantidade,
                pedido.Id,
                Origem.Pedido,
                tipoMovimentacao);

            movimentacao.ConverterQuantidade();

            return movimentacao;
        }

        private async Task GerarSaidas(Pedido pedido, Cliente cliente, Configuracao configuracao)
        {
            if (configuracao.GeraXmlDoPedido)
                await _gerarXmlPedidoUseCase.Executar(pedido);

            if (configuracao.HabilitaEnvioDeEmail)
                await EnviarEmail(pedido, cliente);

            await GerarTitulo(pedido, cliente);
        }

        private async Task EnviarEmail(Pedido pedido, Cliente cliente)
        {
            await _emailService.EnviarPedidoCriadoAsync(
                cliente.Email,
                cliente.Nome,
                pedido.Id.ToString(),
                pedido.ValorTotal,
                pedido.Itens
            );
        }

        private async Task GerarTitulo(Pedido pedido, Cliente cliente)
        {
            var tipoTitulo = pedido.Tipo == TipoPedido.Compra
                ? TipoTitulo.Saída
                : TipoTitulo.Entrada;

            var titulo = new Titulo(
                cliente.Nome,
                pedido.ValorTotal,
                pedido.ClienteId,
                string.Empty,
                tipoTitulo,
                pedido.Id,
                Origem.Pedido);

            titulo.ConverterValor();

            await _titulosRepository.CadastrarTitulo(titulo);
        }

        private async Task<Pedido> ObterPedido(int pedidoId)
        {
            var pedido = await _pedidoRepository.ObterPorId(pedidoId);

            if (pedido == null)
                throw new NotFoundException(PedidosExceptions.Pedido_NaoEncontrado);

            return pedido;
        }

        private async Task<Cliente> ObterCliente(int clienteId)
        {
            var cliente = await _clienteRepository.ObterPorId(clienteId);

            if (cliente == null)
                throw new NotFoundException("Cliente não encontrado.");

            return cliente;
        }

    }
}