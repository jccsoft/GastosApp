namespace Gastos.Pwa.Camera;

public class PhotoCapturedEventArgs
{
    public byte[] ImageData { get; set; } = [];
    public string FileName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
}