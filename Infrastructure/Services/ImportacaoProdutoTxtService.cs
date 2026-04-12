using GestaoPedidos.Domain.Entities;
using System.Globalization;

namespace GestaoPedidos.Infrastructure.Services
{
    public class ImportacaoProdutoTxtService
    {
        public List<Produto> Importar(Stream stream)
        {
            var produtos = new List<Produto>();

            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    var linha = reader.ReadLine();

                    if (string.IsNullOrWhiteSpace(linha))
                        continue;

                    var colunas = linha.Split(';');

                    if (colunas.Length < 4)
                        continue;

                    var nome = colunas[0];
                    var marca = colunas[1];

                    if (!int.TryParse(colunas[2], out int estoque))
                        continue;

                    if (!decimal.TryParse(colunas[3],
                            NumberStyles.Any,
                            new CultureInfo("pt-BR"),
                            out decimal preco))
                        continue;

                    var produto = new Produto(nome, marca, estoque, preco);

                    produtos.Add(produto);
                }
            }

            return produtos;
        }
     }
}
