using GestaoPedidos.Domain.Entities;

namespace GestaoPedidos.Domain.Abstractions
{
    public interface IMensagemWhatsAppRepository
    {
        /// <summary>
        /// Grava a mensagem se ela ainda não existir.
        /// Devolve <c>false</c> quando o mesmo identificador da Meta já estava
        /// registrado, o que torna a operação segura para reentregas.
        /// </summary>
        Task<bool> AdicionarSeNaoExistirAsync(
            MensagemWhatsApp mensagem,
            CancellationToken cancellationToken = default);
    }
}
