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
                NIF = nif,
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

                // GUARDAR TOKEN
                await SecureStorage.SetAsync("auth_token", result.Token);

                // ⭐️ MARCAR QUE EL REGISTRO INICIAL SE COMPLETÓ
                Preferences.Set("RegistroInicialCompletado", true);
                Preferences.Set("ModoSetup", false);

                await App.Current.MainPage.DisplayAlert("Éxito",
                    "Registro completado correctamente.",
                    "OK");

                // 🚀 NAVEGAR AL FLUJO NORMAL
                await Shell.Current.GoToAsync("///MainPage");
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}
