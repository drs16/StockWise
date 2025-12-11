using StockWise.Client.Modelo;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;

namespace StockWise.Client.Services;

/// <summary>
/// Servicio encargado de gestionar todas las comunicaciones entre
/// la aplicación MAUI y la API StockWise.
/// 
/// Incluye:
/// - Autenticación JWT
/// - CRUD de productos
/// - Gestión de usuarios
/// - Gestión de empresa
/// - Exportación CSV
/// - Lectura de QR
/// - Manejo de token y SecureStorage
/// </summary>
public class ApiService
{
    private readonly HttpClient _httpClient;
    private string _token;

    /// <summary>
    /// Determina si el cliente usará la API local (Swagger)
    /// o la API desplegada en Render.
    /// </summary>
    private readonly bool _useLocal = false;

    private readonly string _baseUrl;

    /// <summary>
    /// Inicializa el servicio estableciendo la URL base y configurando HttpClient.
    /// </summary>
    public ApiService()
    {
        _baseUrl = _useLocal
            ? "https://localhost:7013/api/"
            : "https://stockwise-api-82wo.onrender.com/api/";

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_baseUrl)
        };

        Console.WriteLine($"[CLIENT-INIT] Base URL configurada: {_baseUrl}");
    }

    public HttpClient HttpClient => _httpClient;

    /// <summary>
    /// Asigna el token JWT recibido tras el login y lo añade
    /// automáticamente a las cabeceras Authorization.
    /// </summary>
    public void SetToken(string token)
    {
        _token = token;
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        Console.WriteLine($"[CLIENT-TOKEN] Token asignado correctamente.");
    }

    /// <summary>
    /// Realiza el login llamando al endpoint Auth/login, obtiene el token
    /// y guarda datos del usuario (rol, nombre, empresa) en SecureStorage.
    /// </summary>
    public async Task<string?> LoginAsync(string email, string password)
    {
        try
        {
            var json = JsonSerializer.Serialize(new
            {
                Email = email,
                password = password
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("Auth/login", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode) return null;

            using var doc = JsonDocument.Parse(responseBody);
            var token = doc.RootElement.GetProperty("token").GetString();

            // 🔍 Leer claims del JWT
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            await SecureStorage.SetAsync("usuario_nombre",
                jwt.Claims.FirstOrDefault(c => c.Type.Contains("identity/claims/name"))?.Value ?? "");

            await SecureStorage.SetAsync("usuario_rol",
                jwt.Claims.FirstOrDefault(c => c.Type.Contains("identity/claims/role"))?.Value ?? "");

            await SecureStorage.SetAsync("empresa_id",
                jwt.Claims.FirstOrDefault(c => c.Type == "EmpresaId")?.Value ?? "");

            return token;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Obtiene un producto mediante su código QR.
    /// </summary>
    public async Task<ProductoDto?> GetProductoPorQR(string codigoQR)
    {
        var response = await _httpClient.GetAsync($"Productos/porQR/{codigoQR}");

        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<ProductoDto>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    /// <summary>
    /// Crea un nuevo producto (solo administradores).
    /// </summary>
    public async Task<bool> CrearProductoAsync(ProductoDto producto)
    {
        var json = JsonSerializer.Serialize(producto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("Productos", content);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Edita un producto existente.
    /// </summary>
    public async Task<bool> EditarProductoAsync(int id, ProductoDto producto)
    {
        var json = JsonSerializer.Serialize(producto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PutAsync($"Productos/{id}", content);

        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Elimina un producto por ID.
    /// </summary>
    public async Task<bool> EliminarProductoAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"Productos/{id}");
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Devuelve todos los productos relacionados con la empresa del usuario.
    /// </summary>
    public async Task<List<ProductoDto>> GetProductosAsync()
    {
        try
        {
            var empresaIdString = await SecureStorage.GetAsync("empresa_id");

            if (string.IsNullOrEmpty(empresaIdString))
                return new List<ProductoDto>();

            int empresaId = int.Parse(empresaIdString);

            var response = await _httpClient.GetAsync($"Productos/porEmpresa/{empresaId}");

            if (!response.IsSuccessStatusCode)
                return new List<ProductoDto>();

            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<List<ProductoDto>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }
        catch
        {
            return new List<ProductoDto>();
        }
    }

    /// <summary>
    /// Importa una lista de productos en bloque.
    /// </summary>
    public async Task<bool> ImportarProductosAsync(List<ProductoDto> productos)
    {
        try
        {
            var json = JsonSerializer.Serialize(productos);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("Productos/importar", content);

            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Obtiene todos los usuarios registrados (solo administradores).
    /// </summary>
    public async Task<List<UsuarioDto>> GetUsuariosAsync()
    {
        var token = await SecureStorage.GetAsync("jwt_token");
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.GetAsync("Usuarios");

        if (!response.IsSuccessStatusCode)
            return new List<UsuarioDto>();

        var json = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<List<UsuarioDto>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    /// <summary>
    /// Crea un usuario y devuelve la contraseña temporal generada.
    /// </summary>
    public async Task<string> CrearUsuarioAsync(CrearUsuarioDto dto)
    {
        var token = await SecureStorage.GetAsync("jwt_token");
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var json = JsonSerializer.Serialize(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var resp = await _httpClient.PostAsync("Usuarios", content);
        var result = await resp.Content.ReadAsStringAsync();

        if (!resp.IsSuccessStatusCode)
            return $"ERROR:{result}";

        using var doc = JsonDocument.Parse(result);
        return doc.RootElement.GetProperty("tempPassword").GetString()!;
    }

    /// <summary>
    /// Elimina un usuario por ID.
    /// </summary>
    public async Task<bool> EliminarUsuarioAsync(int id)
    {
        var token = await SecureStorage.GetAsync("jwt_token");

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.DeleteAsync($"Usuarios/{id}");

        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Exporta el inventario a CSV para la empresa indicada.
    /// </summary>
    public async Task<byte[]> ExportarCSVAsync(int empresaId)
    {
        var response = await _httpClient.GetAsync($"Productos/exportar/{empresaId}");

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadAsByteArrayAsync();
    }

    /// <summary>
    /// Obtiene información de la empresa del usuario actual.
    /// </summary>
    public async Task<EmpresaDto?> ObtenerMiEmpresaAsync()
    {
        var empresaIdString = await SecureStorage.GetAsync("empresa_id");

        if (string.IsNullOrEmpty(empresaIdString))
            return null;

        int empresaId = int.Parse(empresaIdString);

        var response = await _httpClient.GetAsync($"Empresas/{empresaId}");

        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<EmpresaDto>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    /// <summary>
    /// Actualiza los datos de una empresa.
    /// </summary>
    public async Task<bool> ActualizarEmpresaAsync(int id, EmpresaDto empresa)
    {
        var json = JsonSerializer.Serialize(empresa);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PutAsync($"Empresas/{id}", content);

        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Obtiene un producto mediante un código QR validando token y formato.
    /// </summary>
    public async Task<ProductoDto?> GetProductoByQRAsync(string qr)
    {
        if (string.IsNullOrWhiteSpace(qr))
            return null;

        // Asegurar token cargado
        if (string.IsNullOrEmpty(_token))
        {
            var storedToken = await SecureStorage.GetAsync("jwt_token");
            if (!string.IsNullOrEmpty(storedToken))
            {
                _token = storedToken;
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", storedToken);
            }
        }

        var safe = Uri.EscapeDataString(qr);

        var resp = await _httpClient.GetAsync($"Productos/porQR/{safe}");

        if (!resp.IsSuccessStatusCode)
            return null;

        return await resp.Content.ReadFromJsonAsync<ProductoDto?>(
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    /// <summary>
    /// Actualiza el stock de un producto.
    /// </summary>
    public async Task<bool> UpdateStockAsync(ProductoDto producto)
    {
        var dto = new { NuevaCantidad = producto.Cantidad };

        var response = await _httpClient.PostAsJsonAsync(
            $"Productos/{producto.Id}/actualizarStock", dto);

        return response.IsSuccessStatusCode;
    }


    /// <summary>
    /// Resetea la contraseña de un usuario (solo administradores).
    /// </summary>
    public async Task<string?> ResetearPassword(int usuarioId)
    {
        var token = await SecureStorage.GetAsync("jwt_token");

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var resp = await _httpClient.PostAsync($"Usuarios/{usuarioId}/reset-password", null);

        if (!resp.IsSuccessStatusCode)
            return null;

        var json = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        return doc.RootElement.GetProperty("tempPassword").GetString();
    }

    /// <summary>
    /// Permite que el usuario cambie su propia contraseña.
    /// </summary>
    public async Task<bool> CambiarMiPassword(string nueva)
    {
        var token = await SecureStorage.GetAsync("jwt_token");
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var json = JsonSerializer.Serialize(new { NuevaPassword = nueva });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("Usuarios/cambiarPassword", content);

        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Actualiza los datos de un usuario.
    /// </summary>
    public async Task<bool> ActualizarUsuarioAsync(UsuarioDto usuario)
    {
        var token = await SecureStorage.GetAsync("jwt_token");
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var dto = new EditarUsuarioDto
        {
            NombreUsuario = usuario.NombreUsuario,
            Email = usuario.Email
        };

        var json = JsonSerializer.Serialize(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PutAsync($"usuarios/{usuario.Id}", content);

        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Obtiene los movimientos de stock de la empresa para auditorías.
    /// </summary>
    public async Task<List<MovimientoStockDto>> GetMovimientosAsync(int empresaId)
    {
        var token = await SecureStorage.GetAsync("jwt_token");

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var resp = await _httpClient.GetAsync($"Productos/movimientos/{empresaId}");

        if (!resp.IsSuccessStatusCode)
            return new List<MovimientoStockDto>();

        var json = await resp.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<List<MovimientoStockDto>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    /// <summary>
    /// Obtiene Todas las empresas.
    /// </summary>
    public async Task<EmpresaDto?> GetEmpresaAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"Empresas/{id}");

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<EmpresaDto>();
        }
        catch
        {
            return null;
        }
    }

}
