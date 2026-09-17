namespace GestaoPedidos.Domain.Entities
{
    /// <summary>
    /// Mensagem recebida pelo webhook da Cloud API do WhatsApp.
    /// É um registro do que chegou: nasce válida e não é alterada depois.
    /// </summary>
    public class MensagemWhatsApp
    {
        // Os limites são os mesmos aplicados às colunas em AppDbContext.
        // Ficam aqui para existir uma única fonte: se a entidade aceitasse
        // mais do que a coluna comporta, a falha só apareceria no SaveChanges.
        public const int TamanhoMaximoIdMensagem = 255;
        public const int TamanhoMaximoNumeroRemetente = 30;
        public const int TamanhoMaximoNomeRemetente = 255;
        public const int TamanhoMaximoTipoMensagem = 50;
        public const int TamanhoMaximoConteudo = 5000;
        public const int TamanhoMaximoPhoneNumberId = 50;

        public int Id { get; private set; }

        public string IdMensagemWhatsApp { get; private set; } = string.Empty;

        public string NumeroRemetente { get; private set; } = string.Empty;

        public string NomeRemetente { get; private set; } = string.Empty;

        public string TipoMensagem { get; private set; } = string.Empty;

        public string Conteudo { get; private set; } = string.Empty;

        public DateTime DataMensagem { get; private set; }

        public string PhoneNumberId { get; private set; } = string.Empty;

        protected MensagemWhatsApp() { }

        public MensagemWhatsApp(
            string idMensagemWhatsApp,
            string numeroRemetente,
            string nomeRemetente,
            string tipoMensagem,
            string conteudo,
            DateTime dataMensagem,
            string phoneNumberId)
        {
            IdMensagemWhatsApp = ExigirTexto(
                idMensagemWhatsApp,
                TamanhoMaximoIdMensagem,
                nameof(idMensagemWhatsApp));

            NumeroRemetente = ExigirTexto(
                numeroRemetente,
                TamanhoMaximoNumeroRemetente,
                nameof(numeroRemetente));

            TipoMensagem = ExigirTexto(
                tipoMensagem,
                TamanhoMaximoTipoMensagem,
                nameof(tipoMensagem));

            PhoneNumberId = ExigirTexto(
                phoneNumberId,
                TamanhoMaximoPhoneNumberId,
                nameof(phoneNumberId));

            // Nome e conteúdo podem chegar vazios de forma legítima:
            // contato sem perfil publicado, e mensagem que não é de texto.
            NomeRemetente = LimitarTexto(
                nomeRemetente,
                TamanhoMaximoNomeRemetente,
                nameof(nomeRemetente));

            Conteudo = LimitarTexto(
                conteudo,
                TamanhoMaximoConteudo,
                nameof(conteudo));

            if (dataMensagem.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException(
                    "A data da mensagem precisa estar em UTC.",
                    nameof(dataMensagem));
            }

            DataMensagem = dataMensagem;
        }

        private static string ExigirTexto(string? valor, int tamanhoMaximo, string parametro)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                throw new ArgumentException("Valor obrigatório não informado.", parametro);
            }

            return LimitarTexto(valor, tamanhoMaximo, parametro);
        }

        private static string LimitarTexto(string? valor, int tamanhoMaximo, string parametro)
        {
            var texto = (valor ?? string.Empty).Trim();

            // Truncar inventaria conteúdo diferente do que o remetente enviou.
            // Melhor recusar a mensagem e registrar do que gravar algo alterado.
            if (texto.Length > tamanhoMaximo)
            {
                throw new ArgumentOutOfRangeException(
                    parametro,
                    $"Excede o limite de {tamanhoMaximo} caracteres.");
            }

            return texto;
        }
    }
}
