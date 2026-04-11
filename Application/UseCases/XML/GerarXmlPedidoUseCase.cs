using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Entities.Pedidos;
using System.Xml.Linq;

namespace GestaoPedidos.Application.UseCases.XML
{
    public class GerarXmlPedidoUseCase
    {
        private readonly IConfiguracaoRepository _configuracaoRepository;
        public GerarXmlPedidoUseCase(IConfiguracaoRepository configuracaoRepository)
        {
            _configuracaoRepository = configuracaoRepository;
        }
            
        public async Task Executar(Pedido pedido)
        {

            var configuracao = await _configuracaoRepository.ObterConfiguracao();

            string pasta = configuracao.DiretorioXML;
            if(!Directory.Exists(pasta))
                Directory.CreateDirectory(pasta);

            var xml = new XElement("Pedido",
               new XElement("Id", pedido.Id),
               new XElement("CodigoCliente", pedido.ClienteId),
               new XElement("ValorTotal", pedido.ValorTotal)
            );

            var itensXml = new XElement("Itens");
            foreach (var item in pedido.Itens)
            {
                itensXml.Add(new XElement("Item",
                    new XElement("CodigoProduto", item.ProdutoId),
                    new XElement("ProdutoNome", item.produto.Nome),
                    new XElement("Quantidade", item.Quantidade),
                    new XElement("ValorUnitario", item.Preco),
                    new XElement("Valor", item.SubTotal)));
            }
            xml.Add(itensXml);
            string nomeArquivo = $"Pedido_{pedido.Id}.xml";
            string caminho = Path.Combine(pasta, nomeArquivo);
            xml.Save( caminho );

        }
        
    }
}
