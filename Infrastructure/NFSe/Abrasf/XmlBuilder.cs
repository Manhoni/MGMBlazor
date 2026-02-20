using System.Globalization;
using System.Xml.Linq;
using MGMBlazor.Domain.Entities;
using MGMBlazor.Infrastructure.NFSe.Configuration;
using Microsoft.Extensions.Options;

namespace MGMBlazor.Infrastructure.NFSe.Abrasf;

public class AbrasfXmlBuilder
{
    private readonly NfseOptions _options;
    private static readonly XNamespace Ns = "http://www.abrasf.org.br/nfse.xsd";

    public AbrasfXmlBuilder(IOptions<NfseOptions> options)
    {
        _options = options.Value;
    }

    public XElement MontarConteudoRps(NotaFiscal nota)
    {
        return new XElement(Ns + "InfDeclaracaoPrestacaoServico",
            new XAttribute("Id", "R" + nota.Id),

            new XElement(Ns + "Rps",
                new XElement(Ns + "IdentificacaoRps",
                    new XElement(Ns + "Numero", nota.Id),
                    new XElement(Ns + "Serie", "1"),
                    new XElement(Ns + "Tipo", 1)
                ),
                new XElement(Ns + "DataEmissao", nota.DataEmissao.ToString("yyyy-MM-dd")),
                new XElement(Ns + "Status", 1)
            ),

            new XElement(Ns + "Competencia", nota.DataEmissao.ToString("yyyy-MM-dd")),

            new XElement(Ns + "Servico",
                new XElement(Ns + "Valores",
                    new XElement(Ns + "ValorServicos", nota.Valor.ToString("F2", CultureInfo.InvariantCulture))

                ),
                new XElement(Ns + "IssRetido", 2),
                new XElement(Ns + "ItemListaServico", nota.Servico.CodigoMunicipal),
                new XElement(Ns + "Discriminacao", nota.Servico.Descricao),
                new XElement(Ns + "CodigoMunicipio", _options.Prestador.CodigoMunicipio),
                new XElement(Ns + "ExigibilidadeISS", 1)
            ),

            new XElement(Ns + "Prestador",
                new XElement(Ns + "CpfCnpj",
                    new XElement(Ns + "Cnpj", _options.Prestador.Cnpj)
                ),
                new XElement(Ns + "InscricaoMunicipal", _options.Prestador.InscricaoMunicipal)
            ),

            new XElement(Ns + "Tomador",
                new XElement(Ns + "IdentificacaoTomador",
                    new XElement(Ns + "CpfCnpj",
                        new XElement(Ns + "Cnpj", nota.Tomador.Cnpj)
                    )
                ),
                new XElement(Ns + "RazaoSocial", nota.Tomador.RazaoSocial),
                new XElement(Ns + "Endereco",
                    new XElement(Ns + "Endereco", nota.Tomador.Endereco),
                    new XElement(Ns + "Numero", nota.Tomador.Numero),
                    new XElement(Ns + "Bairro", nota.Tomador.Bairro),
                    new XElement(Ns + "CodigoMunicipio", nota.Tomador.MunicipioCodigoIbge),
                    new XElement(Ns + "Uf", nota.Tomador.Uf),
                    new XElement(Ns + "CodigoPais", "1058"),
                    new XElement(Ns + "Cep", nota.Tomador.Cep)
                ),
                new XElement(Ns + "Contato",
                    new XElement(Ns + "Email", nota.Tomador.Email)
                )
            ),

            new XElement(Ns + "RegimeEspecialTributacao", _options.Prestador.RegimeEspecialTributacao),
            new XElement(Ns + "OptanteSimplesNacional", 1),
            new XElement(Ns + "IncentivoFiscal", 2)
        );
    }

    public XDocument GerarXml(NotaFiscal nota)
    {
        return new XDocument(
            new XElement(Ns + "GerarNfseEnvio",
                new XElement(Ns + "Rps", MontarConteudoRps(nota))
            )
        );
    }
    public XDocument MontarXmlConsultaRps(int rpsNumero)
    {
        var cnpj = _options.Prestador.Cnpj;
        var im = _options.Prestador.InscricaoMunicipal;

        return new XDocument(
            new XElement(Ns + "ConsultarNfseRpsEnvio",
                new XElement(Ns + "IdentificacaoRps",
                    new XElement(Ns + "Numero", rpsNumero),
                    new XElement(Ns + "Serie", "1"),
                    new XElement(Ns + "Tipo", 1)
                ),
                new XElement(Ns + "Prestador",
                    new XElement(Ns + "CpfCnpj",
                        new XElement(Ns + "Cnpj", cnpj)
                    ),
                    new XElement(Ns + "InscricaoMunicipal", im)
                )
            )
        );
    }
    public XDocument MontarXmlCancelamento(string numeroNota, string codigoCancelamento = "1")
    {
        // 1 = Erro na emissão, 2 = Serviço não prestado, etc.
        return new XDocument(
            new XElement(Ns + "CancelarNfseEnvio",
                new XElement(Ns + "Pedido",
                    new XElement(Ns + "InfPedidoCancelamento", new XAttribute("Id", "C" + numeroNota),
                        new XElement(Ns + "IdentificacaoNfse",
                            new XElement(Ns + "Numero", numeroNota),
                            new XElement(Ns + "CpfCnpj", new XElement(Ns + "Cnpj", _options.Prestador.Cnpj)),
                            new XElement(Ns + "InscricaoMunicipal", _options.Prestador.InscricaoMunicipal),
                            new XElement(Ns + "CodigoMunicipio", "4115200") // Maringá
                        ),
                        new XElement(Ns + "CodigoCancelamento", codigoCancelamento)
                    )
                )
            )
        );
    }
    public XDocument MontarXmlSubstituicao(string numeroNotaSubstituir, NotaFiscal novaNota, string codigoCancelamento = "1")
    {
        // A substituição é um cancelamento e uma emissão no mesmo envelope
        return new XDocument(
            new XElement(Ns + "SubstituirNfseEnvio",
                new XElement(Ns + "SubstituicaoNfse", new XAttribute("Id", "S" + numeroNotaSubstituir),
                    new XElement(Ns + "Pedido",
                        new XElement(Ns + "InfPedidoCancelamento", new XAttribute("Id", "C" + numeroNotaSubstituir),
                            new XElement(Ns + "IdentificacaoNfse",
                                new XElement(Ns + "Numero", numeroNotaSubstituir),
                                new XElement(Ns + "CpfCnpj", new XElement(Ns + "Cnpj", _options.Prestador.Cnpj)),
                                new XElement(Ns + "InscricaoMunicipal", _options.Prestador.InscricaoMunicipal),
                                new XElement(Ns + "CodigoMunicipio", "4115200")
                            ),
                            new XElement(Ns + "CodigoCancelamento", codigoCancelamento)
                        )
                    ),
                    new XElement(Ns + "Rps", MontarConteudoRps(novaNota)) // Reaproveita a lógica de montagem de nota
                )
            )
        );
    }
}