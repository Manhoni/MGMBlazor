using MGMBlazor.Domain.Entities;
using MGMBlazor.Models.Sicoob;

namespace MGMBlazor.Services.Sicoob;

public enum PeriodoParcelamento { Mensal, Quinzenal, Semanal }

public interface ISicoobService
{
    Task<List<Cobranca>> ListarCobrancasAsync(DateTime inicio, DateTime fim);
    Task<BoletoResponse?> IncluirBoletoAsync(int? notaFiscalId, BoletoRequest request);
    Task<BoletoResponse?> ConsultarBoletoAsync(long nossoNumero);
    Task<bool> BaixarBoletoAsync(long nossoNumero);
    Task<List<BoletoResponse>> GerarLoteBoletosAsync(int? notaId, BoletoRequest requestBase, int totalParcelas, PeriodoParcelamento periodo);
}