using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using System.Xml.Linq;

public class XmlSigner
{
    public string AssinarElemento(XDocument xml, string nomeTagParaAssinar, X509Certificate2 certificado)
    {
        var xmlDoc = new XmlDocument { PreserveWhitespace = true };
        using (var reader = xml.CreateReader()) { xmlDoc.Load(reader); }

        var nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
        nsManager.AddNamespace("ns", "http://www.abrasf.org.br/nfse.xsd");

        var nodeToSign = xmlDoc.SelectSingleNode($"//ns:{nomeTagParaAssinar}", nsManager)
            ?? xmlDoc.DocumentElement; // Se não achar a tag, assina a raiz

        if (nodeToSign == null) throw new Exception("Não foi possível encontrar um nó para assinar.");

        var signedXml = new SignedXml(xmlDoc) { SigningKey = certificado.GetRSAPrivateKey() };

        // Se a tag tiver um atributo "Id", usamos ele, senão assinamos o documento todo ""
        string? id = (nodeToSign as XmlElement)?.GetAttribute("Id");
        var reference = new Reference { Uri = string.IsNullOrEmpty(id) ? "" : "#" + id };

        reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
        reference.AddTransform(new XmlDsigC14NTransform());
        signedXml.AddReference(reference);

        var keyInfo = new KeyInfo();
        keyInfo.AddClause(new KeyInfoX509Data(certificado));
        signedXml.KeyInfo = keyInfo;

        signedXml.ComputeSignature();

        nodeToSign?.ParentNode?.AppendChild(xmlDoc.ImportNode(signedXml.GetXml(), true));

        return xmlDoc.OuterXml;
    }
}
