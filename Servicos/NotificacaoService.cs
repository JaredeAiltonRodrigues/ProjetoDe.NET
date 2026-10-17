using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using OngDoacao.Models;

namespace OngDoacao.Servicos
{
    public class NotificacaoService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<NotificacaoService> _logger;

        public NotificacaoService(IConfiguration configuration, ILogger<NotificacaoService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ResultadoNotificacao> AvisarBeneficiarioAsync(Beneficiario pessoa)
        {
            if (string.IsNullOrWhiteSpace(pessoa.Email))
            {
            return new ResultadoNotificacao("Sem e-mail", "O cadastro não possui e-mail informado.");
            }

            string assunto = "Sua Cesta Básica está Pronta!";
            string mensagem = $"Olá {pessoa.Nome},\n\nSua cesta básica foi reservada com sucesso!\n" +
                              $"Sua posição na fila chegou. Por favor, retire na ONG no dia {pessoa.HorarioRetirada:dd/MM/yyyy} às {pessoa.HorarioRetirada:HH:mm}.\n\n" +
                              "Agradecemos o contato!";

            return await EnviarEmailAsync(pessoa.Email, assunto, mensagem);
        }

        private async Task<ResultadoNotificacao> EnviarEmailAsync(string emailDestino, string assunto, string texto)
        {
            var host = _configuration["Email:SmtpHost"];
            var username = _configuration["Email:Username"];
            var password = _configuration["Email:Password"];
            var fromAddress = _configuration["Email:FromAddress"];

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(fromAddress))
            {
                _logger.LogInformation("Notificação por e-mail não enviada: SMTP não configurado.");
                return new ResultadoNotificacao("Aguardando configuração", "O serviço de e-mail ainda não foi configurado.");
            }

            try
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(
                    _configuration["Email:FromName"] ?? "ONG Alimento para Todos",
                    fromAddress));
                email.To.Add(new MailboxAddress("", emailDestino));
                email.Subject = assunto;
                email.Body = new TextPart(MimeKit.Text.TextFormat.Plain) { Text = texto };

                using var smtp = new SmtpClient();
                var port = _configuration.GetValue<int?>("Email:SmtpPort") ?? 587;
                await smtp.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(username, password);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
                return new ResultadoNotificacao("Enviada", "E-mail enviado com sucesso.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao enviar uma notificação por e-mail.");
                return new ResultadoNotificacao("Falhou", "Não foi possível enviar o e-mail. Verifique a configuração SMTP.");
            }
        }
    }

    public sealed record ResultadoNotificacao(string Status, string Mensagem);
}