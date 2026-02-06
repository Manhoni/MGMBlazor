using MGMBlazor.Domain.Entities;
using MGMBlazor.Infrastructure.NFSe.Abrasf;
using System.Xml.Linq;
using MGMBlazor.Infrastructure.NFSe.Certificates;
using MGMBlazor.Infrastructure.NFSe.Soap;
using MGMBlazor.Infrastructure.NFSe.Abrasf.Parsing;
using MGMBlazor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using MGMBlazor.Infrastructure.NFSe.Configuration;
using Microsoft.Extensions.Options;

namespace MGMBlazor.Services.Nfse;

public class NfseService : INfseService
{
    private readonly AppDbContext _context;
    private readonly AbrasfXmlBuilder _builder;
    private readonly XmlValidator _validator;
    private readonly XmlSigner _signer;
    private readonly ICertificateProvider _certificateProvider;
    private readonly HttpClient _httpCliente;
    private readonly FintelSoapClient _soapClient; // Injetado ou criado aqui
    private readonly string _pastaSchemas;
    private readonly INfseRetornoParser _retornoParser;
    private readonly NfseOptions _config;

    public NfseService(
        AppDbContext context,
        AbrasfXmlBuilder builder,
        XmlValidator validator,
        XmlSigner signer,
        ICertificateProvider certificateProvider,
        HttpClient httpClient,
        FintelSoapClient soapClient,
        INfseRetornoParser retornoParser,
        IOptions<NfseOptions> config)
    {
        _context = context;
        _builder = builder;
        _validator = validator;
        _signer = signer;
        _certificateProvider = certificateProvider;
        _httpCliente = httpClient;
        _soapClient = soapClient;
        _retornoParser = retornoParser;
        _config = config.Value;

        // Define a pasta uma única vez no construtor
        _pastaSchemas = Path.Combine(AppContext.BaseDirectory, "Infrastructure", "NFSe", "Abrasf", "Schemas");
    }

    public async Task<int> ObterProximoNumeroRpsAsync()
    {
        // Busca o maior VendaId já salvo no banco. Se não houver nenhum, começa do 1 (ou do número que você definir)
        var ultimoRPS = await _context.NotasFiscaisEmitidas
            .MaxAsync(n => (int?)n.RpsNumero) ?? 0; // Se quiser começar de um número específico, mude o 0 para 100, por exemplo.

        return ultimoRPS + 1;
    }

    public string GerarLinkConsultaPublica(string numeroNota, string codigoVerificacao)
    {
        var cnpjPrestador = _config.Prestador.Cnpj;
        // URL padrão de consulta de Maringá (ajuste se o manual indicar outra)
        //https://maringa.fintel.com.br/ImprimirNfse/1931/02152507000196/EWCJVYB1U   // numeros teste
        return $"https://maringa.fintel.com.br/ImprimirNfse/{numeroNota}/{cnpjPrestador}/{codigoVerificacao}";
    }

    public async Task<List<NotaFiscalEmitida>> ListarNotasAsync(DateTime inicio, DateTime fim)
    {
        return await _context.NotasFiscaisEmitidas
            .Where(n => n.DataEmissao >= inicio && n.DataEmissao <= fim)
            .OrderByDescending(n => n.DataEmissao)
            .ToListAsync();
    }

    public async Task<RespostaEmissao> VerificarSeRpsJaExisteNaPrefeitura(int rpsNumero)
    {
        Console.WriteLine($"[CONSULTA] Verificando RPS {rpsNumero}...");

        var resposta = new RespostaEmissao(); // Cria o objeto que será preenchido

        try
        {
            // 1. Monta o XDocument (Padrão que você definiu)
            var xmlDoc = _builder.MontarXmlConsultaRps(rpsNumero);
            // 2. Validação XSD (Usando a variável do construtor)
            _validator.Validar(xmlDoc, _pastaSchemas);
            // 3. Certificado
            var certificado = _certificateProvider.ObterCertificado();
            // 4. Assina 
            //var xmlAssinado = _signer.AssinarElemento(xmlDoc, "ConsultarNfseRpsEnvio", certificado);

            //Vamos tentar sem assinar, ja convertendo para string. Prefeitura de Mga não exige assinatura na consulta
            string xmlAssinado = xmlDoc.ToString();
            // 5. Envia via SOAP
            var xmlRespostaSoap = await _soapClient.ConsultarNfsePorRpsAsync(xmlAssinado, certificado);

            resposta.XmlRetorno = xmlRespostaSoap;

            // 6. Processa preenchendo o objeto 'resposta' (Seu padrão void Processar)
            _retornoParser.Processar(xmlRespostaSoap, resposta);

            if (resposta.Erros.Any()) Console.WriteLine("[DEBUG-CONSULTA] Erros no Parser: " + string.Join(", ", resposta.Erros));

            if (resposta.Sucesso)
            {
                Console.WriteLine($"[CONSULTA] RPS {rpsNumero} já é a Nota {resposta.NumeroNota}. Sincronizando banco...");

                var notaDb = await _context.NotasFiscaisEmitidas.FirstOrDefaultAsync(n => n.RpsNumero == rpsNumero);

                if (notaDb == null)
                {
                    // Se a nota não existia no banco, salva com o status que veio da prefeitura
                    await SalvarNoBanco(new NotaFiscal { Id = rpsNumero }, resposta, resposta.StatusRecuperado);
                }
                else
                {
                    // Se já existia, sincroniza o status (Ex: estava faturada mas foi cancelada no site)
                    notaDb.Status = resposta.StatusRecuperado;
                    //notaDb.NumeroNota = resposta.NumeroNota;
                    notaDb.XmlRetorno = resposta.XmlRetorno;
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"[BANCO] Status da nota {resposta.NumeroNota} sincronizado para: {notaDb.Status}");
                }
            }
        }
        catch (Exception ex)
        {
            resposta.Sucesso = false;
            resposta.Erros.Add($"Erro na consulta: {ex.Message}");
        }

        return resposta;
    }

    // private async Task SalvarNoBancoAposRecuperacao(int rpsNumero, RespostaEmissao resposta)
    // {
    //     var notaEmitida = new NotaFiscalEmitida
    //     {
    //         RpsNumero = rpsNumero,
    //         VendaId = 0, // Id de venda desconhecido na recuperação
    //         DataEmissao = DateTime.UtcNow,
    //         NumeroNota = resposta.NumeroNota,
    //         CodigoVerificacao = resposta.CodigoVerificacao,
    //         XmlRetorno = resposta.XmlRetorno,
    //         Status = StatusNfse.Faturada
    //     };

    //     _context.NotasFiscaisEmitidas.Add(notaEmitida);
    //     await _context.SaveChangesAsync();

    //     // Atualiza o ID interno na resposta para o fluxo do boleto seguir
    //     resposta.IdInternoNoBanco = notaEmitida.Id;
    // }

    public async Task<RespostaEmissao> EmitirNotaAsync(NotaFiscal nota)
    {
        var resposta = new RespostaEmissao();

        try
        {
            // 1. Geração do XML
            var xmlDoc = _builder.GerarXml(nota);
            // 2. Validação XSD (Usando a variável do construtor)
            _validator.Validar(xmlDoc, _pastaSchemas);
            // 3. Certificado
            var certificado = _certificateProvider.ObterCertificado();
            // 4. Assinatura (Retorna String)
            string xmlAssinado = _signer.AssinarElemento(xmlDoc, "InfDeclaracaoPrestacaoServico", certificado);
            resposta.XmlEnviado = xmlAssinado;

            // GRAVA O ARQUIVO PARA COPIAR PARA O POSTMAN
            File.WriteAllText("nota_assinada.xml", xmlAssinado);

            // Limpa a declaração XML interna que Maringá não gosta
            string xmlSemDeclaracao = xmlAssinado.Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>", "");

            // Escapa o XML programaticamente (transforma < em &lt;)
            string xmlParaPostman = System.Security.SecurityElement.Escape(xmlSemDeclaracao);

            File.WriteAllText("CONTEUDO_PARA_POSTMAN.txt", xmlParaPostman);

            string xmlRetorno = await _soapClient.EnviarGerarNfseAsync(xmlAssinado, certificado);

            _retornoParser.Processar(xmlRetorno, resposta);

            if (resposta.Sucesso)
            {
                await SalvarNoBanco(nota, resposta);
            }
        }
        catch (Exception ex)
        {
            resposta.Sucesso = false;
            resposta.Erros.Add($"Falha na emissão: {ex.Message}");
        }

        return resposta;
    }

    private async Task SalvarNoBanco(NotaFiscal nota, RespostaEmissao resposta, StatusNfse status = StatusNfse.Faturada)
    {
        var statusFinal = resposta.StatusRecuperado != StatusNfse.Pendente
                      ? resposta.StatusRecuperado
                      : status;

        var notaEmitida = new NotaFiscalEmitida
        {
            RpsNumero = nota.Id, // Grava o número do RPS que acabamos de usar
            VendaId = 100, // tanto faz a empresa que escolhe (preciso acertar a logica para que ela seja inserida pelo usuario e volte pelo Response)
            Valor = nota.Valor,
            CnpjTomador = nota.Tomador.Cnpj,
            DataEmissao = DateTime.UtcNow,
            NumeroNota = resposta.NumeroNota,
            CodigoVerificacao = resposta.CodigoVerificacao,
            XmlRetorno = resposta.XmlRetorno,
            Status = statusFinal
        };

        _context.NotasFiscaisEmitidas.Add(notaEmitida);
        await _context.SaveChangesAsync();

        resposta.IdInternoNoBanco = notaEmitida.Id;
        Console.WriteLine($"[BANCO] Nota {resposta.NumeroNota} salva com sucesso!");
    }

    public async Task<RespostaEmissao> CancelarNotaAsync(string numeroNota, string codigoMotivo = "1")
    {
        var resposta = new RespostaEmissao();
        try
        {
            var xmlDoc = _builder.MontarXmlCancelamento(numeroNota, codigoMotivo);
            // Validação XSD
            _validator.Validar(xmlDoc, _pastaSchemas);
            // Certificado
            var cert = _certificateProvider.ObterCertificado();
            // Tag específica para cancelamento
            var xmlAssinado = _signer.AssinarElemento(xmlDoc, "InfPedidoCancelamento", cert);

            var respSoap = await _soapClient.CancelarNfseAsync(xmlAssinado, cert);
            _retornoParser.Processar(respSoap, resposta);

            if (resposta.Sucesso)
            {
                Console.WriteLine($"[BANCO] Nota {numeroNota} cancelada");
                //Atualize o status no banco para Cancelada
                var notaCancelada = await _context.NotasFiscaisEmitidas.FirstOrDefaultAsync(n => n.NumeroNota == numeroNota);
                if (notaCancelada != null)
                {
                    // Chamamos a consulta por RPS.
                    // Esse método já vai baixar o XML COMPLETO e atualizar o Status no banco local.
                    Console.WriteLine($"[BANCO] Nota {numeroNota} cancelada e sendo sincronizada com o banco...");
                    await VerificarSeRpsJaExisteNaPrefeitura(notaCancelada.RpsNumero);
                }
            }
        }
        catch (Exception ex)
        {
            resposta.Sucesso = false;
            resposta.Erros.Add($"Falha no cancelamento: {ex.Message}");
            return resposta;
        }

        return resposta;
    }
    public async Task<RespostaEmissao> SubstituirNotaAsync(string numeroAntiga, NotaFiscal novaNota)
    {
        var resposta = new RespostaEmissao();
        try
        {
            var xmlDoc = _builder.MontarXmlSubstituicao(numeroAntiga, novaNota);
            _validator.Validar(xmlDoc, _pastaSchemas);
            var cert = _certificateProvider.ObterCertificado();
            string xmlAssinado = _signer.AssinarElemento(xmlDoc, "SubstituicaoNfse", cert);

            string xmlRetorno = await _soapClient.SubstituirNfseAsync(xmlAssinado, cert);
            _retornoParser.Processar(xmlRetorno, resposta);

            if (resposta.Sucesso)
            {
                // 1. "MATA" A NOTA ANTIGA NO SEU BANCO
                var notaAntigaDb = await _context.NotasFiscaisEmitidas.FirstOrDefaultAsync(n => n.NumeroNota == numeroAntiga);
                if (notaAntigaDb != null)
                {
                    notaAntigaDb.Status = StatusNfse.Cancelada;
                    _context.NotasFiscaisEmitidas.Update(notaAntigaDb);
                    Console.WriteLine($"[BANCO] Nota antiga {numeroAntiga} marcada como Cancelada.");
                    await _context.SaveChangesAsync();
                }

                // 2. "NASCE" A NOTA NOVA (A 1921 que o parser agora vai pegar certo)
                await SalvarNoBanco(novaNota, resposta);
            }
        }
        catch (Exception ex)
        {
            resposta.Sucesso = false;
            resposta.Erros.Add($"Falha na substituição: {ex.Message}");
        }
        return resposta;
    }
}
