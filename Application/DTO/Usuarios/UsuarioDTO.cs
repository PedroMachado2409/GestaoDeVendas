using GestaoPedidos.Domain.Enum;

namespace GestaoPedidos.Application.DTO.Usuarios
{
    public class UsuarioDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; } 
        public string Email { get; set; } 
        public UserRole Role { get; set; }
        public DateTime DataCadastro { get; set; } 
        public bool Ativo { get; set; } 
    }
}
