using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Exceptions;

namespace GestaoPedidos.Application.UseCases.Titulos.Commands
{
    public class RemoverBaixaUseCase
    {
        private readonly ITitulosRepository _titulosRepository;
        private readonly IMovimentosFinanceirosRepository _movimentosFinanceirosRepository;
        public RemoverBaixaUseCase (ITitulosRepository titulosRepository, IMovimentosFinanceirosRepository movimentosFinanceirosRepository)
        {
            _titulosRepository = titulosRepository;
            _movimentosFinanceirosRepository = movimentosFinanceirosRepository;
        }

        public async Task Executar(int id)
        {
            var titulo = await _titulosRepository.ObterTituloPorId(id);
            if (titulo == null)
                throw new BadRequestException("Titulo não encontrado!");
            var movimentoFinanceiro = await _movimentosFinanceirosRepository.ObterPorOrigem(titulo.Id);
            titulo.RemoverBaixaTitulo();
            await _titulosRepository.AtualizarTitulo(titulo);

            await _movimentosFinanceirosRepository.Deletar(movimentoFinanceiro.Id);

        }
    }
}
