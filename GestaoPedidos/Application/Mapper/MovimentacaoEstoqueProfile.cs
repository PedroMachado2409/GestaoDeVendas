using AutoMapper;
using GestaoPedidos.Application.DTO.MovimentacaoEstoque;
using GestaoPedidos.Domain.Entities;

namespace GestaoPedidos.Application.Mapper
{
    public class MovimentacaoEstoqueProfile : Profile
    {
        public MovimentacaoEstoqueProfile()
        {
            // Só entidade -> DTO. O caminho inverso permitiria montar uma
            // movimentação a partir de dados de fora, contornando o construtor
            // que valida a quantidade.
            CreateMap<MovimentacaoEstoque, MovimentacaoEstoqueResponseDTO>();
        }
    }
}
