using GestaoPedidos.Domain.Enum;

namespace GestaoPedidos.Application.DTO.Usuarios
{
    public class UsuarioCreateDTO
    {
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Senha { get; set; }
        public UserRole Role { get; set; }
    }
}
