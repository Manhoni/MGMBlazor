using System.Globalization;
using System.Xml.Linq;
using MGMBlazor.Domain.Entities;
using MGMBlazor.Infrastructure.NFSe.Configuration;
using Microsoft.Extensions.Options;

namespace MGMBlazor.Infrastructure.NFSe.Abrasf;



public class AbrasfXmlBuilder
{
    private readonly NfseOptions _options;

    public AbrasfXmlBuilder(IOptions<NfseOptions> options)
    {
        _options = options.Value;
    }

    // ACRESCENTADO O .xsd NO FINAL
    private static readonly XNamespace Ns = "http://www.abrasf.org.br/nfse.xsd";

    public XDocument GerarXml(NotaFiscal nota)
    {
        // Estrutura interna exigida pelo XSD Abrasf 2.01
        var infDeclaracao = new XElement(Ns + "InfDeclaracaoPrestacaoServico",
            new XAttribute("Id", "R" + (nota.Id > 0 ? nota.Id : 1)),
            
            // Tag Rps interna (exigência do 2.01)
            new XElement(Ns + "Rps",
                new XElement(Ns + "IdentificacaoRps",
                    new XElement(Ns + "Numero", nota.Id > 0 ? nota.Id : 1),
                    new XElement(Ns + "Serie", "UNICA"),
                    new XElement(Ns + "Tipo", 1)
                ),
                new XElement(Ns + "DataEmissao", nota.DataEmissao.ToString("yyyy-MM-dd")), // Apenas data conforme XSD
                new XElement(Ns + "Status", 1)
            ),

            new XElement(Ns + "Competencia", nota.DataEmissao.ToString("yyyy-MM-dd")),

            new XElement(Ns + "Servico",
                new XElement(Ns + "Valores",
                    new XElement(Ns + "ValorServicos", nota.Servico.Valor.ToString("F2", CultureInfo.InvariantCulture)),
                    new XElement(Ns + "IssRetido", 2)
                ),
                new XElement(Ns + "ItemListaServico", nota.Servico.CodigoMunicipal),
                new XElement(Ns + "Discriminacao", nota.Servico.Descricao),
                new XElement(Ns + "CodigoMunicipio", nota.Tomador.MunicipioCodigoIbge),
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
                new XElement(Ns + "Contato",
                    new XElement(Ns + "Email", nota.Tomador.Email)
                )
            ),
            new XElement(Ns + "OptanteSimplesNacional", 1),
            new XElement(Ns + "IncentivoFiscal", 2)
        );

        return new XDocument(
            new XElement(Ns + "GerarNfseEnvio",
                new XElement(Ns + "Rps", infDeclaracao)
            )
        );
    }
}