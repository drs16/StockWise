public class MovimientoStockDto
{
    public string NombreProducto { get; set; }
    public string NombreUsuario { get; set; }
    public int CantidadAntes { get; set; }
    public int CantidadDespues { get; set; }
    public DateTime Fecha { get; set; }

    // Diferencia como texto: "+3 unidades" o "-2 unidades"
    public string Diferencia =>
        CantidadDespues - CantidadAntes > 0
        ? $"+{CantidadDespues - CantidadAntes} unidades"
        : $"{CantidadDespues - CantidadAntes} unidades";

    // Color verde si suma, rojo si resta
    public string ColorCambio =>
        CantidadDespues - CantidadAntes > 0 ? "#4EE1A0" : "#FF4E4E";

    // Fecha formateada
    public string FechaFormateada =>
        $"Fecha del movimiento: {Fecha:dd/MM/yyyy HH:mm}";
}
