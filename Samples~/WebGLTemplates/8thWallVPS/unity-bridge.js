// Unity WebGL bridge — connects 8th Wall camera/VPS to Unity via arclip.min.js protocol

const stripBang = (s) => typeof s === 'string' && s.startsWith('!') ? s.slice(1) : s

let cameraSendingActive = false
let vpsControlCallbacks = null
let pendingCallbacks = []  // queued ARLibReceiver calls made before myGameInstance is ready
let hasSentInitialized = false
const trackingImageNames = new Set()

function callWhenReady(fn) {
  if (window.myGameInstance) {
    fn()
  } else {
    pendingCallbacks.push(fn)
  }
}

function flushPendingCallbacks() {
  const queued = pendingCallbacks.splice(0)
  for (const fn of queued) {
    try { fn() } catch (e) { console.error('[UnityBridge] deferred callback error', e) }
  }
}

function invokeReceiver(method, payload) {
  callWhenReady(() => {
    if (!window.ARLibReceiver || typeof window.ARLibReceiver[method] !== 'function') {
      return
    }

    if (payload === undefined) {
      window.ARLibReceiver[method]()
      return
    }

    window.ARLibReceiver[method](payload)
  })
}

function emitTrackedImagesArrayUpdate() {
  invokeReceiver('OnTrackedImagesArrayUpdate', JSON.stringify({
    names: Array.from(trackingImageNames),
  }))
}

function notifyInitialized() {
  if (hasSentInitialized) {
    return
  }

  hasSentInitialized = true
  invokeReceiver('OnInitialized')
  emitTrackedImagesArrayUpdate()
}

function parseTrackingImageName(data) {
  try {
    const payload = JSON.parse(stripBang(data))
    return typeof payload?.name === 'string' ? payload.name : null
  } catch (error) {
    console.warn('[UnityBridge] Failed to parse tracking image payload', error)
    return null
  }
}

// --- Unity Loader ---

export async function loadUnity() {
  const cfg = window.__UNITY_CONFIG
  if (!cfg) throw new Error('window.__UNITY_CONFIG not found')
  const loadingBar = document.getElementById('unity-loading-bar')
  const progressBarFull = document.getElementById('unity-progress-bar-full')

  const showLoading = () => {
    if (loadingBar) {
      loadingBar.style.display = 'block'
    }
  }

  const setProgress = (progress) => {
    if (progressBarFull) {
      progressBarFull.style.width = `${Math.max(0, Math.min(1, progress)) * 100}%`
    }
  }

  const hideLoading = () => {
    if (loadingBar) {
      loadingBar.style.display = 'none'
    }
  }

  showLoading()
  setProgress(0.05)

  // Dynamically load Unity loader script
  await new Promise((resolve, reject) => {
    const s = document.createElement('script')
    s.src = cfg.loaderUrl
    s.onload = resolve
    s.onerror = () => reject(new Error('Failed to load Unity loader'))
    document.body.appendChild(s)
  })

  const canvas = document.getElementById('unity-canvas')
  window.myGameInstance = null

  const instance = await createUnityInstance(canvas, {
    dataUrl: cfg.dataUrl,
    frameworkUrl: cfg.frameworkUrl,
    codeUrl: cfg.codeUrl,
    streamingAssetsUrl: cfg.streamingAssetsUrl,
    companyName: cfg.companyName,
    productName: cfg.productName,
    productVersion: cfg.productVersion,
  }, (progress) => {
    setProgress(progress)
  })

  window.myGameInstance = instance
  setProgress(1)
  hideLoading()
  console.log('[UnityBridge] Unity instance created')
  flushPendingCallbacks()
}

// --- Register VPS control callbacks from index.js ---

export function registerVpsControls(callbacks) {
  vpsControlCallbacks = callbacks
}

// --- Camera Pose Sender ---

export function startCameraSending() {
  cameraSendingActive = true
  console.log('[UnityBridge] Camera sending started')
}

export function stopCameraSending() {
  cameraSendingActive = false
  console.log('[UnityBridge] Camera sending stopped')
}

export function sendCameraPoseToUnity(camera) {
  if (!cameraSendingActive) return
  if (typeof window.writeCamera !== 'function') return

  // Three.js (RH Y-up) → Unity (LH Y-up): negate Z axis
  // Position: (x, y, z) → (x, y, -z)
  // Rotation: under Z-mirror, rotations around X,Y flip sign, around Z stays
  //   (rx, ry, rz) → (-rx, -ry, rz)
  const euler = new THREE.Euler().setFromQuaternion(camera.quaternion, 'YXZ')
  const RAD2DEG = 180 / Math.PI

  const data = [
    camera.position.x,
    camera.position.y,
    -camera.position.z,
    -euler.x * RAD2DEG,
    -euler.y * RAD2DEG,
    euler.z * RAD2DEG,
    ...camera.projectionMatrix.elements,
  ]

  window.writeCamera(data)
}

// --- VPS Result Forwarder ---

export function sendVpsResultToUnity(vpsPose, trackingPose, locationId) {
  if (typeof window.ARLibReceiver?.OnVPSLocalized !== 'function') return

  const response = {
    status: 'success',
    localisation: {
      trackingPosition: {x: trackingPose.x, y: trackingPose.y, z: trackingPose.z},
      trackingRotation: {x: trackingPose.rx, y: trackingPose.ry, z: trackingPose.rz},
      vpsPosition: {x: vpsPose.x, y: vpsPose.y, z: vpsPose.z},
      vpsRotation: {x: vpsPose.rx, y: vpsPose.ry, z: vpsPose.rz},
      gpsLatitude: 0,
      gpsLongitude: 0,
      heading: 0,
      accuracy: 0,
      timestamp: Date.now() / 1000,
      locationId,
    },
  }

  const json = JSON.stringify(response)
  callWhenReady(() => window.ARLibReceiver?.OnVPSLocalized(json))
  console.log('[UnityBridge] Sent VPS result to Unity', {locationId})
}

// --- ARLibNative (we define this; Unity/arclip calls it) ---

function installARLibNative() {
  window.ARLibNative = {
    // General
    Initialize() {
      console.log('[UnityBridge] ARLibNative.Initialize')
      notifyInitialized()
    },
    EnableAR() { /* no-op, 8th Wall already running */ },
    DisableAR() { /* no-op */ },
    Log(value) { console.log('[Unity]', value) },
    RequestRenderFrame() { /* no-op */ },

    // Camera
    EnableCamera() {
      console.log('[UnityBridge] ARLibNative.EnableCamera')
      startCameraSending()
    },
    DisableCamera() {
      console.log('[UnityBridge] ARLibNative.DisableCamera')
      stopCameraSending()
    },

    // VPS
    SetupVPS(settings) {
      console.log('[UnityBridge] ARLibNative.SetupVPS', stripBang(settings))
      vpsControlCallbacks?.handleSetupVPS(stripBang(settings))
      callWhenReady(() => window.ARLibReceiver?.OnVPSReady())
    },
    StartVPS() {
      console.log('[UnityBridge] ARLibNative.StartVPS')
      vpsControlCallbacks?.setVpsEnabled(true)
    },
    StopVPS() {
      console.log('[UnityBridge] ARLibNative.StopVPS')
      vpsControlCallbacks?.setVpsEnabled(false)
    },
    PauseVPS() {
      console.log('[UnityBridge] ARLibNative.PauseVPS')
      vpsControlCallbacks?.setVpsEnabled(false)
    },
    ResumeVPS() {
      console.log('[UnityBridge] ARLibNative.ResumeVPS')
      vpsControlCallbacks?.setVpsEnabled(true)
    },
    ResetTracking() {
      console.log('[UnityBridge] ARLibNative.ResetTracking')
      XR8.XrController.recenter()
    },
    SetLocationIds(json) {
      console.log('[UnityBridge] ARLibNative.SetLocationIds', stripBang(json))
      vpsControlCallbacks?.handleSetLocationIds(stripBang(json))
    },
    UpdateSessionId() {
      console.log('[UnityBridge] ARLibNative.UpdateSessionId')
      vpsControlCallbacks?.resetVpsSessionFromBridge()
    },
    SetAnimationTime() {},
    SetSendFastPhotoDelay() {},
    SetSendPhotoDelay() {},
    SetDistanceForInterp() {},
    SetGpsAccuracyBarrier() {},
    SetTimeOutDuration() {},
    SetFirstRequestDelay() {},
    SetAngleForInterp() {},

    // Stubs
    TakeScreenshot() {},
    DisableTracking() {},
    EnableSurfaceTracking() {},
    EnableImageTracking() {},
    AddTrackingImage(data) {
      const name = parseTrackingImageName(data)
      if (name) {
        trackingImageNames.add(name)
      }
      if (hasSentInitialized) {
        emitTrackedImagesArrayUpdate()
      }
    },
    RemoveTrackingImage(name) {
      if (typeof name === 'string' && name.length > 0) {
        trackingImageNames.delete(name)
      }
      if (hasSentInitialized) {
        emitTrackedImagesArrayUpdate()
      }
    },
    RemoveAllTrackingImages() {
      trackingImageNames.clear()
      if (hasSentInitialized) {
        emitTrackedImagesArrayUpdate()
      }
    },
    GetCurrentPosition() {},
    WatchPosition() {},
    ClearWatch() {},
    StartHeadingUpdates() {},
    StopHeadingUpdates() {},
  }

  console.log('[UnityBridge] ARLibNative installed')
}

// Install immediately so it's available when Unity boots
installARLibNative()
