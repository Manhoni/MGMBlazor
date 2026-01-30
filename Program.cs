using MGMBlazor.Domain.Entities;
using MGMBlazor.Infrastructure.NFSe.Certificates;
using MGMBlazor.Infrastructure.NFSe.Abrasf;
using MGMBlazor.Infrastructure.NFSe.Abrasf.Parsing;
using MGMBlazor.Infrastructure.NFSe.Soap;
using MGMBlazor.Services.Nfse;
using MGMBlazor.Services.Sicoob;
using MGMBlazor.Infrastructure.NFSe.Configuration;
using System.Net;
using MGMBlazor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using MGMBlazor.Models.Sicoob;

var builder = WebApplication.CreateBuilder(args);



// ---------- CONFIGURAÇÃO DE CERTIFICADO DINÂMICO ----------
// Removemos a duplicação. O .NET decide aqui qual provider usar.
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    builder.Services.AddSingleton<ICertificateProvider, WindowsCertificateProvider>();
}
else
{
    builder.Services.AddSingleton<ICertificateProvider, LinuxCertificateProvider>();
}

// Função para configurar o HttpClient com o Certificado Digital MGM
HttpClientHandler CriarHandlerComCertificado(IServiceProvider sp)
{
    var handler = new HttpClientHandler();
    var certProvider = sp.GetRequiredService<ICertificateProvider>();
    var cert = certProvider.ObterCertificado();

    if (cert != null)
    {
        handler.ClientCertificates.Add(cert);
    }
    return handler;
}

// ---------- BANCO DE DADOS E CONFIGS ----------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.Configure<NfseOptions>(
    builder.Configuration.GetSection("Nfse"));

// ---------- INFRAESTRUTURA NFSe ----------
builder.Services.AddScoped<AbrasfXmlBuilder>();
builder.Services.AddScoped<XmlValidator>();
builder.Services.AddScoped<XmlSigner>();
builder.Services.AddScoped<INfseRetornoParser, AbrasfRetornoParser>();
builder.Services.AddScoped<FintelSoapClient>();

// ---------- REGISTRO DOS SERVIÇOS COM HTTPCLIENT "BATIZADO" ----------

// Registra NfseService injetando o HttpClient já com certificado
builder.Services.AddHttpClient<INfseService, NfseService>()
    .ConfigurePrimaryHttpMessageHandler(sp => CriarHandlerComCertificado(sp));

// Prepara o SicoobService (ele usará o mesmo certificado automaticamente, se não for Sandbox)
builder.Services.AddHttpClient<ISicoobService, SicoobService>()
    .ConfigurePrimaryHttpMessageHandler(sp => CriarHandlerComCertificado(sp));

var app = builder.Build();

app.Logger.LogInformation(
    "Ambiente atual: {env}",
    app.Environment.EnvironmentName);

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// ---------- TESTE DE HOMOLOGAÇÃO (MANTIDO) ----------
using (var scope = app.Services.CreateScope())
{
    var nfseService = scope.ServiceProvider.GetRequiredService<INfseService>();
    var sicoobService = scope.ServiceProvider.GetRequiredService<ISicoobService>();

    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    bool isSandbox = config.GetValue<bool>("SicoobConfig:UsarSandbox");

    try
    {
        var proximoRPS = await nfseService.ObterProximoNumeroRpsAsync();
        //int proximoRPS = 1; // Número fixo para evitar conflitos em múltiplos testes
        Console.WriteLine($"Gerando nota com RPS: {proximoRPS}");

        // --- TESTE COMPLETO DE EMISSÃO, SUBSTITUIÇÃO E CANCELAMENTO ---

        var consulta = await nfseService.VerificarSeRpsJaExisteNaPrefeitura(proximoRPS);

        if (consulta.Sucesso)
        {
            // Se entrou aqui, significa que o RPS já virou nota na prefeitura
            // O método VerificarSeRps... já salvou no banco.
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"[AVISO] O RPS {proximoRPS} já havia sido emitido (Nota {consulta.NumeroNota}).");
            Console.WriteLine("O sistema sincronizou o banco de dados. Tente rodar novamente para o próximo número.");
            Console.ResetColor();
        }
        else
        {
            Console.WriteLine($"[DEBUG] Entrou na emissão de Nota - RPS {proximoRPS}.");
            var notaTeste = new NotaFiscal
            {
                Id = proximoRPS,
                Tomador = new Cliente
                {
                    RazaoSocial = "Cliente Teste LTDA",
                    Cnpj = "12345678000199",
                    Email = "teste@cliente.com",
                    MunicipioCodigoIbge = "4115200"
                },
                Servico = new Servico
                {
                    CodigoMunicipal = "0701",
                    Descricao = "Exame ocupacional admissional",
                    Valor = 150.00m
                }
            };

            var respostaNfse = await nfseService.EmitirNotaAsync(notaTeste);

            Console.WriteLine($"Sucesso NFSe: {respostaNfse.Sucesso}");
            Console.WriteLine($"Número NFSe: {respostaNfse.NumeroNota}");
            if (respostaNfse.Erros.Any())
                Console.WriteLine("Erros: " + string.Join(Environment.NewLine, respostaNfse.Erros));

            if (respostaNfse.Sucesso)
            {
                string numeroNotaOriginal = respostaNfse.NumeroNota!;
                Console.WriteLine($"✅ Nota {numeroNotaOriginal} emitida!");

                // PASSO 3: SUBSTITUIR (Trocar a nota original por uma nova)
                int rpsNovo = proximoRPS + 1;
                Console.WriteLine($"\n🔄 Substituindo Nota {numeroNotaOriginal} pelo novo RPS {rpsNovo}...");
                var notaNova = new NotaFiscal
                {
                    Id = rpsNovo,
                    Tomador = new Cliente
                    {
                        RazaoSocial = "SubsCliente NovaNota LTDA",
                        Cnpj = "66645678000111",
                        Email = "novo@nota.com",
                        MunicipioCodigoIbge = "5005277"
                    },
                    Servico = new Servico
                    {
                        CodigoMunicipal = "0701",
                        Descricao = "Exame ocupacional admissional",
                        Valor = 60.00m
                    }
                };

                var respSubst = await nfseService.SubstituirNotaAsync(numeroNotaOriginal, notaNova);

                Console.WriteLine($"Sucesso NFSe: {respSubst.Sucesso}");
                Console.WriteLine($"Número NFSe: {respSubst.NumeroNota}");
                if (respSubst.Erros.Any())
                    Console.WriteLine("Erros: " + string.Join(Environment.NewLine, respSubst.Erros));

                if (respSubst.Sucesso)
                {
                    string numeroNotaNova = respSubst.NumeroNota!;
                    Console.WriteLine($"✅ Substituição OK! Nova Nota: {numeroNotaNova}");

                    //PASSO 4: CANCELAR (Matar a nota nova para limpar o rastro)
                    Console.WriteLine($"\n🗑️ Cancelando a última nota ({numeroNotaNova})...");
                    var respCancel = await nfseService.CancelarNotaAsync(numeroNotaNova, "1");

                    if (respCancel.Sucesso)
                        Console.WriteLine("✅ Ciclo completo! Nota cancelada com sucesso.");
                }
            }

            /*
            if (respostaNfse.Sucesso)
            {
                Console.WriteLine("\n--- VERIFICAÇÃO DE SEGURANÇA SICOOB ---");

                // Verificamos o que o sistema está lendo do appsettings
                string urlUsada = isSandbox ? config["SicoobConfig:UrlSandbox"]! : config["SicoobConfig:Api-cobranca-bancaria-Url"]!;

                if (isSandbox)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("AMBIENTE: [SANDBOX] - NENHUM BOLETO REAL SERÁ GERADO.");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("ATENÇÃO: AMBIENTE [PRODUÇÃO]! BOLETO REAL COM CUSTO BANCÁRIO.");
                }
                Console.ResetColor();
                Console.WriteLine($"URL Alvo: {urlUsada}");
                Console.WriteLine("---------------------------------------\n");

                // Se estiver escrito SANDBOX acima, pode descomentar a linha abaixo sem medo:

                long numeroClienteSandbox = 25546454;

                var boletoReq = new BoletoRequest
                {
                    NumeroCliente = isSandbox ? numeroClienteSandbox : config.GetValue<long>("SicoobConfig:NumeroCliente"),
                    NumeroContaCorrente = isSandbox ? 0 : config.GetValue<long>("SicoobConfig:NumeroContaCorrente"),
                    Valor = notaTeste.Servico.Valor,
                    SeuNumero = respostaNfse.NumeroNota, //o usuario coloca o que quiser e pode contar letras
                    //DataVencimento = DateTime.Now.AddDays(7).ToString("yyyy-MM-ddT00:00:00-03:00"),
                    DataVencimento = "2018-09-20", // Data fixa para evitar problemas de datas em testes futuros
                    Pagador = new PagadorRequest
                    {
                        Nome = notaTeste.Tomador.RazaoSocial,
                        NumeroCpfCnpj = notaTeste.Tomador.Cnpj,
                        Endereco = "Rua Teste, 123",
                        Bairro = "Centro",
                        Cidade = "Maringa",
                        Cep = "87000000",
                        Uf = "PR"
                    }
                };

                // PODE DESCOMENTAR AQUI SE O LOG ACIMA DISSER SANDBOX:
                var respBoleto = await sicoobService.IncluirBoletoAsync(respostaNfse.IdInternoNoBanco, boletoReq);

                if (respBoleto?.Resultado != null)
                {
                    Console.WriteLine($"BOLETO SANDBOX GERADO! Nosso Número: {respBoleto.Resultado.NossoNumero}");
                    Console.WriteLine($"   Linha Digitável: {respBoleto.Resultado.LinhaDigitavel}");
                    Console.WriteLine($"   ID Vinculado no Banco: {respostaNfse.IdInternoNoBanco}");

                    long nossoNumeroGerado = respBoleto.Resultado.NossoNumero;

                    // // if (!string.IsNullOrEmpty(respBoleto.Resultado.PdfBoleto))
                    // // {
                    // //     string base64Original = respBoleto.Resultado.PdfBoleto;

                    // //     // --- DEBUG: RAIO-X DO BASE64 ---
                    // //     Console.WriteLine("\n[DEBUG-PDF] Verificando integridade do Base64...");
                    // //     Console.WriteLine($"Tamanho Total: {base64Original.Length} caracteres");
                    // //     Console.WriteLine($"Início (50 chrs): {base64Original.Substring(0, Math.Min(50, base64Original.Length))}");
                    // //     Console.WriteLine($"Fim (10 chrs): {base64Original.Substring(Math.Max(0, base64Original.Length - 10))}");
                    // //     // -------------------------------

                    // //     // LIMPEZA RADICAL: Remove tudo que não for caractere válido de Base64
                    // //     // (Letras, Números, +, /, =)
                    // //     string base64Limpo = new string(base64Original
                    // //         .Where(c => char.IsLetterOrDigit(c) || c == '/' || c == '+' || c == '=')
                    // //         .ToArray());

                    // //     // AJUSTE DE PADDING (O "enchimento" do Base64)
                    // //     // O tamanho da string Base64 deve ser múltiplo de 4. Se não for, o C# dá erro.
                    // //     int mod4 = base64Limpo.Length % 4;
                    // //     if (mod4 > 0)
                    // //     {
                    // //         base64Limpo += new string('=', 4 - mod4);
                    // //         Console.WriteLine($"[DEBUG-PDF] Ajustando padding: Adicionados {4 - mod4} caracteres '='");
                    // //     }

                    // //     try
                    // //     {
                    // //         byte[] pdfBytes = Convert.FromBase64String(base64Limpo);
                    // //         string nomeArquivo = $"Boleto_Teste_{respBoleto.Resultado.NossoNumero}.pdf";
                    // //         await File.WriteAllBytesAsync(nomeArquivo, pdfBytes);

                    // //         Console.WriteLine($"✅ [PDF] Sucesso! Salvo em: {nomeArquivo}");
                    // //     }
                    // //     catch (FormatException ex)
                    // //     {
                    // //         Console.WriteLine($"❌ Falha crítica no Base64 após limpeza: {ex.Message}");
                    // //         // Se falhar, vamos imprimir a string limpa para você copiar e testar num site externo
                    // //         // Console.WriteLine($"String Limpa: {base64Limpo}"); 
                    // //     }
                    // // }

                    // // --- TESTE DE CONSULTA ---
                    // Console.WriteLine($"⏳ Testando CONSULTA do boleto: {nossoNumeroGerado}...");
                    // var boletoConsultado = await sicoobService.ConsultarBoletoAsync(nossoNumeroGerado);
                    // if (boletoConsultado != null)
                    // {
                    //     Console.WriteLine($"✅ Consulta OK! Situação no Banco: {boletoConsultado.Resultado?.SituacaoBoleto}");
                    // }

                    // // --- TESTE DE BAIXA (CANCELAMENTO) ---
                    // // CUIDADO: No sandbox ele pode dar 204 (sucesso), mas o GET continuar dizendo "Aberto" pois o mock não muda.
                    // Console.WriteLine($"⏳ Testando BAIXA do boleto: {nossoNumeroGerado}...");
                    // bool baixou = await sicoobService.BaixarBoletoAsync(nossoNumeroGerado);
                    // if (baixou)
                    // {
                    //     Console.WriteLine("✅ Comando de BAIXA enviado com sucesso!");
                    // }
                }
            }
            else
            {
                Console.WriteLine("Falha na NFSe. O fluxo do boleto foi interrompido para sua segurança.");
                if (respostaNfse.Erros.Any())
                    Console.WriteLine("Erros: " + string.Join(" | ", respostaNfse.Erros));
            }
            */
        }

    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro no teste de inicialização: {ex.Message}");
    }
}

app.Run();