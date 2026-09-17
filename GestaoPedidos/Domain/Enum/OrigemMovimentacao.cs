namespace GestaoPedidos.Domain.Enum
{
    /// <summary>
    /// O que originou a movimentação. Substitui o texto livre em NomeOrigem:
    /// valor fechado não aceita grafia divergente e permite agrupar na análise.
    /// </summary>
    public enum OrigemMovimentacao
    {
        PedidoDeVenda = 0,
        PedidoDeCompra = 1,
        AjusteManual = 2
    }
}
