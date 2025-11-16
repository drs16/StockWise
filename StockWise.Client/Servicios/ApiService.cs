using StockWise.Client.Modelo;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;

namespace StockWise.Client.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private string _token;

    // 🟢 Cambia este valor según lo que quieras probar:
    // true → Usa API local (Swagger / Visual Studio)
    // false → Usa API en Render (producción)
    private readonly bool _useLocal = true;

    private readonly string _baseUrl;

    public ApiService()
    {
        _baseUrl = _useLocal
            ? "https://localhost:7013/api/"  // ✅ coincide con tu Swagger
            : "https://stockwise-api-82wo.onrender.com/api";

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_baseUrl)
        };

        Console.WriteLine($"[CLIENT-INIT] Base URL configurada: {_baseUrl}");
    }

    public void SetToken(string token)
    {
        _token = token;
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        Console.WriteLine($"[CLIENT-TOKEN] Token asignado correctamente: {_token.Substring(0, 10)}...");
    }

    public async Task<string?> LoginAsync(string email, string password)
    {
        try
        {
            Console.WriteLine($"[CLIENT-LOGIN] Iniciando login con Email: {email}");

            var json = JsonSerializer.Serialize(new
            {
                email = email,
                passwordHash = password
            });

            Console.WriteLine($"[CLIENT-LOGIN] JSON a enviar: {json}");

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // 🧩 Mostrar la URL completa que se va a llamar
            var endpoint = $"{_httpClient.BaseAddress}Auth/login";
            Console.WriteLine($"[CLIENT-LOGIN] Intentando POST a: {endpoint}");
            await Application.Current.MainPage.DisplayAlert("DEBUG", $"Llamando a:\n{endpoint}", "OK");

            // 🚀 Ejecutar llamada
            var response = await _httpClient.PostAsync("Auth/login", content);

            Console.WriteLine($"[CLIENT-LOGIN] Respuesta HTTP: {(int)response.StatusCode} - {response.StatusCode}");

            var responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[CLIENT-LOGIN] Cuerpo de respuesta: {responseBody}");

            if (!response.IsSuccessStatusCode)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Error HTTP {response.StatusCode}", "OK");
                return null;
            }

            using var doc = JsonDocument.Parse(responseBody);
            var token = doc.RootElement.GetProperty("token").GetString();

            Console.WriteLine($"[CLIENT-LOGIN] Token recibido correctamente (longitud: {token?.Length ?? 0})");

            return token;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CLIENT-ERROR] Excepción en LoginAsync: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("EXCEPCIÓN", ex.Message, "OK");
            return null;
        }
    }

    public async Task<ProductoDto?> GetProductoPorQR(string codigoQR)
    {
        var response = await _httpClient.GetAsync($"Productos/porQR/{codigoQR}");

        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<ProductoDto>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<bool> CrearProductoAsync(ProductoDto producto)
    {
        var json = JsonSerializer.Serialize(producto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("Productos", content);
        return response.IsSuccessStatusCode;
    }
    public async Task<bool> EditarProductoAsync(int id, ProductoDto producto)
    {
        var json = JsonSerializer.Serialize(producto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PutAsync($"Productos/{id}", content);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> EliminarProductoAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"Productos/{id}");
        return response.IsSuccessStatusCode;
    }




    public async Task<List<ProductoDto>> GetProductosAsync()
    {
        try
        {
            Console.WriteLine("[CLIENT-PRODUCTOS] Solicitando lista de productos...");

            var response = await _httpClient.GetAsync("Productos");

            Console.WriteLine($"[CLIENT-PRODUCTOS] HTTP Status: {(int)response.StatusCode} - {response.StatusCode}");

            var json = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"[CLIENT-PRODUCTOS] Respuesta completa: {json}");

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error al obtener productos: {response.StatusCode}");

            var productos = JsonSerializer.Deserialize<List<ProductoDto>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

            Console.WriteLine($"[CLIENT-PRODUCTOS] Productos recibidos: {productos.Count}");

            return productos;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CLIENT-ERROR] Error en GetProductosAsync: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("EXCEPCIÓN", ex.Message, "OK");
            return new List<ProductoDto>();
        }
    }

    public async Task<bool> ImportarProductosAsync(List<ProductoDto> productos)
    {
        try
        {
            var json = JsonSerializer.Serialize(productos);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("Productos/importar", content);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error en ImportarProductosAsync: {ex.Message}");
            return false;
        }
    }

    public async Task<List<UsuarioDto>> GetUsuariosAsync()
    {
        var token = await SecureStorage.GetAsync("jwt_token");

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.GetAsync("Usuarios");

        var json = await response.Content.ReadAsStringAsync();
        Console.WriteLine("[GET-USUARIOS] Respuesta: " + json);

        if (!response.IsSuccessStatusCode)
            return new List<UsuarioDto>();

        return JsonSerializer.Deserialize<List<UsuarioDto>>(json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })!;
    }


    public async Task<bool> CrearUsuarioAsync(UsuarioDto usuario)
    {
        var token = await SecureStorage.GetAsync("jwt_token");

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var json = JsonSerializer.Serialize(usuario);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("Usuarios", content);

        var body = await response.Content.ReadAsStringAsync();
        Console.WriteLine("[CREAR-USUARIO] Respuesta: " + body);

        return response.IsSuccessStatusCode;
    }




    public async Task<bool> ActualizarUsuarioAsync(UsuarioDto usuario)
    {
        var token = await SecureStorage.GetAsync("jwt_token");

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var json = JsonSerializer.Serialize(usuario);

        var response = await _httpClient.PutAsync($"Usuarios/{usuario.Id}",
            new StringContent(json, Encoding.UTF8, "application/json"));

        return response.IsSuccessStatusCode;
    }


    public async Task<bool> EliminarUsuarioAsync(int id)
    {
        var token = await SecureStorage.GetAsync("jwt_token");

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.DeleteAsync($"Usuarios/{id}");

        return response.IsSuccessStatusCode;
    }


    private string ObtenerClaim(string token, string claimType)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        return jwt.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
    }
}
