using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace OngDoacao.Servicos
{
    public class IaService
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;
        private readonly ILogger<IaService> _logger;

        public IaService(HttpClient httpClient, IConfiguration configuration, ILogger<IaService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _apiKey = configuration["Gemini:ApiKey"] ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? string.Empty;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> ObterConselhoDoGestorAsync(int estoque, int cestas, int pessoasFila, int cestasPlanejadas)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                return "Assistente indisponível: configure GEMINI_API_KEY. A distribuição continua disponível para confirmação manual.";
            }

            var prompt = $"Você é um assistente operacional de uma ONG de alimentos. " +
                         $"Há {estoque} unidades válidas, suficientes para até {cestas} cestas de 5 unidades. " +
                         $"Existem {pessoasFila} famílias aguardando e o sistema preparou um plano para {cestasPlanejadas} famílias. " +
                         $"O plano distribui 5 unidades por cesta e segue a ordem de cadastro, da mais antiga para a mais recente. " +
                         "Resuma esse plano para revisão humana e dê uma orientação operacional breve, sem alterar a fila, escolher beneficiários, inventar dados ou afirmar que a distribuição já aconteceu.";

            var requestBody = new
            {
                contents = new[] { new { parts = new[] { new { text = prompt } } } }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-flash-latest:generateContent";

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = content
                };
                request.Headers.Add("X-goog-api-key", _apiKey);

                using var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("A API Gemini respondeu com o status HTTP {StatusCode}.", (int)response.StatusCode);
                    return "Assistente indisponível no momento. Revise e confirme a distribuição manualmente.";
                }

                var responseString = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(responseString))
                {
                    return "Assistente indisponível no momento. Revise e confirme a distribuição manualmente.";
                }

                using var document = JsonDocument.Parse(responseString);
                if (document.RootElement.TryGetProperty("candidates", out var candidates) &&
                    candidates.ValueKind == JsonValueKind.Array &&
                    candidates.GetArrayLength() > 0 &&
                    candidates[0].TryGetProperty("content", out var contentElement) &&
                    contentElement.TryGetProperty("parts", out var parts) &&
                    parts.ValueKind == JsonValueKind.Array &&
                    parts.GetArrayLength() > 0 &&
                    parts[0].TryGetProperty("text", out var textElement))
                {
                    var texto = textElement.GetString();
                    return string.IsNullOrWhiteSpace(texto)
                        ? "Assistente indisponível no momento. Revise e confirme a distribuição manualmente."
                        : texto;
                }
            }
            catch (HttpRequestException exception)
            {
                _logger.LogWarning(exception, "Não foi possível conectar à API Gemini.");
                return "Assistente indisponível no momento. Revise e confirme a distribuição manualmente.";
            }
            catch (TaskCanceledException exception)
            {
                _logger.LogWarning(exception, "A solicitação à API Gemini expirou ou foi cancelada.");
                return "Assistente indisponível no momento. Revise e confirme a distribuição manualmente.";
            }
            catch (JsonException exception)
            {
                _logger.LogWarning(exception, "A API Gemini retornou uma resposta JSON inválida.");
                return "Assistente indisponível no momento. Revise e confirme a distribuição manualmente.";
            }

            return "Assistente indisponível no momento. Revise e confirme a distribuição manualmente.";
        }
    }
}