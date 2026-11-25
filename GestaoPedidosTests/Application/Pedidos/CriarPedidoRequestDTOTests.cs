using System.Text.Json;
using System.Text.Json.Serialization;
using GestaoPedidos.Application.DTO.Pedidos;
using GestaoPedidos.Application.Validators.Pedidos;
using GestaoPedidos.Domain.Enum;

namespace GestaoPedidosTests.Application.Pedidos
{
    [TestClass]
    public class CriarPedidoRequestDTOTests
    {
        [TestMethod]
        public void Deve_Rejeitar_Json_Quando_TipoMovimentacao_Nao_For_Informado()
        {
            var json = """{"pessoaId":1,"itens":[{"produtoId":1,"quantidade":1}]}""";

            Assert.ThrowsException<JsonException>(() =>
                JsonSerializer.Deserialize<CriarPedidoRequestDTO>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
        }

        [TestMethod]
        public void Deve_Rejeitar_Json_Quando_TipoMovimentacao_For_Nulo()
        {
            var json = """{"pessoaId":1,"tipoMovimentacao":null,"itens":[{"produtoId":1,"quantidade":1}]}""";

            Assert.ThrowsException<JsonException>(() =>
                JsonSerializer.Deserialize<CriarPedidoRequestDTO>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
        }

        [TestMethod]
        [DataRow("Entrada", TipoMovimentacao.Entrada)]
        [DataRow("Saida", TipoMovimentacao.Saida)]
        public void Deve_Aceitar_Json_Quando_TipoMovimentacao_For_Valido(string tipo, TipoMovimentacao esperado)
        {
            var json = "{\"pessoaId\":1,\"tipoMovimentacao\":\"" + tipo + "\",\"itens\":[{\"produtoId\":1,\"quantidade\":1}]}";
            var opcoes = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            opcoes.Converters.Add(new JsonStringEnumConverter());

            var dto = JsonSerializer.Deserialize<CriarPedidoRequestDTO>(json, opcoes)!;

            Assert.AreEqual(esperado, dto.tipoMovimentacao);
            Assert.IsTrue(new CriarPedidoRequestValidator().Validate(dto).IsValid);
        }

        [TestMethod]
        public void Deve_Rejeitar_TipoMovimentacao_Que_Nao_Existe_No_Enum()
        {
            var dto = new CriarPedidoRequestDTO
            {
                PessoaId = 1,
                tipoMovimentacao = (TipoMovimentacao)999,
                Itens = new() { new() { ProdutoId = 1, Quantidade = 1 } }
            };

            var resposta = new CriarPedidoRequestValidator().Validate(dto);

            Assert.IsFalse(resposta.IsValid);
            Assert.IsTrue(resposta.Errors.Any(e => e.PropertyName == nameof(dto.tipoMovimentacao)));
        }
    }
}
