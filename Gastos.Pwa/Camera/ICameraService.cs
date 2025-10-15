namespace Gastos.Pwa.Camera;

public interface ICameraService
{
    Task<CameraInitResult> InitializeCameraAsync(string videoElementId, string canvasElementId);
    Task<PhotoCaptureResult> CapturePhotoAsync();
    Task<CameraResult> StopCameraAsync();
    Task<CameraAvailabilityResult> IsCameraAvailableAsync();
    Task<CameraDevicesResult> GetCameraDevicesAsync();
    Task<CameraInitResult> SwitchCameraAsync(string deviceId, string videoElementId, string canvasElementId);
}
