using GestaoPedidos.Application.DTO.WhatsApp;
using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Entities;

namespace GestaoPedidos.Application.UseCases.WhatsApp.Commands
{
    /// <summary>
    /// Registra as mensagens recebidas em uma notificação do webhook.
    /// </summary>
    public class RegistrarMensagensRecebidasUseCase
    {
        private const string TipoTexto = "text";

        private readonly IMensagemWhatsAppRepository _repository;
        private readonly ILogger<RegistrarMensagensRecebidasUseCase> _logger;

        public RegistrarMensagensRecebidasUseCase(
            IMensagemWhatsAppRepository repository,
            ILogger<RegistrarMensagensRecebidasUseCase> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        /// <summary>
        /// Devolve quantas mensagens foram gravadas nesta chamada. Reentregas
        /// não contam, porque a mensagem já estava registrada.
        /// </summary>
        public async Task<int> Executar(
            WebhookWhatsAppDTO webhook,
            CancellationToken cancellationToken = default)
        {
            var registradas = 0;

            foreach (var (mensagem, valor) in ExtrairMensagens(webhook))
            {
                if (await RegistrarAsync(mensagem, valor, cancellationToken))
                {
                    registradas++;
                }
            }

            return registradas;
        }

        /// <summary>
        /// Achata a estrutura aninhada do webhook (entry / changes / messages)
        /// para que o registro não precise de quatro laços encaixados.
        /// </summary>
        private static IEnumerable<(MensagemRecebidaWhatsAppDTO Mensagem, ValorWhatsAppDTO Valor)>
            ExtrairMensagens(WebhookWhatsAppDTO webhook)
            => (webhook.Entry ?? new List<EntradaWhatsAppDTO>())
                .SelectMany(entrada => entrada.Changes ?? new List<AlteracaoWhatsAppDTO>())
                .Select(alteracao => alteracao.Value)
                .Where(valor => valor is not null)
                .SelectMany(valor => (valor.Messages ?? new List<MensagemRecebidaWhatsAppDTO>())
                    .Select(mensagem => (mensagem, valor)));

        private async Task<bool> RegistrarAsync(
            MensagemRecebidaWhatsAppDTO mensagem,
            ValorWhatsAppDTO valor,
            CancellationToken cancellationToken)
        {
            MensagemWhatsApp entidade;

            try
            {
                entidade = Converter(mensagem, valor);
            }
            catch (ArgumentException excecao)
            {
                // Uma mensagem que não podemos representar fielmente não pode
                // derrubar o lote inteiro: as demais do mesmo webhook são
                // válidas e a Meta não reenvia parcialmente. Fica o registro
                // em log com o identificador, que permite recuperar depois.
                _logger.LogWarning(
                    excecao,
                    "Mensagem {IdMensagem} descartada por conteúdo inválido",
                    mensagem.Id);

                return false;
            }

            var gravou = await _repository.AdicionarSeNaoExistirAsync(entidade, cancellationToken);

            if (!gravou)
            {
                _logger.LogInformation(
                    "Mensagem {IdMensagem} ignorada por já estar registrada",
                    mensagem.Id);
            }

            return gravou;
        }

        private static MensagemWhatsApp Converter(
            MensagemRecebidaWhatsAppDTO mensagem,
            ValorWhatsAppDTO valor)
            => new(
                mensagem.Id,
                mensagem.From,
                ObterNomeRemetente(valor.Contacts, mensagem.From),
                mensagem.Type,
                ObterConteudo(mensagem),
                ConverterTimestamp(mensagem.Timestamp),
                valor.Metadata.PhoneNumberId);

        private static string ObterNomeRemetente(
            List<ContatoWhatsAppDTO>? contatos,
            string numeroRemetente)
            => contatos?
                .FirstOrDefault(contato => contato.WaId == numeroRemetente)?
                .Profile?
                .Name
               ?? string.Empty;

        private static string ObterConteudo(MensagemRecebidaWhatsAppDTO mensagem)
            => mensagem.Type == TipoTexto
                ? mensagem.Text?.Body ?? string.Empty
                : string.Empty;

        private static DateTime ConverterTimestamp(string timestamp)
        {
            // Sem substituir por "agora": a hora de recebimento não é a hora
            // de envio, e gravá-la no lugar inventaria um dado que ninguém
            // conseguiria distinguir depois do verdadeiro.
            if (!long.TryParse(timestamp, out var segundosUnix))
            {
                throw new ArgumentException(
                    $"Timestamp inválido: '{timestamp}'.",
                    nameof(timestamp));
            }

            return DateTimeOffset.FromUnixTimeSeconds(segundosUnix).UtcDateTime;
        }
    }
}
