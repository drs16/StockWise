using System;
using System.Collections.Generic;
using System.Text;

namespace StockWise.Client.Componentes
{

    public static class PopupHelper
    {
        public static async Task MostrarAsync(INavigation nav, string titulo, string mensaje, string? copiable = null)
        {
            var popup = new MensajeModalPage(titulo, mensaje, copiable);
            await nav.PushModalAsync(popup);
            await popup.EsperarCierre;
        }
    }

}
