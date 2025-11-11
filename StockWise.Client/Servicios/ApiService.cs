using StockWise.Client.Modelo;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace StockWise.Client.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private string _token;

    public ApiService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7013/api/")
        };
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

        var response = await _httpClient.PostAsync("Auth/login", content);

        if (!response.IsSuccessStatusCode)
            return null;

        var responseBody = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(responseBody);
        return doc.RootElement.GetProperty("token").GetString();
    }

    public async Task<List<ProductoDto>> GetProductosAsync()
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _token);

        var response = await client.GetAsync($"{_baseUrl}/api/Productos");
        if (!response.IsSuccessStatusCode)
            throw new Exception("Error al obtener productos.");

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<ProductoDto>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

}
