using MGMBlazor.Domain.Entities;
using System.Xml.Linq;

namespace MGMBlazor.Services.Nfse;

public interface INfseService
{
    Task<RespostaEmissao> EmitirNotaAsync(NotaFiscal nota);
    Task<int> ObterProximoNumeroRpsAsync();

    Task<RespostaEmissao> VerificarSeRpsJaExisteNaPrefeitura(int rpsNumero);
    Task<RespostaEmissao> SubstituirNotaAsync(string numeroNotaExistente, NotaFiscal novaNota);
    Task<RespostaEmissao> CancelarNotaAsync(string numeroNota, string codigoMotivo);
}

