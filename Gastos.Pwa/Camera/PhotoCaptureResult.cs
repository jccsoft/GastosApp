namespace Gastos.Pwa.Camera;

public class PhotoCaptureResult : CameraResult
{
    public string? ImageData { get; set; }
    public string? MimeType { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}
