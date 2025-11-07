using System.Net.Http.Headers;

namespace Harvest.Services;

public class GotenbergService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GotenbergService> _logger;

    public GotenbergService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<GotenbergService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Convert HTML to PDF using Gotenberg
    /// </summary>
    public async Task<byte[]?> ConvertHtmlToPdfAsync(string html, string documentTitle = "document")
    {
        try
        {
            var gotenbergUrl = _configuration.GetValue<string>("GotenbergSettings:Url", "http://localhost:3000");
            var endpoint = $"{gotenbergUrl}/forms/chromium/convert/html";

            using var httpClient = _httpClientFactory.CreateClient();
            using var form = new MultipartFormDataContent();

            // Add HTML content
            var htmlContent = new StringContent(html);
            htmlContent.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            form.Add(htmlContent, "files", "index.html");

            // Add paper size and margins
            form.Add(new StringContent("A4"), "paperWidth");
            form.Add(new StringContent("A4"), "paperHeight");
            form.Add(new StringContent("0.5"), "marginTop");
            form.Add(new StringContent("0.5"), "marginBottom");
            form.Add(new StringContent("0.5"), "marginLeft");
            form.Add(new StringContent("0.5"), "marginRight");

            // Add PDF metadata
            form.Add(new StringContent(documentTitle), "pdftitle");

            // Send request
            var response = await httpClient.PostAsync(endpoint, form);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Gotenberg conversion failed: {error}");
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting HTML to PDF");
            return null;
        }
    }

    /// <summary>
    /// Convert a URL to PDF using Gotenberg
    /// </summary>
    public async Task<byte[]?> ConvertUrlToPdfAsync(string url, string documentTitle = "document")
    {
        try
        {
            var gotenbergUrl = _configuration.GetValue<string>("GotenbergSettings:Url", "http://localhost:3000");
            var endpoint = $"{gotenbergUrl}/forms/chromium/convert/url";

            using var httpClient = _httpClientFactory.CreateClient();
            using var form = new MultipartFormDataContent();

            // Add URL
            form.Add(new StringContent(url), "url");

            // Add paper size and margins
            form.Add(new StringContent("A4"), "paperWidth");
            form.Add(new StringContent("A4"), "paperHeight");
            form.Add(new StringContent("0.5"), "marginTop");
            form.Add(new StringContent("0.5"), "marginBottom");
            form.Add(new StringContent("0.5"), "marginLeft");
            form.Add(new StringContent("0.5"), "marginRight");

            // Add PDF metadata
            form.Add(new StringContent(documentTitle), "pdftitle");

            // Send request
            var response = await httpClient.PostAsync(endpoint, form);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Gotenberg URL conversion failed: {error}");
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting URL to PDF");
            return null;
        }
    }

    /// <summary>
    /// Generate invoice PDF
    /// </summary>
    public async Task<byte[]?> GenerateInvoicePdfAsync(int invoiceId, string baseUrl)
    {
        var printUrl = $"{baseUrl}/print/invoice/{invoiceId}";
        return await ConvertUrlToPdfAsync(printUrl, $"Invoice-{invoiceId}");
    }

    /// <summary>
    /// Generate sales order PDF
    /// </summary>
    public async Task<byte[]?> GenerateOrderPdfAsync(int orderId, string baseUrl)
    {
        var printUrl = $"{baseUrl}/print/order/{orderId}";
        return await ConvertUrlToPdfAsync(printUrl, $"Order-{orderId}");
    }
}
