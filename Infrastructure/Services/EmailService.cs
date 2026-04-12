using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Entities.Pedidos;
using MimeKit;
using MailKit.Net.Smtp;

namespace GestaoPedidos.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguracaoRepository _configuracaoRepository;
        private readonly string _logPath = @"C:\Logs\email_log.txt";

        public EmailService(IConfiguracaoRepository configuracaoRepository)
        {
            _configuracaoRepository = configuracaoRepository;

            // Garante que a pasta existe
            var dir = Path.GetDirectoryName(_logPath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }

        public async Task EnviarPedidoCriadoAsync(
            string emailDestino,
            string nomeCliente,
            string numeroPedido,
            decimal valorTotal,
            IEnumerable<PedidoItem> itens)
        {
            try
            {
                var cfg = await _configuracaoRepository.ObterConfiguracao();

                if (cfg == null)
                    throw new Exception("Configuração não encontrada");

                if (!File.Exists(cfg.CaminhoTemplateEmailPedido))
                    throw new Exception("Template de email não encontrado");

                var html = await File.ReadAllTextAsync(cfg.CaminhoTemplateEmailPedido);
                var listaItens = itens.ToList();

               
                var inicio = html.IndexOf("{{ITEM_TEMPLATE}}");
                var fim = html.IndexOf("{{END_ITEM_TEMPLATE}}");

                var itemTemplate = html.Substring(
                    inicio + "{{ITEM_TEMPLATE}}".Length,
                    fim - (inicio + "{{ITEM_TEMPLATE}}".Length)
                );

                var blocoCompleto = html.Substring(
                    inicio,
                    (fim + "{{END_ITEM_TEMPLATE}}".Length) - inicio
                );

                html = html.Replace(blocoCompleto, "");

                // MONTA ITENS
                var itensHtml = "";

                foreach (var i in listaItens)
                {
                    var linha = itemTemplate
                        .Replace("{{PRODUTO}}", i.produto.Nome)
                        .Replace("{{QUANTIDADE}}", i.Quantidade.ToString())
                        .Replace("{{PRECO}}", i.Preco.ToString("N2"))
                        .Replace("{{SUBTOTAL}}", i.SubTotal.ToString("N2"));

                    itensHtml += linha;
                }

                // VARIÁVEIS
                var variaveis = new Dictionary<string, string>
                {
                    { "NOME_CLIENTE", nomeCliente },
                    { "NUMERO_PEDIDO", numeroPedido },
                    { "VALOR_TOTAL", valorTotal.ToString("N2") },
                    { "DATA", DateTime.UtcNow.ToString("dd/MM/yyyy") },
                    { "ANO", DateTime.UtcNow.Year.ToString() },
                    { "NOME_LOJA", cfg.NomeLoja },
                    { "ITENS", itensHtml }
                };

                foreach (var item in variaveis)
                {
                    html = html.Replace($"{{{{{item.Key}}}}}", item.Value);
                }

                // MONTA EMAIL
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(cfg.NomeLoja, cfg.EmailOrigem));
                message.To.Add(new MailboxAddress(nomeCliente, emailDestino));
                message.Subject = "Pedido Confirmado!";
                message.Body = new TextPart("html") { Text = html };

                using var client = new SmtpClient();

                await client.ConnectAsync(cfg.Smtp, cfg.Porta, cfg.ConexaoSSl);
                await client.AuthenticateAsync(cfg.Usuario, cfg.Senha);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                // LOG DE SUCESSO
                await LogSucessoAsync(emailDestino, numeroPedido);
            }
            catch (Exception ex)
            {
                // LOG DE ERRO LIMPO
                await LogErroEmailAsync(emailDestino, numeroPedido, ex);
            }
        }

        // =========================
        // LOG DE SUCESSO
        // =========================
        private async Task LogSucessoAsync(string emailDestino, string numeroPedido)
        {
            try
            {
                var log = $@"
                ==================================================
                [SUCESSO EMAIL]
                Data: {DateTime.Now:dd/MM/yyyy HH:mm:ss}
                Destino: {emailDestino}
                Pedido: {numeroPedido}
                ==================================================
                ";

                await File.AppendAllTextAsync(_logPath, log);
            }
            catch { }
        }

        // =========================
        // LOG DE ERRO
        // =========================
        private async Task LogErroEmailAsync(string emailDestino, string numeroPedido, Exception ex)
        {
            try
            {
                var mensagemLimpa = LimparMensagemErro(ex.Message);

                var log = $@"
            ==================================================
            [ERRO EMAIL]
            Data: {DateTime.Now:dd/MM/yyyy HH:mm:ss}
            Destino: {emailDestino}
            Pedido: {numeroPedido}
            Erro: {mensagemLimpa}
            ==================================================
            ";

                await File.AppendAllTextAsync(_logPath, log);
            }
            catch { }
        }

        // =========================
        // LIMPA MENSAGEM DE ERRO
        // =========================
        private string LimparMensagemErro(string mensagem)
        {
            if (string.IsNullOrWhiteSpace(mensagem))
                return "Erro desconhecido";

            mensagem = mensagem.Replace("\r", "").Replace("\n", " ");

            var indice = mensagem.IndexOf("For more information", StringComparison.OrdinalIgnoreCase);
            if (indice > 0)
            {
                mensagem = mensagem.Substring(0, indice);
            }

            return mensagem.Trim();
        }
    }
}