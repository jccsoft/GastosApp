namespace Gastos.Pwa.Camera;

public class CameraService(IJSRuntime jsRuntime, ILogger<CameraService> logger) : ICameraService
{
    public async Task<CameraInitResult> InitializeCameraAsync(string videoElementId, string canvasElementId)
    {
        try
        {
            var result = await jsRuntime.InvokeAsync<CameraInitResult>(
                "cameraHelper.initializeCamera", videoElementId, canvasElementId);

            logger.LogInformation("Camera initialization result: {Success}", result.Success);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error initializing camera");
            return new CameraInitResult { Success = false, Error = ex.Message };
        }
    }

    public async Task<PhotoCaptureResult> CapturePhotoAsync()
    {
        try
        {
            var result = await jsRuntime.InvokeAsync<PhotoCaptureResult>("cameraHelper.capturePhoto");

            logger.LogInformation("Photo capture result: {Success}", result.Success);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error capturing photo");
            return new PhotoCaptureResult { Success = false, Error = ex.Message };
        }
    }

    public async Task<CameraResult> StopCameraAsync()
    {
        try
        {
            var result = await jsRuntime.InvokeAsync<CameraResult>("cameraHelper.stopCamera");

            logger.LogInformation("Stop camera result: {Success}", result.Success);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error stopping camera");
            return new CameraResult { Success = false, Error = ex.Message };
        }
    }

    public async Task<CameraAvailabilityResult> IsCameraAvailableAsync()
    {
        try
        {
            var result = await jsRuntime.InvokeAsync<CameraAvailabilityResult>("cameraHelper.isCameraAvailable");

            logger.LogInformation("Camera availability: {Available}", result.Available);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking camera availability");
            return new CameraAvailabilityResult { Available = false, Error = ex.Message };
        }
    }

    public async Task<CameraDevicesResult> GetCameraDevicesAsync()
    {
        try
        {
            var result = await jsRuntime.InvokeAsync<CameraDevicesResult>("cameraHelper.getCameraDevices");

            logger.LogInformation("Found {Count} camera devices", result.Cameras?.Count ?? 0);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting camera devices");
            return new CameraDevicesResult { Success = false, Error = ex.Message, Cameras = [] };
        }
    }

    public async Task<CameraInitResult> SwitchCameraAsync(string deviceId, string videoElementId, string canvasElementId)
    {
        try
        {
            var result = await jsRuntime.InvokeAsync<CameraInitResult>(
                "cameraHelper.switchCamera", deviceId, videoElementId, canvasElementId);

            logger.LogInformation("Switch camera result: {Success}", result.Success);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error switching camera");
            return new CameraInitResult { Success = false, Error = ex.Message };
        }
    }
}
