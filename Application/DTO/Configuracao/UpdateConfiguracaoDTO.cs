namespace GestaoPedidos.Application.DTO.Configuracao
{
    public class UpdateConfiguracaoDTO
    {
        public string NomeLoja { get; set; } = string.Empty;
        public string EmailOrigem { get; set; } = string.Empty;
        public string Smtp { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public int Porta { get; set; } = 0;
        public bool ConexaoSSl { get; set; } = false;

        // Configuração estoque
        public bool PermiteEstoqueNegativo { get; set; } = false;

        //ConfiguraçãoEnvioDeEmail
        public bool HabilitaEnvioDeEmail { get; set; } = false;
        public bool EnviaXMLPorEmail { get; set; } = false;

        //Configuração XML
        public bool GeraXmlDoPedido { get; set; } = false;
        public string DiretorioXML { get; set; } = string.Empty;
    }
}
