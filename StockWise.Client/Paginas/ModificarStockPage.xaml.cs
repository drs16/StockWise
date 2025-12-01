using Microsoft.Maui.Controls;
using StockWise.Client.Modelo;
using StockWise.Client.Services;

namespace StockWise.Client.Paginas
{
    [QueryProperty(nameof(QR), "qr")]
    public partial class ModificarStockPage : ContentPage
    {
        private readonly ApiService _api = new ApiService();
        private ProductoDto _producto;

        public string QR
        {
            set => _ = CargarProductoAsync(value);
        }

        public ModificarStockPage()
        {
            InitializeComponent();
        }

        private async Task CargarProductoAsync(string qr)
        {
            try
            {
                _producto = await _api.GetProductoByQRAsync(qr);

                if (_producto == null)
                {
                    await DisplayAlert("Error", "Producto no encontrado", "OK");
                    await Shell.Current.GoToAsync("..");
                    return;
                }

                NombreLabel.Text = _producto.Nombre;
                CodigoLabel.Text = $"Código: {_producto.CodigoQR}";
                StockLabel.Text = $"Stock actual: {_producto.Cantidad}";
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
    




private void OnAddClicked(object sender, EventArgs e)
        {
            if (int.TryParse(CantidadEntry.Text, out int cantidad) && cantidad > 0)
            {
                _producto.Cantidad += cantidad;
                StockLabel.Text = _producto.Cantidad.ToString();
                CantidadEntry.Text = ""; // limpiar
            }
            else
            {
                DisplayAlert("Error", "Introduce una cantidad válida", "OK");
            }
        }

        private void OnRemoveClicked(object sender, EventArgs e)
        {
            if (int.TryParse(CantidadEntry.Text, out int cantidad) && cantidad > 0)
            {
                if (_producto.Cantidad - cantidad >= 0)
                {
                    _producto.Cantidad -= cantidad;
                    StockLabel.Text = _producto.Cantidad.ToString();
                }
                else
                {
                    _producto.Cantidad = 0;
                    StockLabel.Text = "0";
                }

                CantidadEntry.Text = "";
            }
            else
            {
                DisplayAlert("Error", "Introduce una cantidad válida", "OK");
            }
        }


        private async void GuardarCambios(object sender, EventArgs e)
        {
            bool ok = await _api.UpdateStockAsync(_producto);

            if (!ok)
            {
                await DisplayAlert("Error", "No se pudo actualizar el stock", "OK");
                return;
            }

            await DisplayAlert("Éxito", "Stock actualizado correctamente", "OK");
            await Navigation.PopAsync();
        }

        private async void Cancelar(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private void KeypadClicked(object sender, EventArgs e)
        {
            if (sender is Button btn)
            {
                CantidadEntry.Text += btn.Text;
            }
        }

        private void KeypadDelete(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(CantidadEntry.Text))
                CantidadEntry.Text = CantidadEntry.Text.Substring(0, CantidadEntry.Text.Length - 1);
        }

        private void KeypadClear(object sender, EventArgs e)
        {
            CantidadEntry.Text = "";
        }

    }
}
