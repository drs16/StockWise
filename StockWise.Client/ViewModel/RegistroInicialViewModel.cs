using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StockWise.Client.Modelo;
using StockWise.Client.Services;
using Microsoft.Maui.Storage;
using System.Net.Http.Json;


namespace StockWise.Client.ViewModels
{
    public partial class RegistroInicialViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        public RegistroInicialViewModel()
        {
            _apiService = new ApiService();
        }

        // CAMPOS
        [ObservableProperty] string nombreEmpresa;
        [ObservableProperty] string nif;
        [ObservableProperty] string direccion;
        [ObservableProperty] string emailEmpresa;
        [ObservableProperty] string telefonoEmpresa;
        [ObservableProperty] string adminNombre;
        [ObservableProperty] string adminEmail;
        [ObservableProperty] string password;

        // BOTÓN GUARDAR
        [RelayCommand]
        public async Task Registrar()
        {
            var dto = new RegistroInicialDto
            {
                NombreEmpresa = NombreEmpresa,
                NIF = Nif,
                Direccion = Direccion,
                EmailEmpresa = EmailEmpresa,
                TelefonoEmpresa = TelefonoEmpresa,
                AdminNombre = AdminNombre,
                AdminEmail = AdminEmail,
                Password = Password
            };

            try
            {
                var response = await _apiService.HttpClient.PostAsJsonAsync("Auth/registroInicial", dto);

                if (!response.IsSuccessStatusCode)
                {
                    await App.Current.MainPage.DisplayAlert("Error",
                        "No se pudo registrar. ¿El NIF ya existe?",
                        "OK");
                    return;
                }

                var result = await response.Content.ReadFromJsonAsync<RegistroInicialRespuesta>();

                // GUARDAR TOKEN TEMPORAL (si lo necesitas para debug)
                await SecureStorage.SetAsync("auth_token", result.Token);

                // ⭐ MARCAR QUE YA SE HIZO EL REGISTRO INICIAL
                Preferences.Set("ModoSetup", false);
                Preferences.Set("RegistroInicialCompletado", true);
                // Borrar tokens por si acaso
                SecureStorage.Remove("jwt_token");
                SecureStorage.Remove("auth_token");

                await App.Current.MainPage.DisplayAlert("Éxito",
                    "Registro completado correctamente.",
                    "OK");

                // 🚀 IR AL LOGIN
                await Shell.Current.GoToAsync("//login");
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }

    }
}
