// using MGMBlazor.Domain.Entities;
// using MGMBlazor.Infrastructure.NFSe.Certificates;
// using MGMBlazor.Infrastructure.NFSe.Abrasf;
// using MGMBlazor.Infrastructure.NFSe.Abrasf.Parsing;
// using MGMBlazor.Infrastructure.NFSe.Soap;
// using MGMBlazor.Services.Nfse;
// using MGMBlazor.Services.Sicoob;
// using MGMBlazor.Infrastructure.NFSe.Configuration;
// using System.Net;
// using MGMBlazor.Infrastructure.Data;
// using Microsoft.EntityFrameworkCore;
// using System.Runtime.InteropServices;
// using System.Security.Cryptography.X509Certificates;
// using MGMBlazor.Models.Sicoob;
// using MGMBlazor.Services.Import;

// var builder = WebApplication.CreateBuilder(args);



// // ---------- CONFIGURAÇÃO DE CERTIFICADO DINÂMICO ----------
// // Removemos a duplicação. O .NET decide aqui qual provider usar.
// if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
// {
//     builder.Services.AddSingleton<ICertificateProvider, WindowsCertificateProvider>();
// }
// else
// {
//     builder.Services.AddSingleton<ICertificateProvider, LinuxCertificateProvider>();
// }

// // Função para configurar o HttpClient com o Certificado Digital MGM
// HttpClientHandler CriarHandlerComCertificado(IServiceProvider sp)
// {
//     var handler = new HttpClientHandler();
//     var certProvider = sp.GetRequiredService<ICertificateProvider>();
//     var cert = certProvider.ObterCertificado();

//     if (cert != null)
//     {
//         handler.ClientCertificates.Add(cert);
//     }
//     return handler;
// }

// // ---------- BANCO DE DADOS E CONFIGS ----------
// var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// builder.Services.AddDbContext<AppDbContext>(options =>
//     options.UseNpgsql(connectionString));

// builder.Services.Configure<NfseOptions>(
//     builder.Configuration.GetSection("Nfse"));

// // ---------- INFRAESTRUTURA NFSe ----------
// builder.Services.AddScoped<AbrasfXmlBuilder>();
// builder.Services.AddScoped<XmlValidator>();
// builder.Services.AddScoped<XmlSigner>();
// builder.Services.AddScoped<INfseRetornoParser, AbrasfRetornoParser>();
// builder.Services.AddScoped<FintelSoapClient>();
// builder.Services.AddScoped<FaturaImportService>();

// // ---------- REGISTRO DOS SERVIÇOS COM HTTPCLIENT "BATIZADO" ----------

// // Registra NfseService injetando o HttpClient já com certificado
// builder.Services.AddHttpClient<INfseService, NfseService>()
//     .ConfigurePrimaryHttpMessageHandler(sp => CriarHandlerComCertificado(sp));

// // Prepara o SicoobService (ele usará o mesmo certificado automaticamente, se não for Sandbox)
// builder.Services.AddHttpClient<ISicoobService, SicoobService>()
//     .ConfigurePrimaryHttpMessageHandler(sp => CriarHandlerComCertificado(sp));

// var app = builder.Build();

// app.Logger.LogInformation(
//     "Ambiente atual: {env}",
//     app.Environment.EnvironmentName);

// if (app.Environment.IsDevelopment())
// {
//     app.UseDeveloperExceptionPage();
// }

// // ---------- TESTE DE HOMOLOGAÇÃO (MANTIDO) ----------
// using (var scope = app.Services.CreateScope())
// {
//     var nfseService = scope.ServiceProvider.GetRequiredService<INfseService>();
//     var sicoobService = scope.ServiceProvider.GetRequiredService<ISicoobService>();

//     var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
//     bool isSandbox = config.GetValue<bool>("SicoobConfig:UsarSandbox");

//     try
//     {
//         var proximoRPS = await nfseService.ObterProximoNumeroRpsAsync();
//         //int proximoRPS = 1; // Número fixo para evitar conflitos em múltiplos testes
//         Console.WriteLine($"Gerando nota com RPS: {proximoRPS}");

//         // --- TESTE COMPLETO DE EMISSÃO, SUBSTITUIÇÃO E CANCELAMENTO ---

//         var consulta = await nfseService.VerificarSeRpsJaExisteNaPrefeitura(proximoRPS);

//         if (consulta.Sucesso)
//         {
//             // Se entrou aqui, significa que o RPS já virou nota na prefeitura
//             // O método VerificarSeRps... já salvou no banco.
//             Console.ForegroundColor = ConsoleColor.Cyan;
//             Console.WriteLine($"[AVISO] O RPS {proximoRPS} já havia sido emitido (Nota {consulta.NumeroNota}).");
//             Console.WriteLine("O sistema sincronizou o banco de dados. Tente rodar novamente para o próximo número.");
//             Console.ResetColor();
//         }
//         else
//         {
//             Console.WriteLine($"[DEBUG] Entrou na emissão de Nota - RPS {proximoRPS}.");
//             var notaTeste = new NotaFiscal
//             {
//                 Id = proximoRPS,
//                 Valor = 100.00m,
//                 Tomador = new Cliente
//                 {
//                     RazaoSocial = "Cliente Teste LTDA",
//                     Cnpj = "12345678000199",
//                     Email = "teste@cliente.com",
//                     MunicipioCodigoIbge = "4115200"
//                 },
//                 Servico = new Servico
//                 {
//                     CodigoMunicipal = "0701",
//                     Descricao = "Exame ocupacional admissional",
//                 }
//             };

//             var respostaNfse = await nfseService.EmitirNotaAsync(notaTeste);

//             Console.WriteLine($"Sucesso NFSe: {respostaNfse.Sucesso}");
//             Console.WriteLine($"Número NFSe: {respostaNfse.NumeroNota}");
//             if (respostaNfse.Erros.Any())
//                 Console.WriteLine("Erros: " + string.Join(Environment.NewLine, respostaNfse.Erros));

//             if (respostaNfse.Sucesso)
//             {
//                 string numeroNotaOriginal = respostaNfse.NumeroNota!;
//                 Console.WriteLine($"✅ Nota {numeroNotaOriginal} emitida!");

//                 // PASSO 3: SUBSTITUIR (Trocar a nota original por uma nova)
//                 int rpsNovo = proximoRPS + 1;
//                 Console.WriteLine($"\n🔄 Substituindo Nota {numeroNotaOriginal} pelo novo RPS {rpsNovo}...");
//                 var notaNova = new NotaFiscal
//                 {
//                     Id = rpsNovo,
//                     Valor = 150.00m,
//                     Tomador = new Cliente
//                     {
//                         RazaoSocial = "SubsCliente NovaNota LTDA",
//                         Cnpj = "66645678000111",
//                         Email = "novo@nota.com",
//                         MunicipioCodigoIbge = "5005277"
//                     },
//                     Servico = new Servico
//                     {
//                         CodigoMunicipal = "0701",
//                         Descricao = "Exame ocupacional admissional",
//                     }
//                 };

//                 var respSubst = await nfseService.SubstituirNotaAsync(numeroNotaOriginal, notaNova);

//                 Console.WriteLine($"Sucesso NFSe: {respSubst.Sucesso}");
//                 Console.WriteLine($"Número NFSe: {respSubst.NumeroNota}");
//                 if (respSubst.Erros.Any())
//                     Console.WriteLine("Erros: " + string.Join(Environment.NewLine, respSubst.Erros));

//                 if (respSubst.Sucesso)
//                 {
//                     string numeroNotaNova = respSubst.NumeroNota!;
//                     Console.WriteLine($"✅ Substituição OK! Nova Nota: {numeroNotaNova}");

//                     //PASSO 4: CANCELAR (Matar a nota nova para limpar o rastro)
//                     Console.WriteLine($"\n🗑️ Cancelando a última nota ({numeroNotaNova})...");
//                     var respCancel = await nfseService.CancelarNotaAsync(numeroNotaNova, "1");

//                     if (respCancel.Sucesso)
//                         Console.WriteLine("✅ Ciclo completo! Nota cancelada com sucesso.");
//                 }
//             }

//             /*
//             if (respostaNfse.Sucesso)
//             {
//                 Console.WriteLine("\n--- VERIFICAÇÃO DE SEGURANÇA SICOOB ---");

//                 // Verificamos o que o sistema está lendo do appsettings
//                 string urlUsada = isSandbox ? config["SicoobConfig:UrlSandbox"]! : config["SicoobConfig:Api-cobranca-bancaria-Url"]!;

//                 if (isSandbox)
//                 {
//                     Console.ForegroundColor = ConsoleColor.Yellow;
//                     Console.WriteLine("AMBIENTE: [SANDBOX] - NENHUM BOLETO REAL SERÁ GERADO.");
//                 }
//                 else
//                 {
//                     Console.ForegroundColor = ConsoleColor.Red;
//                     Console.WriteLine("ATENÇÃO: AMBIENTE [PRODUÇÃO]! BOLETO REAL COM CUSTO BANCÁRIO.");
//                 }
//                 Console.ResetColor();
//                 Console.WriteLine($"URL Alvo: {urlUsada}");
//                 Console.WriteLine("---------------------------------------\n");

//                 // Se estiver escrito SANDBOX acima, pode descomentar a linha abaixo sem medo:

//                 long numeroClienteSandbox = 25546454;

//                 var boletoReq = new BoletoRequest
//                 {
//                     NumeroCliente = isSandbox ? numeroClienteSandbox : config.GetValue<long>("SicoobConfig:NumeroCliente"),
//                     NumeroContaCorrente = isSandbox ? 0 : config.GetValue<long>("SicoobConfig:NumeroContaCorrente"),
//                     Valor = notaTeste.Servico.Valor,
//                     SeuNumero = respostaNfse.NumeroNota, //o usuario coloca o que quiser e pode contar letras
//                     //DataVencimento = DateTime.Now.AddDays(7).ToString("yyyy-MM-ddT00:00:00-03:00"),
//                     DataVencimento = "2018-09-20", // Data fixa para evitar problemas de datas em testes futuros
//                     Pagador = new PagadorRequest
//                     {
//                         Nome = notaTeste.Tomador.RazaoSocial,
//                         NumeroCpfCnpj = notaTeste.Tomador.Cnpj,
//                         Endereco = "Rua Teste, 123",
//                         Bairro = "Centro",
//                         Cidade = "Maringa",
//                         Cep = "87000000",
//                         Uf = "PR"
//                     }
//                 };

//                 // PODE DESCOMENTAR AQUI SE O LOG ACIMA DISSER SANDBOX:
//                 var respBoleto = await sicoobService.IncluirBoletoAsync(respostaNfse.IdInternoNoBanco, boletoReq);

//                 if (respBoleto?.Resultado != null)
//                 {
//                     Console.WriteLine($"BOLETO SANDBOX GERADO! Nosso Número: {respBoleto.Resultado.NossoNumero}");
//                     Console.WriteLine($"   Linha Digitável: {respBoleto.Resultado.LinhaDigitavel}");
//                     Console.WriteLine($"   ID Vinculado no Banco: {respostaNfse.IdInternoNoBanco}");

//                     long nossoNumeroGerado = respBoleto.Resultado.NossoNumero;

//                     // // if (!string.IsNullOrEmpty(respBoleto.Resultado.PdfBoleto))
//                     // // {
//                     // //     string base64Original = respBoleto.Resultado.PdfBoleto;

//                     // //     // --- DEBUG: RAIO-X DO BASE64 ---
//                     // //     Console.WriteLine("\n[DEBUG-PDF] Verificando integridade do Base64...");
//                     // //     Console.WriteLine($"Tamanho Total: {base64Original.Length} caracteres");
//                     // //     Console.WriteLine($"Início (50 chrs): {base64Original.Substring(0, Math.Min(50, base64Original.Length))}");
//                     // //     Console.WriteLine($"Fim (10 chrs): {base64Original.Substring(Math.Max(0, base64Original.Length - 10))}");
//                     // //     // -------------------------------

//                     // //     // LIMPEZA RADICAL: Remove tudo que não for caractere válido de Base64
//                     // //     // (Letras, Números, +, /, =)
//                     // //     string base64Limpo = new string(base64Original
//                     // //         .Where(c => char.IsLetterOrDigit(c) || c == '/' || c == '+' || c == '=')
//                     // //         .ToArray());

//                     // //     // AJUSTE DE PADDING (O "enchimento" do Base64)
//                     // //     // O tamanho da string Base64 deve ser múltiplo de 4. Se não for, o C# dá erro.
//                     // //     int mod4 = base64Limpo.Length % 4;
//                     // //     if (mod4 > 0)
//                     // //     {
//                     // //         base64Limpo += new string('=', 4 - mod4);
//                     // //         Console.WriteLine($"[DEBUG-PDF] Ajustando padding: Adicionados {4 - mod4} caracteres '='");
//                     // //     }

//                     // //     try
//                     // //     {
//                     // //         byte[] pdfBytes = Convert.FromBase64String(base64Limpo);
//                     // //         string nomeArquivo = $"Boleto_Teste_{respBoleto.Resultado.NossoNumero}.pdf";
//                     // //         await File.WriteAllBytesAsync(nomeArquivo, pdfBytes);

//                     // //         Console.WriteLine($"✅ [PDF] Sucesso! Salvo em: {nomeArquivo}");
//                     // //     }
//                     // //     catch (FormatException ex)
//                     // //     {
//                     // //         Console.WriteLine($"❌ Falha crítica no Base64 após limpeza: {ex.Message}");
//                     // //         // Se falhar, vamos imprimir a string limpa para você copiar e testar num site externo
//                     // //         // Console.WriteLine($"String Limpa: {base64Limpo}"); 
//                     // //     }
//                     // // }

//                     // // --- TESTE DE CONSULTA ---
//                     // Console.WriteLine($"⏳ Testando CONSULTA do boleto: {nossoNumeroGerado}...");
//                     // var boletoConsultado = await sicoobService.ConsultarBoletoAsync(nossoNumeroGerado);
//                     // if (boletoConsultado != null)
//                     // {
//                     //     Console.WriteLine($"✅ Consulta OK! Situação no Banco: {boletoConsultado.Resultado?.SituacaoBoleto}");
//                     // }

//                     // // --- TESTE DE BAIXA (CANCELAMENTO) ---
//                     // // CUIDADO: No sandbox ele pode dar 204 (sucesso), mas o GET continuar dizendo "Aberto" pois o mock não muda.
//                     // Console.WriteLine($"⏳ Testando BAIXA do boleto: {nossoNumeroGerado}...");
//                     // bool baixou = await sicoobService.BaixarBoletoAsync(nossoNumeroGerado);
//                     // if (baixou)
//                     // {
//                     //     Console.WriteLine("✅ Comando de BAIXA enviado com sucesso!");
//                     // }
//                 }
//             }
//             else
//             {
//                 Console.WriteLine("Falha na NFSe. O fluxo do boleto foi interrompido para sua segurança.");
//                 if (respostaNfse.Erros.Any())
//                     Console.WriteLine("Erros: " + string.Join(" | ", respostaNfse.Erros));
//             }
//             */
//         }

//     }
//     catch (Exception ex)
//     {
//         Console.WriteLine($"Erro no teste de inicialização: {ex.Message}");
//     }
// }

// app.Run();

using MGMBlazor.Infrastructure.Data;
using MGMBlazor.Infrastructure.NFSe.Abrasf;
using MGMBlazor.Infrastructure.NFSe.Abrasf.Parsing;
using MGMBlazor.Infrastructure.NFSe.Certificates;
using MGMBlazor.Infrastructure.NFSe.Configuration;
using MGMBlazor.Infrastructure.NFSe.Soap;
using MGMBlazor.Services.Import;
using MGMBlazor.Services.Nfse;
using MGMBlazor.Services.Sicoob;
using MGMBlazor.web.Components.Account;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using MGMBlazor.Domain.Entities;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Components.Authorization;
using MGMBlazor.Infrastructure.Security;
using MGMBlazor.Services.Shared;
using MGMBlazor.Services.Clientes;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// --- 1. CONFIGURAÇÕES BASE ---
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContextFactory<AppDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// --- 2. CERTIFICADOS (SINGLETON) ---
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    builder.Services.AddSingleton<ICertificateProvider, WindowsCertificateProvider>();
else
    builder.Services.AddSingleton<ICertificateProvider, LinuxCertificateProvider>();

// Função auxiliar para os Handlers
HttpClientHandler CriarHandler(IServiceProvider sp)
{
    var h = new HttpClientHandler();
    var p = sp.GetRequiredService<ICertificateProvider>();
    var c = p.ObterCertificado();
    if (c != null) h.ClientCertificates.Add(c);
    return h;
}

// --- 3. INFRA E SERVIÇOS ---
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // IMPORTANTE: Como o IP da VM é fixo ou interno, limpamos os proxies conhecidos
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});
builder.Services.Configure<NfseOptions>(
    builder.Configuration.GetSection("Nfse"));
builder.Services.AddScoped<AbrasfXmlBuilder>();
builder.Services.AddScoped<XmlValidator>();
builder.Services.AddScoped<XmlSigner>();
builder.Services.AddScoped<INfseRetornoParser, AbrasfRetornoParser>();
builder.Services.AddScoped<FintelSoapClient>();
builder.Services.AddScoped<FaturaImportService>(); // Novo serviço de CSV
builder.Services.AddAuthenticationCore();
builder.Services.AddScoped<ProtectedLocalStorage>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<AuditLogService>();

// Adiciona os serviços de autenticação e autorização padrão do .NET
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

// 2. Habilita Autenticação por Cookies (Padrão para Web Apps)
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
    })
    .AddIdentityCookies();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // mudar para true se quiser exigir confirmação por e-mail
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddRoles<IdentityRole>() // Habilita o sistema de Admin/Fiscal/Funcionario
.AddEntityFrameworkStores<AppDbContext>()
.AddSignInManager()
.AddDefaultTokenProviders()
.AddClaimsPrincipalFactory<MGMClaimsFactory>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromMinutes(15); // Tempo de inatividade: 15 minutos
    options.SlidingExpiration = true;
    options.LoginPath = "/Account/Login";
});

builder.Services.AddScoped<IEmailSender<ApplicationUser>, EmailService>();

builder.Services.AddHttpClient<INfseService, NfseService>().ConfigurePrimaryHttpMessageHandler(sp => CriarHandler(sp));
builder.Services.AddHttpClient<ISicoobService, SicoobService>().ConfigurePrimaryHttpMessageHandler(sp => CriarHandler(sp));
builder.Services.AddHostedService<SicoobWorkerService>();

builder.Services.AddHttpClient<CepService>();

// Configura a cultura padrão para Português do Brasil
var cultureInfo = new System.Globalization.CultureInfo("pt-BR");
System.Globalization.CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var app = builder.Build();

app.UseForwardedHeaders();

// --- 4. PIPELINE HTTP ---
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// --- ATIVAÇÃO DO SEEDER ---
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    if (!await userManager.Users.AnyAsync())
    {
        await DbSeeder.SeedAsync(userManager, roleManager);
    }
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();
// app.UseAuthentication();
// app.UseAuthorization();

//app.MapIdentityApi<ApplicationUser>();
app.MapRazorComponents<MGMBlazor.web.Components.App>() // Verifique se o namespace MGMBlazor.Web.Components está correto
    .AddInteractiveServerRenderMode();

app.MapAdditionalIdentityEndpoints(); // Mapeia endpoints adicionais para gerenciamento de usuários e roles

// --- TESTE DE EMAIL (Mailtrap) ---
// Você pode comentar este bloco após o primeiro sucesso
// using (var scope = app.Services.CreateScope())
// {
//     var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();
//     Console.WriteLine("[TESTE] Tentando enviar e-mail de teste para o Mailtrap...");

//     bool enviou = await emailService.EnviarFaturamentoPorEmail(
//         "teste@cliente.com",
//         "https://nfse.maringa.pr.gov.br/exemplo",
//         "", // Enviando vazio para testar apenas o corpo e o logo
//         "9999"
//     );

//     if (enviou) Console.WriteLine("[TESTE] E-mail de teste enviado! Verifique o painel do Mailtrap.");
//     else Console.WriteLine("[TESTE] Falha no e-mail. Verifique o console para erros de SMTP.");
// }
// ---------------------------------

app.Run();