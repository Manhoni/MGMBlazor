using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using System.Xml.Linq;

public class XmlSigner
{
    public string AssinarLoteRps(XDocument xml, X509Certificate2 certificado)
    {
        var xmlDoc = new XmlDocument { PreserveWhitespace = true };
        using (var reader = xml.CreateReader()) { xmlDoc.Load(reader); }

        var nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
        // ADICIONADO O .xsd AQUI TAMBÉM
        nsManager.AddNamespace("ns", "http://www.abrasf.org.br/nfse.xsd");

        // BUSCA A TAG CORRETA DO 2.01
        var nodeToSign = xmlDoc.SelectSingleNode("//ns:InfDeclaracaoPrestacaoServico", nsManager)
            ?? throw new Exception("Nó InfDeclaracaoPrestacaoServico não encontrado.");

        var signedXml = new SignedXml(xmlDoc) { SigningKey = certificado.GetRSAPrivateKey() };
        var reference = new Reference { Uri = "" };
        reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
        reference.AddTransform(new XmlDsigC14NTransform());
        signedXml.AddReference(reference);

        var keyInfo = new KeyInfo();
        keyInfo.AddClause(new KeyInfoX509Data(certificado));
        signedXml.KeyInfo = keyInfo;

        signedXml.ComputeSignature();
        nodeToSign.AppendChild(xmlDoc.ImportNode(signedXml.GetXml(), true));

        return xmlDoc.OuterXml;
    }
}
