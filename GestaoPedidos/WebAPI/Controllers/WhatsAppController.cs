using GestaoPedidos.Application.DTO.WhatsApp;
using GestaoPedidos.Application.UseCases.WhatsApp.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestaoPedidos.WebAPI.Controllers
{
    /// <summary>
    /// Webhook do WhatsApp SEM autenticação: projeto de estudo, não vai para
    /// produção. Não há conferência de token nem de assinatura — qualquer um
    /// que conheça a URL consegue registrar mensagens.
    /// </summary>
    [ApiController]
    [Route("api/whatsapp")]
    public class WhatsAppController : ControllerBase
    {
        private readonly RegistrarMensagensRecebidasUseCase _registrarMensagens;

        public WhatsAppController(RegistrarMensagensRecebidasUseCase registrarMensagens)
        {
            _registrarMensagens = registrarMensagens;
        }

        /// <summary>
        /// Verificação inicial do webhook. A Meta chama uma única vez, ao
        /// cadastrar a URL, e espera receber de volta o desafio em texto puro.
        /// </summary>
        [HttpGet("webhook")]
        [AllowAnonymous]
        [Produces("text/plain")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult VerificarWebhook(
            [FromQuery(Name = "hub.mode")] string? mode,
            [FromQuery(Name = "hub.challenge")] string? challenge)
        {
            if (mode != "subscribe")
            {
                return BadRequest();
            }

            return Content(challenge ?? string.Empty, "text/plain");
        }

        /// <summary>
        /// Recebe as notificações de mensagem.
        /// </summary>
        [HttpPost("webhook")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ReceberWebhook(
            [FromBody] WebhookWhatsAppDTO webhook,
            CancellationToken cancellationToken)
        {
            await _registrarMensagens.Executar(webhook, cancellationToken);

            // A Meta reentrega a notificação enquanto não receber 200. O corpo
            // da resposta é ignorado por ela, então nada é devolvido aqui.
            return Ok();
        }
    }
}
