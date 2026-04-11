namespace GestaoPedidos.Domain.Entities
{
    public class Configuracao
    {
        public int Id { get; set; } = 1;

        //Configurações de email
        public string NomeLoja { get; set; } = string.Empty;
        public string EmailOrigem {  get; set; } = string.Empty;
        public string Smtp {  get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public string Senha {  get; set; } = string.Empty;
        public int Porta { get; set; } = 0;
        public bool ConexaoSSl {  get; set; } = false;

        // Configuração estoque
        public bool PermiteEstoqueNegativo { get; set; } = false;
        
        //ConfiguraçãoEnvioDeEmail
        public bool HabilitaEnvioDeEmail { get; set; } = false;
        public bool EnviaXMLPorEmail { get; set; } = false;

        //Configuração XML
        public bool GeraXmlDoPedido { get; set; } = false;
        public string DiretorioXML { get; set; } = string.Empty;

        protected Configuracao() { }

        public void Atualizar(string nomeLoja, string emailOrigem, string smtp, string usuario, string senha, int porta, bool conexaoSsl, bool permiteEstoqueNegativo, bool habilitaEnvioDeEmail, bool enviaXmlPorEmail, bool geraXmlDoPedido, string diretorioXml)
        {
            NomeLoja = nomeLoja; 
            EmailOrigem = emailOrigem;
            Smtp = smtp; Usuario = usuario;
            Senha = senha; Porta = porta;
            ConexaoSSl = conexaoSsl; 
            PermiteEstoqueNegativo = permiteEstoqueNegativo; 
            HabilitaEnvioDeEmail = habilitaEnvioDeEmail; 
            EnviaXMLPorEmail = enviaXmlPorEmail;
            GeraXmlDoPedido = geraXmlDoPedido;
            DiretorioXML = diretorioXml;
        }
    }
}
