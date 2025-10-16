namespace Gastos.Pwa.Camera;

public interface ICameraService
{
    Task<CameraResult> InitializeCameraAsync(string videoElementId, string canvasElementId);
    Task<PhotoCaptureResult> CapturePhotoAsync();
    Task<CameraResult> StopCameraAsync();
    Task<CameraAvailabilityResult> IsCameraAvailableAsync();
    Task<CameraDevicesResult> GetCameraDevicesAsync();
    Task<CameraResult> SwitchCameraAsync(string deviceId, string videoElementId, string canvasElementId);
}
