using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;

namespace StockWise.api.Servicios
{
    public class QRService
    {
        public byte[] GenerarQR(string texto)
        {
            using var generator = new QRCodeGenerator();
            var data = generator.CreateQrCode(texto, QRCodeGenerator.ECCLevel.Q);

            using var qrCode = new QRCode(data);
            using var bitmap = qrCode.GetGraphic(20);

            using var stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Png);

            return stream.ToArray();
        }
    }
}
