// using MailKit.Net.Smtp;
// using MimeKit;
// using MimeKit.Utils;
// using Microsoft.Extensions.Configuration;
// using Microsoft.AspNetCore.Identity.UI.Services;

// namespace MGMBlazor.Services.Shared;

// public class EmailService : IEmailSender
// {
//       private readonly IConfiguration _config;

//       public EmailService(IConfiguration config)
//       {
//             _config = config;
//       }

//       public async Task SendEmailAsync(string email, string subject, string htmlMessage)
//       {
//             await EnviarEmailBase(email, subject, htmlMessage, null, null);
//       }

//       public async Task EnviarFaturamentoPorEmail(string emailDestino, string linkNota, string base64Boleto, string numeroNota)
//       {
//             // --- LÓGICA DO LOGO ---
//             // Vamos anexar o logo para ele aparecer dentro do corpo do e-mail
//             var pathLogo = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "mgmLogo.jpg");
//             var image = bodyBuilder.LinkedResources.Add(pathLogo);
//             image.ContentId = MimeUtils.GenerateMessageId();

//             // Corpo do E-mail em HTML
//             var htmlBody = $@"
//             <div style='font-family: sans-serif; color: #333;'>
//                 <img src='cid:{image.ContentId}' style='width: 150px; margin-bottom: 20px;' />
//                 <h2 style='color: #76b82a;'>Olá!</h2>
//                 <p>Sua nota fiscal nº <b>{numeroNota}</b> foi emitida com sucesso.</p>
//                 <p>Você pode visualizar a nota oficial da prefeitura pelo link abaixo:</p>
//                 <p><a href='{linkNota}' style='background-color: #2e2e2e; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Ver Nota Fiscal</a></p>
//                 <hr />
//                 <p><b>O boleto bancário segue em anexo a este e-mail.</b></p>
//                 <br />
//                 <small>Este é um e-mail automático, por favor não responda.</small>
//             </div>";

//             await EnviarEmailBase(emailDestino, $"Nota Fiscal {numeroNota} - MGM", htmlBody, base64Boleto, numeroNota);
//       }

//       private async Task EnviarEmailBase(string destino, string assunto, string corpoHtml, string? base64Anexo, string? numDoc)
//       {
//             var message = new MimeMessage();
//             var remetente = _config["EmailConfig:Usuario"] ?? "financeiro@mgm.com.br";

//             message.From.Add(new MailboxAddress("MGM Faturamento", remetente));
//             message.To.Add(new MailboxAddress("Cliente", destino));
//             message.Subject = assunto;

//             var bodyBuilder = new BodyBuilder();



//             // --- ANEXO DO BOLETO (PDF) ---
//             if (!string.IsNullOrEmpty(base64Anexo))
//             {
//                   byte[] pdfBytes = Convert.FromBase64String(base64Anexo);
//                   bodyBuilder.Attachments.Add($"Boleto_MGM_{numDoc}.pdf", pdfBytes);
//             }

//             message.Body = bodyBuilder.ToMessageBody();

//             // --- ENVIO REAL ---
//             using var client = new SmtpClient();
//             try
//             {
//                   // Lê as configs do JSON
//                   var host = _config["EmailConfig:SmtpServer"];
//                   var porta = _config.GetValue<int>("EmailConfig:Porta");
//                   var user = _config["EmailConfig:Usuario"];
//                   var pass = _config["EmailConfig:Senha"];

//                   await client.ConnectAsync(host, porta, MailKit.Security.SecureSocketOptions.StartTls);
//                   await client.AuthenticateAsync(user, pass);
//                   await client.SendAsync(message);
//             }
//             finally
//             {
//                   await client.DisconnectAsync(true);
//             }
//       }
// }
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using System.Globalization;
using MGMBlazor.Domain.Entities;

namespace MGMBlazor.Services.Shared;

public class EmailService : IEmailSender<ApplicationUser>
{
      private readonly IConfiguration _config;

      public EmailService(IConfiguration config)
      {
            _config = config;
      }

      // --- 1. MÉTODO PARA O IDENTITY ---
      public async Task SendEmailAsync(string email, string subject, string htmlMessage)
      {
            var message = GerarMensagemBase(email, subject);
            var bodyBuilder = new BodyBuilder { HtmlBody = htmlMessage };
            message.Body = bodyBuilder.ToMessageBody();

            await EnviarSmtpAsync(message);
      }

      // --- MÉTODOS OBRIGATÓRIOS DO IDENTITY ---
      public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) =>
        EnviarEmailSimplesAsync(email, "Confirme sua conta - MGM", $"Clique aqui: {confirmationLink}");

      public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) =>
          EnviarEmailSimplesAsync(email, "Reset de Senha - MGM", $"Clique aqui: {resetLink}");

      public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) =>
          EnviarEmailSimplesAsync(email, "Código de Segurança - MGM", $"Seu código é: {resetCode}");

      // --- MÉTODOS PRIVADOS AUXILIARES ---
      private async Task EnviarEmailSimplesAsync(string email, string assunto, string html)
      {
            var message = new MimeMessage();
            message.To.Add(new MailboxAddress("", email));
            message.Subject = assunto;
            message.Body = new TextPart("html") { Text = html };
            await EnviarSmtpAsync(message);
      }

      // --- 2. MÉTODO PARA NOTA FISCAL E BOLETO ---
      public async Task<bool> EnviarFaturamentoPorEmail(string emailDestino, string linkNota, List<BoletoAnexo> boletos, string numeroNota)
      {
            var message = GerarMensagemBase(emailDestino, $"Nota Fiscal {numeroNota} - MGM Engenharia");
            var bodyBuilder = new BodyBuilder();

            // Lógica do Logo (Anexando a imagem ao corpo do e-mail)
            var pathLogo = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "mgmLogo.jpg");
            string cidLogo = "";

            if (File.Exists(pathLogo))
            {
                  var image = bodyBuilder.LinkedResources.Add(pathLogo);
                  image.ContentId = MimeUtils.GenerateMessageId();
                  cidLogo = image.ContentId;
            }

            string textoBoleto = boletos.Count > 1 ? "Os boletos bancários seguem em anexo" : "O boleto bancário segue em anexo";

            //Versão em texto puro para os filtros de Spam
            bodyBuilder.TextBody = $"Olá! Sua nota fiscal nº {numeroNota} foi emitida. " +
                                   $"Acesse em: {linkNota}. {textoBoleto}";
            // Montagem do HTML com o Logo
            bodyBuilder.HtmlBody = $@"
            <div style='font-family: sans-serif; color: #333; max-width: 600px;'>
                {(string.IsNullOrEmpty(cidLogo) ? "" : $"<img src='cid:{cidLogo}' style='width: 150px; margin-bottom: 20px;' />")}
                <h2 style='color: #76b82a;'>Olá!</h2>
                <p>Informamos que a sua <b>Nota Fiscal nº {numeroNota}</b> foi emitida com sucesso.</p>
                <p>Você pode visualizá-la diretamente no portal da prefeitura clicando no botão abaixo:</p>
                <p style='margin: 30px 0;'>
                    <a href='{linkNota}' style='background-color: #2e2e2e; color: white; padding: 12px 25px; text-decoration: none; border-radius: 5px; font-weight: bold;'>VER NOTA FISCAL</a>
                </p>
                <hr style='border: 0; border-top: 1px solid #eee;' />
                <p><b>{textoBoleto} a este e-mail (PDF).</b></p>
                <br />
                <p style='font-size: 12px; color: #999;'>MGM Engenharia de Segurança e Medicina do Trabalho<br/>Este é um e-mail automático, por favor não responda.</p>
            </div>";

            // Anexo do Boleto
            foreach (var boleto in boletos)
            {
                  if (!string.IsNullOrEmpty(boleto.Base64) && boleto.Base64.Length > 500)
                  {
                        try
                        {
                              byte[] pdfBytes = Convert.FromBase64String(boleto.Base64);
                              bodyBuilder.Attachments.Add($"Boleto_MGM_{numeroNota}_Parcela_{boleto.NumeroParcela:D2}.pdf", pdfBytes);
                        }
                        catch (Exception ex)
                        {
                              Console.WriteLine($"[AVISO EMAIL] Base64 do boleto era inválido: {ex.Message}");
                        }
                  }
                  else if (!string.IsNullOrEmpty(boleto.Base64))
                  {
                        Console.WriteLine("[INFO EMAIL] Boleto ignorado por ser string de Sandbox (muito curta).");
                  }
            }

            message.Body = bodyBuilder.ToMessageBody();
            return await EnviarSmtpAsync(message);
      }

      // --- 3. GERADOR DE MENSAGEM ---
      private MimeMessage GerarMensagemBase(string destino, string assunto)
      {
            var message = new MimeMessage();
            var remetenteEmail = _config["EmailConfig:Usuario"] ?? "financeiro@mgm.com.br";

            message.From.Add(new MailboxAddress("MGM Faturamento", remetenteEmail));
            message.To.Add(new MailboxAddress("Cliente", destino));
            message.Subject = assunto;
            return message;
      }

      // --- 4. MOTOR DE ENVIO ---
      private async Task<bool> EnviarSmtpAsync(MimeMessage message)
      {
            using var client = new SmtpClient();
            try
            {
                  var host = _config["EmailConfig:SmtpServer"] ?? "localhost";
                  var porta = _config.GetValue<int>("EmailConfig:Porta");
                  var user = _config["EmailConfig:Usuario"] ?? "";
                  var pass = _config["EmailConfig:Senha"] ?? "";

                  // Conecta e envia
                  await client.ConnectAsync(host, porta, MailKit.Security.SecureSocketOptions.StartTls);
                  await client.AuthenticateAsync(user, pass);
                  await client.SendAsync(message);

                  Console.WriteLine("[EMAIL] Enviado com sucesso para: " + message.To.ToString());
                  return true;
            }
            catch (Exception ex)
            {
                  Console.WriteLine($"[ERRO EMAIL] Falha ao enviar: {ex.Message}");
                  return false;
            }
            finally
            {
                  if (client.IsConnected)
                        await client.DisconnectAsync(true);
            }
      }

      public class BoletoAnexo
      {
            public string Base64 { get; set; } = string.Empty;
            public int NumeroParcela { get; set; }
      }
}