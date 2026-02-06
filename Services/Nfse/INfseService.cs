using MGMBlazor.Domain.Entities;
using System.Xml.Linq;

namespace MGMBlazor.Services.Nfse;

public interface INfseService
{
    Task<int> ObterProximoNumeroRpsAsync();
    Task<RespostaEmissao> EmitirNotaAsync(NotaFiscal nota);
    Task<RespostaEmissao> VerificarSeRpsJaExisteNaPrefeitura(int rpsNumero);
    Task<RespostaEmissao> SubstituirNotaAsync(string numeroNotaExistente, NotaFiscal novaNota);
    Task<RespostaEmissao> CancelarNotaAsync(string numeroNota, string codigoMotivo);
    Task<List<NotaFiscalEmitida>> ListarNotasAsync(DateTime inicio, DateTime fim);
    string GerarLinkConsultaPublica(string numeroNota, string codigoVerificacao);
}

