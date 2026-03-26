# WebXRVPS Template

Unity WebGL template for browser-based WebXR builds with VPS support.

## Included

- `index.html` adapted from a working WebXR build
- `arclip-webxr.js` implementing the `window.ARLib` facade expected by `ARClip.dll`
- `webxr-unity-bridge.js` implementing `window.ARLibNative` for WebXR
- `vps-client.js` for WebXR camera capture and VPS requests
- `TemplateData/` assets for the WebXR shell UI

## Requirements

- A Unity WebGL build that includes the WebXR runtime/export package used by your project
- `ARClip.dll` in the Unity project
- HTTPS hosting for camera and WebXR AR support

## How to use

1. Import the package sample `WebGLTemplates`.
2. Copy `WebGLTemplates/WebXRVPS` into your project's `Assets/WebGLTemplates/`.
3. Select `WebXRVPS` in `Project Settings -> Player -> WebGL -> Resolution and Presentation -> WebGL Template`.
4. Build a WebGL player with the `WebXR` target selected in the ARClip build window.

## Notes

- This template expects `unityInstance.Module.WebXR` to exist after Unity boots.
- WebXR drives the Unity camera itself; the template only provides camera API compatibility and VPS callbacks.
- VPS endpoint and location ids are expected to come from `ARLibController.SetupVPS(settings)`.
