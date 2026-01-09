using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace MGMBlazor.Infrastructure.NFSe.Abrasf;

public class XmlValidator
{
    public void Validar(XDocument xml, string pastaSchemas)
    {
        var schemas = new XmlSchemaSet();

        // Usando 'null' o .NET extrai o namespace automaticamente de dentro do arquivo
        schemas.Add(null, Path.Combine(pastaSchemas, "nfse_v2.01.xsd"));
        schemas.Add(null, Path.Combine(pastaSchemas, "xmldsig-core-schema20020212.xsd"));

        // É importante compilar os schemas antes de validar
        schemas.Compile();

        xml.Validate(schemas, (o, e) =>
        {
            throw new Exception($"Erro de validação XSD: {e.Message}");
        });
    }
}
