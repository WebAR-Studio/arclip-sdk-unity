# ARClip SDK for Unity

The **ARClip SDK** by [WebAR Studio](https://web-ar.studio) enables fast and easy integration of WebAR into your Unity WebGL projects.

**AR Clip** is a powerful augmented reality launcher that enables instant access to immersive AR experiences — no full app installation required.

Designed for developers and creators, AR Clip lets you build interactive AR scenes using any modern web stack (like **Three.js**, **Babylon.js**, or your **custom framework**) and launch them using an **App Clip** or a **QR code scan**. The native part of the app leverages **ARKit** and our proprietary **AR Clip VPS engine** to provide high-performance AR, while the content itself is rendered in a **secure WebView** — giving you full flexibility and native power.

Perfect for:
- Geospatial AR
- Outdoor installations
- Guided AR tours
- Educational overlays
- Branded marketing experiences

## Features

- 📱 **App Clip–based AR launcher** — open via link or QR code  
- 🎯 **Full access to ARKit** (camera, motion tracking, VPS)  
- 📍 **Geo-anchored AR** with AR Clip VPS engine  
- 🌐 **Web-based content rendering** in secure WebView  
- 📷 **Built-in QR scanner** for launching custom AR projects  
- ⚡ **Instant AR launch experience** — no user friction  

---

**AR Clip bridges the gap between native AR performance and the flexibility of WebAR**, making it ideal for developers who want to deliver high-end AR content with zero install barrier.


## 📦 Installation

### Prerequisites

If you plan to build for the **WebXR** target, install **WebXR Export** first:

1. Open **Unity** and go to `Window → Package Manager`.
2. Click the **+** button and select **Add package from Git URL...**
3. Paste the following URL and click **Add**:

[https://github.com/De-Panther/unity-webxr-export.git?path=/Packages/webxr](https://github.com/De-Panther/unity-webxr-export.git?path=/Packages/webxr)

> Full instructions and documentation for WebXR Export are available at [https://github.com/De-Panther/unity-webxr-export](https://github.com/De-Panther/unity-webxr-export).

### ARClip SDK

After installing the prerequisites, add the ARClip SDK:

1. Open **Unity** and go to `Window → Package Manager`.
2. Click the **+** button and select **Add package from Git URL...**
3. Paste the following URL and click **Add**:

[https://github.com/WebAR-Studio/arclip-sdk-unity.git](https://github.com/WebAR-Studio/arclip-sdk-unity.git)

4. The library will be downloaded and added to your project.

### ⚠️ Important: Remove Existing `ARLib` Folder
Before proceeding, **delete any existing `ARLib` folder** under `Assets/`. Otherwise you may hit errors like:

```
error CS0433: The type 'ARLibTester' exists in both 'ARLib, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' and 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
```

### 🔧 Post-Installation Setup

**Step 1 — WebGLTemplates**
- In **Package Manager → ARClip → Samples**, import the **WebGLTemplates** sample.
- Copy the entire `WebGLTemplates` folder into your project’s `Assets/` root (Unity only lists templates placed there).
- Available templates in the sample:
  - `ARLib` for **ARClip App**
  - `8thWallVPS` for **8th Wall**
  - `WebXRVPS` for **WebXR**
- If you use `8thWallVPS`, you must also add the proprietary 8th Wall self-hosted engine files into `Assets/WebGLTemplates/8thWallVPS/vendor/8thwall/` as described in the template README.
- `WebXRVPS` expects the WebXR runtime/export package used by your WebXR build, because the template expects `unityInstance.Module.WebXR` to exist.

**Step 2 — TransparentBackground**
- Import the **TransparentBackground** sample (if it’s separate in the package).
- Move `TransparentBackground.jslib` to `Assets/Plugins` (root level) so the WebGL build can load it.

**Step 3 — Build Window**
- Open `ARClip/Build Window`.
- Choose the runtime target you want to build:
  - `ARClip App`
  - `8th Wall`
  - `WebXR`
- The build window is now the primary way to switch targets. It applies the correct WebGL template for the selected runtime before build.
- For `WebXR`, the window also checks prerequisites and can auto-configure:
  - OpenUPM scoped registry
  - `com.de-panther.webxr`
  - `WebXR Export` loader for `WebGL`
- When switching away from `WebXR`, the window disables the `WebXR Export` loader for `WebGL` so the project does not stay in XR mode accidentally.

---

## Table of Contents

1. [Requirements](#requirements)
2. [Installation](#installation)
3. [Quick Start](#quick-start)
4. [Testing Workflow](#testing-workflow)
5. [Build Targets](#build-targets)
6. [Events](#events)
7. [API Reference](#api-reference)
    - [Initialization & Core Control](#initialization--core-control)
    - [Surface (Plane) Tracking](#surface-plane-tracking)
    - [Image Tracking](#image-tracking)
    - [VPS (Visual Positioning System)](#vps-visual-positioning-system)
    - [Location & Heading](#location--heading)
8. [Editor vs Build Behavior](#editor-vs-build-behavior)
9. [QA / FAQ](#qa--faq)

---

## Requirements

- **Unity** 2020 LTS or newer (tested on 2021+).
- **Build Target**: WebGL (other platforms won’t call the JS side).
- For **8th Wall**, you must provide the self-hosted 8th Wall engine files inside `Assets/WebGLTemplates/8thWallVPS/vendor/8thwall/`.
- For **WebXR**, the project must include the WebXR export/runtime package (`com.de-panther.webxr`) and have `WebXR Export` enabled for `WebGL`.

> ❗ In the Unity **Editor** play mode, most native calls are skipped (`Application.isEditor` guard). You will not see real AR behavior until you **build**, **upload the WebGL build to our backend**, and **test it by scanning the QR code in our mobile app**. For quick testing inside the Editor, use **ARLibTester** — it simulates the native calls so you can verify your logic without a WebGL build.

---

## Rendering & Transparent Background

### Camera Component Disabled
- The GameObject that holds `renderCamera` must have its **Camera component disabled**. `ARLibController` invokes `renderCamera.Render()` manually each frame.

### Multi-Target Camera Setup
- The package now supports build-time runtime selection for `ARClip App`, `8th Wall`, and `WebXR`.
- `ARClip App` and `8th Wall` can share the same scene camera.
- `WebXR` should use its own dedicated camera rig/configuration.
- For multi-target scenes, use:
  - `ARClipCameraPlaceholder` on the camera anchor/object
  - `ARClipCameraBootstrap` on the object that owns `ARLibController`
- At build time, the selected runtime target is baked into the build, and `ARClipCameraBootstrap` chooses the corresponding camera for that target. It does not try to detect the runtime environment dynamically.

### Transparent WebGL Background
- Ensure `TransparentBackground.jslib` is present in the project so the WebGL canvas supports transparency.
- Configure `renderCamera` with **Solid Color** clear flags and **RGBA(0,0,0,0)** as the background color.

---

## Quick Start

### Project Setup

1. Create a camera GameObject, **disable its Camera component**, set **Clear Flags = Solid Color** and background **RGBA(0,0,0,0)**.
2. Add `ARLibController` to any GameObject and assign that camera to `renderCamera`.
3. *(Optional)* Add `ARLibTester` next to `ARLibController` to simulate native calls in the Unity Editor.
4. For WebGL transparency, export the **TransparentBackground** sample from the package and move `TransparentBackground.jslib` to the root `Assets/Plugins` folder.
5. If the project must build for multiple browser runtimes, replace the direct scene camera setup with `ARClipCameraPlaceholder` + `ARClipCameraBootstrap`.
6. Now you can use `ARLibController` from your scripts (e.g., `ARBootstrap`).

```csharp
using UnityEngine;
using ARLib;

public class ARBootstrap : MonoBehaviour
{
    void Awake()
    {
        // Subscribe to events
        ARLibController.Initialized += OnARReady;
        ARLibController.SurfaceTrackingUpdated += OnPlanesUpdated;
    }

    void Start()
    {
        // Initialize library
        ARLibController.Initialize();
    }

    private void OnARReady()
    {
        // Turn on camera + AR
        ARLibController.EnableCamera();
        ARLibController.EnableAR();

        // Start plane tracking ("horizontal", "vertical", or "both")
        ARLibController.EnableSurfaceTracking("horizontal");
    }

    private void OnPlanesUpdated(PlaneInfo[] planes)
    {
        Debug.Log($"Received {planes.Length} planes");
    }
}
```

### Typical Flow

1. Initialize the library (`Initialize()`).  
2. Enable camera/AR.  
3. Enable the tracking subsystems you need (planes, images, VPS).  
4. React to events (planes, images, VPS pose, GPS, heading).

---

## Testing Workflow

1. Import and copy the package `WebGLTemplates` sample into your project `Assets/` folder.
2. Open `ARClip/Build Window`.
3. Select the runtime target you want to build:
   - `ARClip App`
   - `8th Wall`
   - `WebXR`
4. If you selected:
   - `8th Wall`: make sure `Assets/WebGLTemplates/8thWallVPS/vendor/8thwall/` contains the self-hosted 8th Wall binaries.
   - `WebXR`: let the build window install/validate `com.de-panther.webxr` and `WebXR Export` for `WebGL`.
5. Build a WebGL player. The selected runtime target determines:
   - the WebGL template
   - the baked runtime config
   - which camera is selected by `ARClipCameraBootstrap`
6. For `ARClip App`:
   - zip the build with `index.html` at the archive root
   - upload it to https://cdn.mobile.web-ar.studio/clip/pages/zip_uploader.html
   - scan the generated QR code with our mobile app: https://apps.apple.com/app/ar-clip/id6742754238
7. For `8th Wall` and `WebXR`:
   - serve the build over HTTPS
   - open it in a supported mobile browser
   - if local HTTPS certificates or camera/video permissions cause issues during testing, exposing the local server through `ngrok` is usually enough, because it gives the build a public HTTPS URL that works better for browser camera access

If needed, you can still inspect the active template manually in **Project Settings → Player → WebGL → Resolution and Presentation → WebGL Template**, but the build window should be treated as the source of truth.

---

## Build Targets

The package now supports three WebGL runtime targets selected at build time:

| Target        | WebGL Template | Runtime host | Camera setup |
| ------------- | -------------- | ------------ | ------------ |
| `ARClip App`  | `ARLib`        | ARClip native app / WebView | Standard ARClip camera |
| `8th Wall`    | `8thWallVPS`   | Browser with 8th Wall | Same camera as `ARClip App` |
| `WebXR`       | `WebXRVPS`     | Browser with WebXR Export | Dedicated WebXR camera |

### Build-Time Selection

- Runtime selection is explicit and happens before build.
- The build embeds the selected target in a generated build config.
- `ARClipCameraBootstrap` reads that baked config and chooses the correct camera for the build.
- The package does not try to infer the host environment at runtime.

### WebXR Notes

- `WebXRVPS` now includes an ARClip-compatible JavaScript bridge:
  - `window.ARLib`
  - `window.ARLibNative`
  - VPS callbacks back into Unity (`OnVPSReady`, `OnVPSLocalized`, `OnVPSError`)
- WebXR owns the camera pose itself, so the bridge provides camera API compatibility but does not manually push camera pose updates into Unity.
- `SetupVPS(settings)` is used as the main source of VPS configuration in `WebXR`, including:
  - `serverUrl`
  - `locationsIds`
  - `maxFailsCount`
- If `XRRig` or related WebXR objects in the sample scenes show missing or broken components, first install the WebXR Export package (`com.de-panther.webxr`).
- If the rig is still broken after installing the package, recreate it from the Unity hierarchy with `XR -> Convert to XR Rig`, then reassign the new rig to `ARClipCameraPlaceholder.webXrRigRoot` if needed.

### 8th Wall Notes

- `8thWallVPS` includes the ARClip JavaScript bridge and supports common API, camera API, and VPS API.
- `ARClip App` and `8th Wall` are expected to share the same scene camera setup.
- Surface tracking and image tracking are not implemented by the `8th Wall` bridge yet.

---

## Events

All events are `static` so you can subscribe from anywhere. Unsubscribe in `OnDestroy` to avoid memory leaks.

| Event                      | Args                 | When it fires                                   |
| -------------------------- | -------------------- | ----------------------------------------------- |
| `Initialized`              | —                    | The AR library finished initialization.         |
| `SurfaceTrackingUpdated`   | `PlaneInfo[]`        | Surfaces (planes) were detected or updated.     |
| `ImageTrackingUpdated`     | `TrackedImageInfo[]` | Tracked images changed (added/removed/updated). |
| `TrackedImagesArrayUpdate` | `ImagesArrayData`    | The list of active tracking images changed.     |
| `VPSInitialized`           | —                    | VPS subsystem is ready.                         |
| `VPSPositionUpdated`       | `VPSPoseData`        | VPS returned a new global pose.                 |
| `OnVPSErrorHappened`       | `string`             | An error occurred in VPS.                       |
| `OnVPSSessionIdUpdated`    | `string`             | VPS session id was updated.                     |
| `LocationUpdated`          | `LocationData`       | GPS location update received.                   |
| `HeadingUpdated`           | `HeadingData`        | Compass heading update received.                |

**Subscribe / Unsubscribe Example**

```csharp
void OnEnable() {
    ARLibController.VPSPositionUpdated += HandleVpsPose;
}

void OnDisable() {
    ARLibController.VPSPositionUpdated -= HandleVpsPose;
}

void HandleVpsPose(VPSPoseData pose) {
    // Use pose.Position, pose.Rotation etc.
}
```

---

## API Reference

### Initialization & Core Control

```csharp
public static void Initialize();
public static void EnableAR();
public static void DisableAR();
public static void EnableCamera();
public static void DisableCamera();
public static void DisableTracking();
```

- **Initialize** once before any other call.  
- **EnableAR / DisableAR**: start/stop main AR loop.  
- **Enable/DisableCamera**: control camera feed separately if needed.  
- **DisableTracking**: turns off all tracking subsystems (planes, images, VPS).

---

### Surface (Plane) Tracking

```csharp
public static void EnableSurfaceTracking(string mode);
```

- `mode` can be `"horizontal"`, `"vertical"`, or `"both"`.  
- Plane updates arrive via `SurfaceTrackingUpdated`.

---

### Image Tracking

```csharp
public static void EnableImageTracking();
public static void AddTrackingImage(TrackingImageData data);
public static void RemoveTrackingImage(string name);
public static void RemoveAllTrackingImages();
```

**Usage pattern (must-load images before enabling tracking):**

1. Call `AddTrackingImage(data)` for **every** image you plan to track.  
2. Wait until `TrackedImagesArrayUpdate` fires and the `names` payload contains **all** the image names you added.  
3. Only then call `EnableImageTracking()` to start detection.

`TrackingImageData` fields:

```csharp
public class TrackingImageData
{
    public string name;          // Unique name used in callbacks and removal
    public string url;           // HTTP(S) URL of the image asset
    public float physicalWidth;  // Real-world width of the image in meters
}
```

---

### VPS (Visual Positioning System)

```csharp
public static void SetupVPS(VPSSettings settings);
public static void StartVPS();
public static void StopVPS();
public static void PauseVPS();
public static void ResumeVPS();
public static void ResetTracking();
public static void SetLocationIds();
```

**Tuning / Timing Helpers**

```csharp
public static void SetAnimationTime(float value);
public static void SetSendFastPhotoDelay(float value);
public static void SetSendPhotoDelay(float value);
public static void SetDistanceForInterp(float value);
public static void SetGpsAccuracyBarrier(float value);
public static void SetTimeOutDuration(float value);
public static void SetFirstRequestDelay(float value);
public static void SetAngleForInterp(float value);
```

- Listen to `VPSInitialized` to know when VPS is ready.  
- Localization results come via `VPSPositionUpdated`.  
- Errors show up in `OnVPSErrorHappened`.

---

### Location & Heading

```csharp
public static void GetCurrentPosition();
public static void WatchPosition();
public static void ClearWatch();

public static void StartHeadingUpdates();
public static void StopHeadingUpdates();
```

- One-off position request: `GetCurrentPosition()` → `LocationUpdated` fires once.  
- Continuous updates: `WatchPosition()` → call `ClearWatch()` to stop.  
- Heading works analogously with `StartHeadingUpdates()` / `StopHeadingUpdates()` and the `HeadingUpdated` event.

---

## Editor vs Build Behavior

Almost every native interop call is guarded by:

```csharp
if (!Application.isEditor)
{
    // call into WebGL/JS side
}
```

So in the Unity Editor you won’t see live AR, VPS, or GPS. Use mock data, conditional compilation, or the **ARLibTester** tool (included in the SDK) to simulate native calls for editor-time previews.

---

## QA / FAQ

**Q: I don't see the camera image in the background. Why?**

A: Check these points:

1. `TransparentBackground.jslib` is in the project (e.g. `Assets/Plugins/WebGL`).  
2. The `renderCamera` uses **Solid Color** clear flags.  
3. The camera background color is **RGBA(0,0,0,0)** (black, alpha = 0).  
4. The `Camera` component on the GameObject is **disabled** because rendering is triggered manually via `renderCamera.Render()`.  

---
