using System.Globalization;
using System.Xml.Linq;
using MGMBlazor.Domain.Entities;

public class AbrasfXmlBuilder
{
    private static readonly XNamespace Ns = "http://www.abrasf.org.br/nfse";

    public XDocument GerarXml(NotaFiscal nota)
    {
        return new XDocument(
            new XElement(Ns + "EnviarLoteRpsEnvio",
                new XElement(Ns + "LoteRps",
                    new XElement(Ns + "NumeroLote", 1),
                    new XElement(Ns + "Cnpj", "CNPJ_DA_MGM"),
                    new XElement(Ns + "InscricaoMunicipal", "IM_DA_MGM"),
                    new XElement(Ns + "QuantidadeRps", 1),

                    new XElement(Ns + "ListaRps",
                        new XElement(Ns + "Rps",
                            new XElement(Ns + "InfRps",
                                new XElement(Ns + "IdentificacaoRps",
                                    new XElement(Ns + "Numero", 1),
                                    new XElement(Ns + "Serie", "UNICA"),
                                    new XElement(Ns + "Tipo", 1)
                                ),

                                new XElement(Ns + "DataEmissao",
                                    nota.DataEmissao.ToString("yyyy-MM-ddTHH:mm:ss")
                                ),

                                new XElement(Ns + "NaturezaOperacao", 1),
                                new XElement(Ns + "OptanteSimplesNacional", 2),
                                new XElement(Ns + "IncentivadorCultural", 2),
                                new XElement(Ns + "Status", 1),

                                new XElement(Ns + "Servico",
                                    new XElement(Ns + "Valores",
                                        new XElement(Ns + "ValorServicos",
                                            nota.Servico.Valor.ToString("F2",
                                            CultureInfo.InvariantCulture))
                                    ),
                                    new XElement(Ns + "ItemListaServico",
                                        nota.Servico.CodigoMunicipal),
                                    new XElement(Ns + "Discriminacao",
                                        nota.Servico.Descricao),
                                    new XElement(Ns + "CodigoMunicipio",
                                        nota.Tomador.MunicipioCodigoIbge)
                                ),

                                new XElement(Ns + "Prestador",
                                    new XElement(Ns + "Cnpj", "CNPJ_DA_MGM"),
                                    new XElement(Ns + "InscricaoMunicipal", "IM_DA_MGM")
                                ),

                                new XElement(Ns + "Tomador",
                                    new XElement(Ns + "IdentificacaoTomador",
                                        new XElement(Ns + "CpfCnpj",
                                            new XElement(Ns + "Cnpj",
                                                nota.Tomador.Cnpj)
                                        )
                                    ),
                                    new XElement(Ns + "RazaoSocial",
                                        nota.Tomador.RazaoSocial),
                                    new XElement(Ns + "Contato",
                                        new XElement(Ns + "Email",
                                            nota.Tomador.Email)
                                    )
                                )
                            )
                        )
                    )
                )
            )
        );
    }
}
