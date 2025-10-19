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

To add the  SDK to your Unity project:

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
- In **Project Settings → Player → WebGL → Resolution and Presentation**, pick your copied template from the **WebGL Template** dropdown.

**Step 2 — TransparentBackground**
- Import the **TransparentBackground** sample (if it’s separate in the package).
- Move `TransparentBackground.jslib` to `Assets/Plugins` (root level) so the WebGL build can load it.

---

## Table of Contents

1. [Requirements](#requirements)
2. [Installation](#installation)
3. [Quick Start](#quick-start)
4. [Testing Workflow](#testing-workflow)
5. [Events](#events)
6. [API Reference](#api-reference)
    - [Initialization & Core Control](#initialization--core-control)
    - [Surface (Plane) Tracking](#surface-plane-tracking)
    - [Image Tracking](#image-tracking)
    - [VPS (Visual Positioning System)](#vps-visual-positioning-system)
    - [Location & Heading](#location--heading)
7. [Editor vs Build Behavior](#editor-vs-build-behavior)
8. [QA / FAQ](#qa--faq)

---

## Requirements

- **Unity** 2020 LTS or newer (tested on 2021+).
- **Build Target**: WebGL (other platforms won’t call the JS side).

> ❗ In the Unity **Editor** play mode, most native calls are skipped (`Application.isEditor` guard). You will not see real AR behavior until you **build**, **upload the WebGL build to our backend**, and **test it by scanning the QR code in our mobile app**. For quick testing inside the Editor, use **ARLibTester** — it simulates the native calls so you can verify your logic without a WebGL build.

---

## Rendering & Transparent Background

### Camera Component Disabled
- The GameObject that holds `renderCamera` must have its **Camera component disabled**. `ARLibController` invokes `renderCamera.Render()` manually each frame.

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
5. Now you can use `ARLibController` from your scripts (e.g., `ARBootstrap`).

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

1. **Export WebGL Template**: From the package **Samples**, export the provided WebGL template and copy it to your project root under `Assets` (Unity only lists templates placed there).
2. **Select Template in Player Settings**: Open **Project Settings → Player → WebGL → Resolution and Presentation → WebGL Template** and choose the exported template.
3. **Build the Project**: Create a WebGL build (File → Build Settings → WebGL → Build).
4. **Zip the Build**: Put the contents of the build folder into a `.zip`. **`index.html` must be at the root of the archive**.
5. **Upload to Backend**: Go to https://cdn.mobile.web-ar.studio/clip/pages/zip_uploader.html and upload the zip.
6. **Test on Mobile**: Scan the generated QR code with our mobile app: https://apps.apple.com/app/ar-clip/id6742754238

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
