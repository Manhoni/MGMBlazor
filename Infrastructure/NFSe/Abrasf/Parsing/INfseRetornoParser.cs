using MGMBlazor.Domain.Entities;

namespace MGMBlazor.Infrastructure.NFSe.Abrasf.Parsing;

public interface INfseRetornoParser
{
    void Processar(string xmlRetorno, RespostaEmissao resposta);
}
