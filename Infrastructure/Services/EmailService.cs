using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Entities.Pedidos;
using MimeKit;
using MailKit.Net.Smtp;

namespace GestaoPedidos.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguracaoRepository _configuracaoRepository;
        public EmailService(IConfiguracaoRepository configuracaoRepository)
        {
            _configuracaoRepository = configuracaoRepository;
        }

        public async Task EnviarPedidoCriadoAsync(
            string emailDestino,
            string nomeCliente,
            string numeroPedido,
            decimal valorTotal,
            IEnumerable<PedidoItem> itens)
        {
            var cfg = await _configuracaoRepository.ObterConfiguracao();
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(cfg.NomeLoja, cfg.EmailOrigem));
            message.To.Add(new MailboxAddress(nomeCliente, emailDestino));
            message.Subject = "Pedido Confirmado!";

            var itensHtml = string.Join("", itens.Select(i => $@"
                <tr>
                    <td style='padding:8px;border-bottom:1px solid #eee;'>Produto #{i.produto.Nome}</td>
                    <td style='padding:8px;border-bottom:1px solid #eee;text-align:center;'>{i.Quantidade}</td>
                    <td style='padding:8px;border-bottom:1px solid #eee;text-align:right;'>R$ {i.Preco:N2}</td>
                    <td style='padding:8px;border-bottom:1px solid #eee;text-align:right;'>R$ {i.SubTotal:N2}</td>
                </tr>
            "));

            message.Body = new TextPart("html")
            {
                Text = $@"
                <div style='font-family:Arial, sans-serif; background-color:#eff6ff; padding:20px;'>
                    <div style='max-width:600px; margin:auto; background:white; border-radius:10px; overflow:hidden; box-shadow:0 4px 12px rgba(0,0,0,0.1);'>
                        
                        <!-- Header -->
                        <div style='background:#2563eb; color:white; padding:20px; text-align:center;'>
                            <h2 style='margin:0;'>Pedido Confirmado</h2>
                        </div>

                        <!-- Conteúdo -->
                        <div style='padding:20px; color:#333;'>
                            <p>Olá <strong>{nomeCliente}</strong>,</p>
                            <p>Seu pedido <strong>#{numeroPedido}</strong> foi criado com sucesso!</p>

                            <h3 style='margin-top:20px; color:#2563eb;'>Detalhes do Pedido</h3>
                            <table style='width:100%; border-collapse:collapse; margin-top:10px;'>
                                <thead>
                                    <tr style='background:#dbeafe;'>
                                        <th style='text-align:left; padding:10px;'>Produto</th>
                                        <th style='text-align:center; padding:10px;'>Qtd</th>
                                        <th style='text-align:right; padding:10px;'>Preço</th>
                                        <th style='text-align:right; padding:10px;'>Subtotal</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {itensHtml}
                                </tbody>
                            </table>

                            <div style='margin-top:20px; text-align:right;'>
                                <h2 style='color:#2563eb;'>Total: R$ {valorTotal:N2}</h2>
                            </div>

                            <p style='margin-top:20px;'>Obrigado pela sua compra!</p>
                        </div>

                        <!-- Rodapé -->
                        <div style='background:#eff6ff; padding:15px; text-align:center; font-size:12px; color:#555;'>
                            <p style='margin:0;'>Nexus © {DateTime.Now.Year}</p>
                        </div>

                    </div>
                </div>
                "
            };

            using var client = new SmtpClient();

            await client.ConnectAsync(cfg.Smtp, cfg.Porta, cfg.ConexaoSSl);
            await client.AuthenticateAsync(cfg.Usuario, cfg.Senha);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}