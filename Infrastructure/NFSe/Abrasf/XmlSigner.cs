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
        nsManager.AddNamespace("ns", "http://www.abrasf.org.br/nfse.xsd");

        // 1. O QUE ASSINAR: A tag InfDeclaracaoPrestacaoServico
        var nodeToSign = xmlDoc.SelectSingleNode("//ns:InfDeclaracaoPrestacaoServico", nsManager)
            ?? throw new Exception("Nó InfDeclaracaoPrestacaoServico não encontrado.");

        // 2. ONDE PENDURAR A ASSINATURA: Na tag <Rps> (que é a mãe da Inf...)
        var parentNode = nodeToSign.ParentNode 
            ?? throw new Exception("Nó pai (Rps) não encontrado.");

        var signedXml = new SignedXml(xmlDoc) { SigningKey = certificado.GetRSAPrivateKey() };
        
        // Referência deve apontar para o ID que criamos no Builder (ex: #R1)
        string id = nodeToSign.Attributes?["Id"]?.Value ?? "1";
        var reference = new Reference { Uri = "#" + id };
        
        reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
        reference.AddTransform(new XmlDsigC14NTransform());
        signedXml.AddReference(reference);

        var keyInfo = new KeyInfo();
        keyInfo.AddClause(new KeyInfoX509Data(certificado));
        signedXml.KeyInfo = keyInfo;

        signedXml.ComputeSignature();

        // 3. ADICIONA A ASSINATURA NO LUGAR CERTO: Dentro de <Rps>, após a <Inf...>
        parentNode.AppendChild(xmlDoc.ImportNode(signedXml.GetXml(), true));

        return xmlDoc.OuterXml;
    }
}
