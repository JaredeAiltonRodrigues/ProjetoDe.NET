using MailKit.Net.Smtp;
using MimeKit;
using OngDoacao.Models;

namespace OngDoacao.Servicos
{
    public class NotificacaoService
    {
        public async Task AvisarBeneficiarioAsync(Beneficiario pessoa)
        {
            string assunto = "Sua Cesta Básica está Pronta!";
            string mensagem = $"Olá {pessoa.Nome},\n\nSua cesta básica foi reservada com sucesso!\n" +
                              $"Sua posição na fila chegou. Por favor, retire na ONG no dia {pessoa.HorarioRetirada:dd/MM/yyyy} às {pessoa.HorarioRetirada:HH:mm}.\n\n" +
                              "Agradecemos o contato!";

            // 1. Tenta enviar o E-mail se a pessoa cadastrou um
            if (!string.IsNullOrEmpty(pessoa.Email))
            {
                await EnviarEmailAsync(pessoa.Email, assunto, mensagem);
            }

            // 2. Futura integração de WhatsApp (ex: Z-API, Twilio, Evolution API) entraria aqui
            // await EnviarWhatsAppAsync(pessoa.Telefone, mensagem);
        }

        private async Task EnviarEmailAsync(string emailDestino, string assunto, string texto)
        {
            try
            {
                var email = new MimeMessage();
                // Substitua pelo e-mail real da ONG depois
                email.From.Add(new MailboxAddress("ONG Alimento para Todos", "ongalimento@gmail.com"));
                email.To.Add(new MailboxAddress("", emailDestino));
                email.Subject = assunto;
                email.Body = new TextPart(MimeKit.Text.TextFormat.Plain) { Text = texto };

                using var smtp = new SmtpClient();
                // Exemplo usando servidor do Gmail (Para testar de verdade, você precisará gerar uma "Senha de Aplicativo" na sua conta Google)
                await smtp.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync("ongalimento@gmail.com", "sua_senha_de_aplicativo_aqui");
                
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Falha ao enviar e-mail para {emailDestino}: {ex.Message}");
            }
        }
    }
}