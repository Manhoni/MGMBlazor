using MGMBlazor.Domain.Entities;
using System.Xml.Linq;

namespace MGMBlazor.Services.Nfse;

public interface INfseService
{
    Task<RespostaEmissao> EmitirNotaAsync(NotaFiscal nota);
}

