(function (app) {
    "use strict";

    console.log('🎥 Camera Helper loaded and ready');

    let videoStream = null;
    let videoElement = null;
    let canvasElement = null;

    // Initialize camera with specified constraints
    app.initializeCamera = async function (videoElementId, canvasElementId) {
        try {
            console.log('📹 Initializing camera with elements:', { videoElementId, canvasElementId });
            
            videoElement = document.getElementById(videoElementId);
            canvasElement = document.getElementById(canvasElementId);

            if (!videoElement || !canvasElement) {
                throw new Error('Video or canvas element not found');
            }

            // Check if getUserMedia is supported
            if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
                throw new Error('Camera not supported in this browser');
            }

            // Configure camera constraints
            const constraints = {
                video: {
                    width: { ideal: 1920, max: 1920 },
                    height: { ideal: 1080, max: 1080 },
                    facingMode: { ideal: 'environment' }, // Use rear camera on mobile
                    aspectRatio: { ideal: 16/9 }
                },
                audio: false
            };

            console.log('📹 Requesting camera access with constraints:', constraints);

            // Request camera access
            videoStream = await navigator.mediaDevices.getUserMedia(constraints);
            videoElement.srcObject = videoStream;
            
            // Wait for video to be ready
            await new Promise((resolve) => {
                videoElement.onloadedmetadata = () => {
                    videoElement.play();
                    console.log('📹 Camera initialized successfully');
                    resolve();
                };
            });

            return { success: true };
        } catch (error) {
            console.error('📹 Camera initialization error:', error);
            return { success: false, error: error.message };
        }
    };

    // Capture photo from video stream
    app.capturePhoto = function () {
        try {
            console.log('📸 Capturing photo');
            
            if (!videoElement || !canvasElement || !videoStream) {
                throw new Error('Camera not initialized');
            }

            // Set canvas dimensions to match video
            const videoWidth = videoElement.videoWidth;
            const videoHeight = videoElement.videoHeight;
            
            canvasElement.width = videoWidth;
            canvasElement.height = videoHeight;

            console.log('📸 Photo dimensions:', { videoWidth, videoHeight });

            // Draw current video frame to canvas
            const context = canvasElement.getContext('2d');
            context.drawImage(videoElement, 0, 0, videoWidth, videoHeight);

            // Convert canvas to blob
            return new Promise((resolve) => {
                canvasElement.toBlob((blob) => {
                    if (blob) {
                        // Convert blob to base64
                        const reader = new FileReader();
                        reader.onload = function() {
                            const base64Data = reader.result.split(',')[1]; // Remove data:image/jpeg;base64, prefix
                            console.log('📸 Photo captured successfully, size:', blob.size, 'bytes');
                            resolve({
                                success: true,
                                imageData: base64Data,
                                mimeType: 'image/jpeg',
                                width: videoWidth,
                                height: videoHeight
                            });
                        };
                        reader.readAsDataURL(blob);
                    } else {
                        console.error('📸 Failed to create blob from canvas');
                        resolve({ success: false, error: 'Failed to capture image' });
                    }
                }, 'image/jpeg', 0.9); // High quality JPEG
            });
        } catch (error) {
            console.error('📸 Photo capture error:', error);
            return Promise.resolve({ success: false, error: error.message });
        }
    };

    // Stop camera and release resources
    app.stopCamera = function () {
        try {
            console.log('🛑 Stopping camera');
            
            if (videoStream) {
                videoStream.getTracks().forEach(track => {
                    track.stop();
                });
                videoStream = null;
            }

            if (videoElement) {
                videoElement.srcObject = null;
                videoElement = null;
            }

            canvasElement = null;

            console.log('🛑 Camera stopped successfully');
            return { success: true };
        } catch (error) {
            console.error('🛑 Stop camera error:', error);
            return { success: false, error: error.message };
        }
    };

    // Check if camera is available
    app.isCameraAvailable = async function () {
        try {
            console.log('🔍 Checking camera availability');
            
            if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
                console.log('🔍 Camera API not supported');
                return { available: false, error: 'Camera API not supported' };
            }

            // Try to enumerate devices to check if camera exists
            const devices = await navigator.mediaDevices.enumerateDevices();
            // Get devices with camera
            const devicesWithCamera = devices.filter(device => device.kind === 'videoinput');
            const hasCamera = devicesWithCamera.length > 0;

            console.log('🔍 Camera availability:', hasCamera, 'Total devices with camera:', devicesWithCamera.length);
            return { available: hasCamera };
        } catch (error) {
            console.error('🔍 Camera availability check error:', error);
            return { available: false, error: error.message };
        }
    };

    // Get available camera devices
    app.getCameraDevices = async function () {
        try {
            console.log('📹 Getting camera devices');
            
            const devices = await navigator.mediaDevices.enumerateDevices();
            const cameras = devices
                .filter(device => device.kind === 'videoinput')
                .map(device => ({
                    deviceId: device.deviceId,
                    label: device.label || `Camera ${device.deviceId.substring(0, 8)}`,
                    groupId: device.groupId
                }));
            
            console.log('📹 Found cameras:', cameras);
            return { success: true, cameras };
        } catch (error) {
            console.error('📹 Get camera devices error:', error);
            return { success: false, error: error.message, cameras: [] };
        }
    };

    // Switch to a specific camera device
    app.switchCamera = async function (deviceId, videoElementId, canvasElementId) {
        try {
            console.log('🔄 Switching camera to device:', deviceId);
            
            // Stop current stream first
            app.stopCamera();
            
            // Reinitialize with specific device
            videoElement = document.getElementById(videoElementId);
            canvasElement = document.getElementById(canvasElementId);

            const constraints = {
                video: {
                    deviceId: deviceId ? { exact: deviceId } : undefined,
                    width: { ideal: 1920, max: 1920 },
                    height: { ideal: 1080, max: 1080 },
                    aspectRatio: { ideal: 16/9 }
                },
                audio: false
            };

            videoStream = await navigator.mediaDevices.getUserMedia(constraints);
            videoElement.srcObject = videoStream;
            
            await new Promise((resolve) => {
                videoElement.onloadedmetadata = () => {
                    videoElement.play();
                    console.log('🔄 Camera switch successful');
                    resolve();
                };
            });

            return { success: true };
        } catch (error) {
            console.error('🔄 Switch camera error:', error);
            return { success: false, error: error.message };
        }
    };

})((window.cameraHelper = window.cameraHelper || {}));

console.log('🎥 Camera Helper module fully loaded');