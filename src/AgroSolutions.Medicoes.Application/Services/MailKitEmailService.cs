using System.Diagnostics;
using AgroSolutions.Medicoes.Application.Interfaces.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace AgroSolutions.Medicoes.Application.Services;

public class MailKitEmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<MailKitEmailService> _logger;

    public MailKitEmailService(
        IOptions<EmailSettings> options, 
        ILogger<MailKitEmailService> logger)
    {
        _settings = options.Value;
        _logger = logger;    
    }

    public async Task EnviarEmailAsync(string para, string assunto, string corpoHtml)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation(
                "Iniciando envio de e-mail Assunto={assunto}",
                assunto
            );
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Agro Solutions", _settings.From));
            message.To.Add(MailboxAddress.Parse(para));
            message.Subject = assunto;

            message.Body = new BodyBuilder
            {
                HtmlBody = corpoHtml
            }.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(
                _settings.Host,
                _settings.Port,
                _settings.UseSsl
                    ? SecureSocketOptions.StartTls
                    : SecureSocketOptions.None
            );

            _logger.LogInformation(
                "Enviando e-mail Assunto={assunto}", 
                para
            );

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Finalizando envio de e-mail via MailKit após {duracao}ms.", stopwatch.ElapsedMilliseconds);
        } 
        catch(AuthenticationException ex)
        {
            _logger.LogError(
                ex,
                "Falha de autenticação no servidor SMTP Host={Host}",
                _settings.Host);

            throw;
        } 
        catch(Exception ex)
        {
            _logger.LogError(
                ex,
                "Erro inesperado ao enviar o e-mail Assunto={Assunto}",
                assunto);

            throw;
        }
    }
}
