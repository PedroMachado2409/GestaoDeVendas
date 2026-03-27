namespace GestaoPedidos.Application.DTO.Usuarios
{
    public class UsuarioUpdateSenhaDTO
    {
        public int Id { get; set; }
        public string SenhaAntiga { get; set; }
        public string NovaSenha { get; set; }
    }
}
