using MGMBlazor.Domain.Entities;

namespace MGMBlazor.Infrastructure.NFSe.Abrasf.Parsing;

public interface INfseRetornoParser
{
    void Processar(string xmlRetorno, RespostaEmissao resposta);
    public class DadosSubstituicaoDTO
    {
        public Cliente Tomador { get; set; } = new();
        public string ItemListaServico { get; set; } = string.Empty;
    }
    DadosSubstituicaoDTO ExtrairTomadorDoXml(string xmlSalvoNoBanco);
}
