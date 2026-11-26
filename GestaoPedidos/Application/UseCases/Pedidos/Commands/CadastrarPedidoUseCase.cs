using AutoMapper;
using GestaoPedidos.Application.DTO.Pedidos;
using GestaoPedidos.Application.UseCases.Usuarios.Queries;
using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Entities;
using GestaoPedidos.Domain.Entities.Pedidos;
using GestaoPedidos.Domain.Enum;
using GestaoPedidos.Domain.Exceptions;
using GestaoPedidos.Domain.Exceptions.Pessoas;
using GestaoPedidos.Domain.Exceptions.Produtos;
using GestaoPedidos.Domain.Exceptions.TituloFinanceiro;
using Microsoft.AspNetCore.Mvc;

namespace GestaoPedidos.Application.UseCases.Pedidos.Commands
{
    public class CadastrarPedidoUseCase
    {
        private readonly IPedidoRepository _pedidoRepository;
        private readonly IProdutoRepository _produtoRepository;
        private readonly IPessoaRepository _pessoaRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ITituloFinanceiroRepository _tituloFinanceiroRepository;
        private readonly ObterUsuarioAutenticadoUseCase _obterUsuarioAutenticado;

        public CadastrarPedidoUseCase(
            IPedidoRepository pedidoRepository,
            IProdutoRepository produtoRepository,
            IPessoaRepository pessoaRepository,
            IUnitOfWork unitOfWork,
            ITituloFinanceiroRepository tituloFinanceiroRepository,
            ObterUsuarioAutenticadoUseCase obterUsuarioAutenticadoUseCase,
            IMapper mapper)
        {
            _pedidoRepository = pedidoRepository;
            _produtoRepository = produtoRepository;
            _pessoaRepository = pessoaRepository;
            _obterUsuarioAutenticado = obterUsuarioAutenticadoUseCase;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _tituloFinanceiroRepository = tituloFinanceiroRepository;
        }

        public Task<PedidoResponseDTO> Executar(CriarPedidoRequestDTO dto)
            => _unitOfWork.ExecutarEmTransacao(async () =>
            {
                var pessoa = await _pessoaRepository.ObterPorId(dto.PessoaId)
                    ?? throw new NotFoundException(PessoasExceptions.Pessoa_NaoEncontrado);

                if (!pessoa.Ativo)
                {
                    throw new ConflictException(PessoasExceptions.Pessoa_Inativo);
                }

                var usuario = await _obterUsuarioAutenticado.Executar();
                var produtos = await _produtoRepository.ObterPorIds(
                    dto.Itens.Select(i => i.ProdutoId));
                var produtosPorId = produtos.ToDictionary(p => p.Id);
                var itens = new List<PedidoItem>(dto.Itens.Count);

                var tipoFinanceiro = TipoTituloFinanceiro.Receber;

                if (dto.tipoMovimentacao == TipoMovimentacao.Entrada && usuario.Role != UserRole.Admin)
                {
                    throw new ForbiddenException("Usuário sem permissão para realizar Pedidos de Compras");
                }

                foreach (var itemDto in dto.Itens)
                {
                    if (!produtosPorId.TryGetValue(itemDto.ProdutoId, out var produto))
                    {
                        throw new NotFoundException(ProdutoExceptions.Produto_NaoEncontrado);
                    }

                    if (!produto.Ativo)
                    {
                        throw new ConflictException(ProdutoExceptions.Produto_Inativo);
                    }

                    if (dto.tipoMovimentacao == TipoMovimentacao.Saida)
                    {
                        produto.ReservarQuantidadeVenda(itemDto.Quantidade);
                         tipoFinanceiro = TipoTituloFinanceiro.Receber;
                    }
                    else if (dto.tipoMovimentacao == TipoMovimentacao.Entrada)
                    {
                        produto.ReservarQuantidadeCompra(itemDto.Quantidade);
                        tipoFinanceiro = TipoTituloFinanceiro.Pagar;
                    }
                    itens.Add(new PedidoItem(produto.Id, produto.Preco, itemDto.Quantidade));
                }
                var totalPedido = itens.Sum(i => i.SubTotal);
                var pedido = new Pedido(dto.PessoaId, itens, usuario.Id, totalPedido, dto.tipoMovimentacao);

                await _pedidoRepository.Cadastrar(pedido);
                await _unitOfWork.SalvarAlteracoes();

                var tituloFinanceiro = new TituloFinanceiro(pedido.ClienteId,
                    string.Format(TItuloExceptions.Titulo_DescricaoPedido, pedido.Id),
                    tipoFinanceiro, pedido.ValorTotal, pedido.DataCadastro, pedido.Id, OrigemTitulo.Pedido);

                await _tituloFinanceiroRepository.Cadastrar(tituloFinanceiro);
                await _unitOfWork.SalvarAlteracoes();

                var resposta = _mapper.Map<PedidoResponseDTO>(pedido);
                return resposta;
            });
    }
}
