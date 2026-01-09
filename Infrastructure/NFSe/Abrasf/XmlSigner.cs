using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using System.Xml.Linq;

public class XmlSigner
{
    public string AssinarLoteRps(XDocument xml, X509Certificate2 certificado)
    {
        Console.WriteLine("Iniciando assinatura do LoteRps");

        var xmlDoc = new XmlDocument
        {
            PreserveWhitespace = true
        };

        using (var reader = xml.CreateReader())
        {
            xmlDoc.Load(reader);
        }

        var nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
        nsManager.AddNamespace("nfse", "http://www.abrasf.org.br/nfse");

        var loteRpsNode = xmlDoc.SelectSingleNode("//nfse:LoteRps", nsManager)
            ?? throw new Exception("Nó LoteRps não encontrado.");

        var signedXml = new SignedXml(xmlDoc)
        {
            SigningKey = certificado.GetRSAPrivateKey()
        };

        var reference = new Reference
        {
            Uri = ""
        };

        reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
        reference.AddTransform(new XmlDsigC14NTransform());

        signedXml.AddReference(reference);

        signedXml.KeyInfo = new KeyInfo();
        signedXml.KeyInfo.AddClause(new KeyInfoX509Data(certificado));

        signedXml.ComputeSignature();

        var xmlSignature = signedXml.GetXml();
        loteRpsNode.AppendChild(xmlDoc.ImportNode(xmlSignature, true));

        return xmlDoc.OuterXml;
        //return XDocument.Parse(xmlDoc.OuterXml); // ou retornar uma string para não dar problema pra converter para string depois
    }
}
