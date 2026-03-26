import {
  loadUnity,
  registerVpsControls,
  sendCameraPoseToUnity,
  sendVpsResultToUnity,
} from './unity-bridge.js'

// --- VPS API helpers (inlined) ---

const fetchWithTimeout = async (url, options, timeoutMs = 15000) => {
  const controller = new AbortController()
  const timeoutId = setTimeout(() => controller.abort(), timeoutMs)

  try {
    return await fetch(url, {
      ...options,
      signal: controller.signal,
    })
  } finally {
    clearTimeout(timeoutId)
  }
}

const sendToVps = async (formData, url, timeoutMs = 15000) => {
  const response = await fetchWithTimeout(url, {
    method: 'POST',
    body: formData,
    headers: {Accept: 'application/json'},
  }, timeoutMs)

  const json = await response.json()

  if (!response.ok) {
    const detail = json?.detail
    const message = Array.isArray(detail)
      ? detail.map(item => item.msg).join(', ')
      : json?.message || `VPS request failed with status ${response.status}`
    throw new Error(message)
  }

  return json
}

const constructVpsRequestData = (
  photo, photoWidth, photoHeight, intrinsics,
  locationsIds, sessionId, trackerPos, options = {}
) => {
  const {
    clientCoordinateSystem = 'threejs',
    timestamp = Date.now(),
    userId,
  } = options

  const requestJson = {
    attributes: {
      location_ids: locationsIds,
      session_id: sessionId,
      timestamp,
      client_coordinate_system: clientCoordinateSystem,
      tracking_pose: trackerPos || {x: 0, y: 0, z: 0, rx: 0, ry: 0, rz: 0},
      intrinsics,
    },
  }

  if (userId) {
    requestJson.attributes.user_id = userId
  }

  const formData = new FormData()
  formData.append('image', photo)
  formData.append('json', JSON.stringify({data: requestJson}))

  return formData
}

// --- Camera frame cropping ---

const MAX_IMAGE_WIDTH = 540
const MAX_IMAGE_HEIGHT = 960
const cropCanvas = document.createElement('canvas')

const cropCameraImage = async (rows, cols, pixels, options = {}) => {
  const {
    greyScale = false,
    mimeType = 'image/jpeg',
    quality = 0.82,
  } = options
  const croppedWidth = cols > MAX_IMAGE_WIDTH ? MAX_IMAGE_WIDTH : cols
  const croppedHeight = rows > MAX_IMAGE_HEIGHT ? MAX_IMAGE_HEIGHT : rows

  const cropDx = Math.floor((cols - croppedWidth) / 2)
  const cropDy = Math.floor((rows - croppedHeight) / 2)

  const newPixels = new Uint8ClampedArray(croppedHeight * croppedWidth * 4)

  for (let iy = 0; iy < croppedHeight; iy++) {
    let average = 0
    for (let ix = 0; ix < croppedWidth * 4; ix++) {
      if (greyScale) {
        if (ix % 4 === 3) {
          average /= 3
          newPixels[iy * croppedWidth * 4 + ix - 3] = average
          newPixels[iy * croppedWidth * 4 + ix - 2] = average
          newPixels[iy * croppedWidth * 4 + ix - 1] = average
          newPixels[iy * croppedWidth * 4 + ix] = pixels[(iy + cropDy) * cols * 4 + cropDx * 4 + ix]
          average = 0
        } else {
          average += pixels[(iy + cropDy) * cols * 4 + cropDx * 4 + ix]
        }
      } else {
        newPixels[iy * croppedWidth * 4 + ix] = pixels[(iy + cropDy) * cols * 4 + cropDx * 4 + ix]
      }
    }
  }

  cropCanvas.width = croppedWidth
  cropCanvas.height = croppedHeight

  const ctx = cropCanvas.getContext('2d')
  ctx.putImageData(new ImageData(newPixels, croppedWidth, croppedHeight), 0, 0)

  const photo = await new Promise((resolve) => {
    cropCanvas.toBlob((blob) => resolve(blob), mimeType, quality)
  })

  return {photo, croppedWidth, croppedHeight}
}

// --- Constants ---

const FALLBACK_FXFY = 722
const VPS_REQUEST_INTERVAL_MS = 3000
const VPS_REQUEST_TIMEOUT_MS = 15000
const XR_START_TIMEOUT_MS = 8000
const VPS_URL = 'https://was-vps.web-ar.xyz/vps/api/v3'

const canvas = document.getElementById('camerafeed')
const xrEngineScript = document.getElementById('xr-engine-script')

let hasBooted = false
let sceneInitialized = false
let unityLoadStarted = false

const availableLocations = [
  {
    location_id: 'mariel',
    name: 'Mariel',
  },
]

const vpsState = {
  enabled: false,
  requestInProgress: false,
  lastRequestAt: 0,
  sessionId: crypto.randomUUID(),
  userId: localStorage.getItem('vps-user-id') || crypto.randomUUID(),
  hasLocalization: false,
  currentLocation: availableLocations[0],
  serverUrl: VPS_URL,
  maxFailsCount: 0,
}

if (!localStorage.getItem('vps-user-id')) {
  localStorage.setItem('vps-user-id', vpsState.userId)
}

const log = (...args) => {
  console.log('[VPS]', ...args)
}

const degrees = (radians) => radians * 180 / Math.PI

const quaternionToEulerYXZDegrees = (quaternion) => {
  const euler = new THREE.Euler().setFromQuaternion(quaternion, 'YXZ')
  return {
    rx: degrees(euler.x),
    ry: degrees(euler.y),
    rz: degrees(euler.z),
  }
}

const createFullWindowCanvasModule = () => {
  let videoWidth = 0
  let videoHeight = 0

  const fillScreenWithCanvas = () => {
    if (!videoWidth || !videoHeight) {
      Object.assign(canvas.style, {
        width: '100%',
        height: '100%',
        display: 'block',
        margin: '0',
        padding: '0',
        border: '0',
      })
      return
    }

    const devicePixelRatio = window.devicePixelRatio || 1
    const windowWidth = Math.round(window.innerWidth * devicePixelRatio)
    const windowHeight = Math.round(window.innerHeight * devicePixelRatio)

    const portraitWindowHeight = Math.max(windowWidth, windowHeight)
    const portraitWindowWidth = Math.min(windowWidth, windowHeight)
    const portraitWindowAspect = portraitWindowHeight / portraitWindowWidth

    const portraitVideoHeight = Math.max(videoWidth, videoHeight)
    const portraitVideoWidth = Math.min(videoWidth, videoHeight)

    let croppedHeight = portraitVideoHeight
    let croppedWidth = Math.round(portraitVideoHeight / portraitWindowAspect)

    if (croppedWidth > portraitVideoWidth) {
      croppedWidth = portraitVideoWidth
      croppedHeight = Math.round(portraitVideoWidth * portraitWindowAspect)
    }

    if (croppedWidth > portraitWindowWidth || croppedHeight > portraitWindowHeight) {
      croppedWidth = portraitWindowWidth
      croppedHeight = portraitWindowHeight
    }

    if (windowWidth > windowHeight) {
      const previousWidth = croppedWidth
      croppedWidth = croppedHeight
      croppedHeight = previousWidth
    }

    Object.assign(canvas.style, {
      width: '100%',
      height: '100%',
      display: 'block',
      margin: '0',
      padding: '0',
      border: '0',
    })

    canvas.width = croppedWidth
    canvas.height = croppedHeight
  }

  return {
    name: 'local-fullwindow-canvas',
    onAttach: ({videoWidth: attachedVideoWidth, videoHeight: attachedVideoHeight}) => {
      videoWidth = attachedVideoWidth || videoWidth
      videoHeight = attachedVideoHeight || videoHeight
      fillScreenWithCanvas()
    },
    onCameraStatusChange: ({status, video}) => {
      if (status === 'hasVideo' && video) {
        videoWidth = video.videoWidth
        videoHeight = video.videoHeight
        fillScreenWithCanvas()
      }
    },
    onVideoSizeChange: ({videoWidth: nextVideoWidth, videoHeight: nextVideoHeight}) => {
      videoWidth = nextVideoWidth
      videoHeight = nextVideoHeight
      fillScreenWithCanvas()
    },
    onCanvasSizeChange: () => {
      fillScreenWithCanvas()
    },
    onDeviceOrientationChange: () => {
      fillScreenWithCanvas()
    },
    onUpdate: () => {
      fillScreenWithCanvas()
    },
    onDetach: () => {
      videoWidth = 0
      videoHeight = 0
    },
  }
}

const createSceneContentPipelineModule = () => ({
  name: 'scene-content-module',
  onStart: () => {
    if (sceneInitialized) return
    sceneInitialized = true

    const {scene, camera} = XR8.Threejs.xrScene()
    scene.background = null

    camera.position.set(0, 1.6, 0)
    XR8.XrController.updateCameraProjectionMatrix({
      origin: camera.position,
      facing: camera.quaternion,
    })

    log('Scene initialized')
    scheduleUnityLoad()
  },
  onUpdate: () => {
    const {camera} = XR8.Threejs.xrScene()
    sendCameraPoseToUnity(camera)
  },
})

function scheduleUnityLoad() {
  if (unityLoadStarted) {
    return
  }

  unityLoadStarted = true

  window.setTimeout(async () => {
    try {
      log('Loading Unity WebGL...')
      await loadUnity()
      log('Unity loaded successfully')
    } catch (error) {
      unityLoadStarted = false
      console.error(error)
      log('Unity load failed', {message: error.message})
    }
  }, 0)
}

const getFocalLength = () => {
  const intrinsic = XR8.XrController.getIntrinsic?.()
  return {
    fx: intrinsic?.fx || intrinsic?.fy || FALLBACK_FXFY,
    fy: intrinsic?.fy || intrinsic?.fx || FALLBACK_FXFY,
  }
}

const createTrackingPoseSnapshot = (camera) => {
  const euler = quaternionToEulerYXZDegrees(camera.quaternion)

  return {
    x: camera.position.x,
    y: camera.position.y,
    z: camera.position.z,
    rx: euler.rx,
    ry: euler.ry,
    rz: euler.rz,
    qx: camera.quaternion.x,
    qy: camera.quaternion.y,
    qz: camera.quaternion.z,
    qw: camera.quaternion.w,
  }
}

const handleVpsPose = ({response, trackingPose}) => {
  const vpsPose = response?.data?.attributes?.vps_pose
  if (!vpsPose) {
    log('VPS response missing pose', response)
    return
  }

  sendVpsResultToUnity(vpsPose, trackingPose, vpsState.currentLocation.location_id)

  log('VPS pose forwarded to Unity', {
    locationId: vpsState.currentLocation.location_id,
    vpsPose,
    trackingPose,
  })
}

const createVpsPipelineModule = () => ({
  name: 'vps-module',
  onProcessCpu: async ({processGpuResult}) => {
    if (!vpsState.enabled || vpsState.requestInProgress) {
      return
    }

    if (Date.now() - vpsState.lastRequestAt < VPS_REQUEST_INTERVAL_MS) {
      return
    }

    const camerapixelarray = processGpuResult?.camerapixelarray
    if (!camerapixelarray?.pixels) {
      return
    }

    const {camera} = XR8.Threejs.xrScene()
    const locationId = vpsState.currentLocation?.location_id
    if (!locationId) {
      log('Skipping VPS request because no location is selected')
      return
    }

    vpsState.requestInProgress = true
    vpsState.lastRequestAt = Date.now()
    log('VPS cycle started', {locationId, sessionId: vpsState.sessionId})

    const {rows, cols, pixels} = camerapixelarray
    const trackingPose = createTrackingPoseSnapshot(camera)

    try {
      const {photo, croppedWidth, croppedHeight} = await cropCameraImage(rows, cols, pixels, {
        greyScale: true,
        mimeType: 'image/jpeg',
        quality: 0.82,
      })
      if (!photo) {
        throw new Error('Failed to encode camera frame.')
      }

      log('Frame encoded', {croppedWidth, croppedHeight, blobSize: photo.size})

      const focal = getFocalLength()
      const intrinsics = {
        width: croppedWidth,
        height: croppedHeight,
        fx: focal.fx,
        fy: focal.fy,
        cx: croppedWidth / 2,
        cy: croppedHeight / 2,
      }

      const formData = constructVpsRequestData(
        photo,
        croppedWidth,
        croppedHeight,
        intrinsics,
        [locationId],
        vpsState.sessionId,
        {
          x: trackingPose.x,
          y: trackingPose.y,
          z: trackingPose.z,
          rx: trackingPose.rx,
          ry: trackingPose.ry,
          rz: trackingPose.rz,
        },
        {
          clientCoordinateSystem: 'threejs',
          timestamp: Date.now() / 1000,
          userId: vpsState.userId,
        }
      )

      const response = await sendToVps(formData, vpsState.serverUrl, VPS_REQUEST_TIMEOUT_MS)
      log('Received VPS response', response)

      if (response?.data?.status === 'done') {
        handleVpsPose({response, trackingPose})
      } else {
        const serverStatus = response?.data?.status || 'unknown'
        log('VPS completed without localization', serverStatus)
      }
    } catch (error) {
      console.error(error)
      log('VPS request failed', {name: error.name, message: error.message})
    } finally {
      vpsState.requestInProgress = false
    }
  },
})

// --- VPS control functions exported for Unity bridge ---

function setVpsEnabled(enabled) {
  vpsState.enabled = enabled
  vpsState.lastRequestAt = 0
  if (enabled) {
    resetVpsSession()
  }
  log('VPS enabled:', enabled)
}

function resetVpsSession() {
  vpsState.sessionId = crypto.randomUUID()
  vpsState.hasLocalization = false
}

function resetVpsSessionFromBridge() {
  resetVpsSession()
  log('VPS session reset by Unity')
}

function handleSetupVPS(settingsJson) {
  try {
    const settings = JSON.parse(settingsJson)
    log('SetupVPS settings from Unity', settings)

    if (settings.serverUrl) {
      vpsState.serverUrl = settings.serverUrl
      log('VPS server URL set to', settings.serverUrl)
    }

    if (settings.locationsIds?.length > 0) {
      applyLocationIds(settings.locationsIds)
    }

    if (settings.maxFailsCount != null) {
      vpsState.maxFailsCount = settings.maxFailsCount
    }
  } catch {
    log('SetupVPS called with non-JSON settings', settingsJson)
  }
}

function applyLocationIds(ids) {
  if (!ids || ids.length === 0) return

  const locationId = ids[0]

  let location = availableLocations.find((loc) => loc.location_id === locationId)
  if (!location) {
    location = {location_id: locationId, name: locationId}
    availableLocations.push(location)
    log('Added dynamic location from Unity', locationId)
  }

  vpsState.currentLocation = location
  resetVpsSession()
  log('Active location set to', locationId)
}

function handleSetLocationIds(json) {
  try {
    const data = JSON.parse(json)
    const ids = data.locationsIds || data.locationIds || data.location_ids || (Array.isArray(data) ? data : [])
    applyLocationIds(ids)
  } catch {
    log('SetLocationIds called with non-JSON data', json)
  }
}

registerVpsControls({
  setVpsEnabled,
  resetVpsSessionFromBridge,
  handleSetupVPS,
  handleSetLocationIds,
})

// --- Init ---

log('App initialized')

const onXrLoaded = () => {
  log('XR loaded, registering pipeline modules')
  XR8.addCameraPipelineModules([
    createFullWindowCanvasModule(),
    XR8.CameraPixelArray.pipelineModule({maxDimension: 960}),
    XR8.GlTextureRenderer.pipelineModule(),
    XR8.Threejs.pipelineModule(),
    XR8.XrController.pipelineModule(),
  ])

  XR8.addCameraPipelineModule(createSceneContentPipelineModule())
  XR8.addCameraPipelineModule(createVpsPipelineModule())
  log('Starting XR run loop')
  XR8.run({canvas})
}

const boot = async () => {
  if (hasBooted) return
  hasBooted = true

  try {
    log('Boot started, loading SLAM chunk')
    await XR8.loadChunk('slam')
    log('SLAM chunk loaded')
    onXrLoaded()
  } catch (error) {
    console.error(error)
    log('XR boot failed', {message: error.message})
    return
  }
}

xrEngineScript?.addEventListener('error', () => {
  log('Failed to load local xr.js binary')
})

window.addEventListener('error', (event) => {
  log('Window error', event.message)
})

window.addEventListener('unhandledrejection', (event) => {
  const message = event.reason?.message || String(event.reason)
  log('Unhandled rejection', message)
})

window.setTimeout(() => {
  if (!window.XR8 && !hasBooted) {
    log('XR startup timeout elapsed before xrloaded event')
  }
}, XR_START_TIMEOUT_MS)

if (window.XR8) {
  log('XR8 already present on window, booting immediately')
  boot()
} else {
  log('Waiting for xrloaded event')
  window.addEventListener('xrloaded', boot, {once: true})
}
