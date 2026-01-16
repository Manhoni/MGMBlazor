using MGMBlazor.Models.Sicoob;

namespace MGMBlazor.Services.Sicoob;

public interface ISicoobService
{
    Task<BoletoResponse?> IncluirBoletoAsync(int notaFiscalId, BoletoRequest request);
    Task<BoletoResponse?> ConsultarBoletoAsync(long nossoNumero);
    Task<bool> BaixarBoletoAsync(long nossoNumero);
}