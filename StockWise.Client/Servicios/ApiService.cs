using StockWise.Client.Modelo;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace StockWise.Client.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private string _token;

    // 🟢 URL de la API en Render
    private readonly string _baseUrl = "https://stockwise-api.onrender.com/api";

    public ApiService()
    {
        _httpClient = new HttpClient();
    }

    public void SetToken(string token)
    {
        _token = token;
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<string?> LoginAsync(string email, string password)
    {
        var json = JsonSerializer.Serialize(new
        {
            email = email,
            passwordHash = password
        });

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_baseUrl}/Auth/login", content);

        if (!response.IsSuccessStatusCode)
            return null;

        var responseBody = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(responseBody);
        return doc.RootElement.GetProperty("token").GetString();
    }

    public async Task<List<ProductoDto>> GetProductosAsync()
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/Productos");
        if (!response.IsSuccessStatusCode)
            throw new Exception("Error al obtener productos.");

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<ProductoDto>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }
}
