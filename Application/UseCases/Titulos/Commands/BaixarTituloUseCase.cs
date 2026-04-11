using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Entities;
using GestaoPedidos.Domain.Enum;
using GestaoPedidos.Domain.Exceptions;

namespace GestaoPedidos.Application.UseCases.Titulos.Commands
{
    public class BaixarTituloUseCase
    {
        private readonly ITitulosRepository _titulosRepository;
        private readonly IMovimentosFinanceirosRepository _movimentosFinanceirosRepository;
        
        public BaixarTituloUseCase (ITitulosRepository titulosRepository, IMovimentosFinanceirosRepository movimentosFinanceirosRepository)
        {
            _titulosRepository = titulosRepository;
            _movimentosFinanceirosRepository = movimentosFinanceirosRepository;
        }

        public async Task Executar(int id)
        {
            var titulo = await _titulosRepository.ObterTituloPorId(id);
            if (titulo == null)
                throw new BadRequestException("Titulo não encontrado!");
            titulo.BaixarTitulo();
            await _titulosRepository.AtualizarTitulo(titulo);
            var movimentoFinanceiro = new MovimentoFinanceiro(titulo.NomeTitulo, titulo.Id, titulo.ValorTitulo, titulo.Id, Origem.Titulo);
            await _movimentosFinanceirosRepository.CadastrarMovimento(movimentoFinanceiro);
        }
    }
}
