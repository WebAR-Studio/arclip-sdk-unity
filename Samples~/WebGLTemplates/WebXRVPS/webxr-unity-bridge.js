(function () {
  'use strict';

  var pendingCallbacks = [];
  var hasSentInitialized = false;
  var trackingImageNames = new Set();

  function stripBang(value) {
    return typeof value === 'string' && value.charAt(0) === '!' ? value.slice(1) : value;
  }

  function callWhenReady(fn) {
    if (window.myGameInstance) {
      fn();
      return;
    }
    pendingCallbacks.push(fn);
  }

  function flushPendingCallbacks() {
    var queued = pendingCallbacks.splice(0);
    for (var i = 0; i < queued.length; i++) {
      try {
        queued[i]();
      } catch (error) {
        console.error('[WebXRBridge] deferred callback error', error);
      }
    }
  }

  function invokeReceiver(method, payload) {
    callWhenReady(function () {
      if (!window.ARLibReceiver || typeof window.ARLibReceiver[method] !== 'function') {
        return;
      }

      if (payload === undefined) {
        window.ARLibReceiver[method]();
      } else {
        window.ARLibReceiver[method](payload);
      }
    });
  }

  function emitTrackedImagesArrayUpdate() {
    invokeReceiver('OnTrackedImagesArrayUpdate', JSON.stringify({
      names: Array.from(trackingImageNames)
    }));
  }

  function notifyInitialized() {
    if (hasSentInitialized) {
      return;
    }

    hasSentInitialized = true;
    invokeReceiver('OnInitialized');
    emitTrackedImagesArrayUpdate();
  }

  function parseTrackingImageName(data) {
    try {
      var payload = JSON.parse(stripBang(data));
      return payload && typeof payload.name === 'string' ? payload.name : null;
    } catch (error) {
      console.warn('[WebXRBridge] Failed to parse tracking image payload', error);
      return null;
    }
  }

  function configureVpsBridge() {
    if (!window.VPSClient || typeof window.VPSClient.setBridgeCallbacks !== 'function') {
      return;
    }

    window.VPSClient.setBridgeCallbacks({
      onReady: function () { invokeReceiver('OnVPSReady'); },
      onLocalized: function (payload) { invokeReceiver('OnVPSLocalized', payload); },
      onError: function (message) { invokeReceiver('OnVPSError', message); }
    });
  }

  function getWebXRModule() {
    return window.myGameInstance &&
      window.myGameInstance.Module &&
      window.myGameInstance.Module.WebXR
      ? window.myGameInstance.Module.WebXR
      : null;
  }

  function toggleAr(expectRunning) {
    var webXR = getWebXRModule();
    if (!webXR || typeof webXR.toggleAR !== 'function') {
      invokeReceiver('OnVPSError', 'WebXR module is not ready.');
      return;
    }

    var isRunning = !!webXR.xrSession;
    if (isRunning === expectRunning) {
      return;
    }

    try {
      webXR.toggleAR();
    } catch (error) {
      console.error('[WebXRBridge] toggleAR failed', error);
      invokeReceiver('OnVPSError', error && error.message ? error.message : String(error));
    }
  }

  window.ARClipWebXRBridge = {
    onUnityInstanceReady: function () {
      flushPendingCallbacks();
      configureVpsBridge();
    }
  };

  window.ARLibNative = {
    Initialize: function () {
      console.log('[WebXRBridge] ARLibNative.Initialize');
      notifyInitialized();
    },
    EnableAR: function () {
      console.log('[WebXRBridge] ARLibNative.EnableAR');
      toggleAr(false);
    },
    DisableAR: function () {
      console.log('[WebXRBridge] ARLibNative.DisableAR');
      toggleAr(true);
    },
    Log: function (value) {
      console.log('[Unity]', value);
    },
    RequestRenderFrame: function () {},
    EnableCamera: function () {
      console.log('[WebXRBridge] ARLibNative.EnableCamera');
    },
    DisableCamera: function () {
      console.log('[WebXRBridge] ARLibNative.DisableCamera');
    },
    SetupVPS: function (settings) {
      console.log('[WebXRBridge] ARLibNative.SetupVPS', stripBang(settings));
      if (window.VPSClient && typeof window.VPSClient.configureFromUnity === 'function') {
        window.VPSClient.configureFromUnity(stripBang(settings));
      }
      invokeReceiver('OnVPSReady');
    },
    StartVPS: function () {
      if (window.VPSClient && typeof window.VPSClient.setEnabled === 'function') {
        window.VPSClient.setEnabled(true);
      }
    },
    StopVPS: function () {
      if (window.VPSClient && typeof window.VPSClient.setEnabled === 'function') {
        window.VPSClient.setEnabled(false);
      }
    },
    PauseVPS: function () {
      if (window.VPSClient && typeof window.VPSClient.setEnabled === 'function') {
        window.VPSClient.setEnabled(false);
      }
    },
    ResumeVPS: function () {
      if (window.VPSClient && typeof window.VPSClient.setEnabled === 'function') {
        window.VPSClient.setEnabled(true);
      }
    },
    ResetTracking: function () {
      if (window.VPSClient && typeof window.VPSClient.resetSession === 'function') {
        window.VPSClient.resetSession();
      }
    },
    SetLocationIds: function (json) {
      if (window.VPSClient && typeof window.VPSClient.setLocationIds === 'function') {
        window.VPSClient.setLocationIds(stripBang(json));
      }
    },
    SetAnimationTime: function () {},
    SetSendFastPhotoDelay: function (value) {
      if (window.VPSClient && typeof window.VPSClient.setSendFastPhotoDelay === 'function') {
        window.VPSClient.setSendFastPhotoDelay(value);
      }
    },
    SetSendPhotoDelay: function (value) {
      if (window.VPSClient && typeof window.VPSClient.setSendPhotoDelay === 'function') {
        window.VPSClient.setSendPhotoDelay(value);
      }
    },
    SetDistanceForInterp: function () {},
    SetGpsAccuracyBarrier: function () {},
    SetTimeOutDuration: function (value) {
      if (window.VPSClient && typeof window.VPSClient.setTimeoutDuration === 'function') {
        window.VPSClient.setTimeoutDuration(value);
      }
    },
    SetFirstRequestDelay: function (value) {
      if (window.VPSClient && typeof window.VPSClient.setFirstRequestDelay === 'function') {
        window.VPSClient.setFirstRequestDelay(value);
      }
    },
    SetAngleForInterp: function () {},
    TakeScreenshot: function () {},
    DisableTracking: function () {},
    EnableSurfaceTracking: function () {},
    EnableImageTracking: function () {},
    AddTrackingImage: function (data) {
      var name = parseTrackingImageName(data);
      if (name) {
        trackingImageNames.add(name);
      }
      if (hasSentInitialized) {
        emitTrackedImagesArrayUpdate();
      }
    },
    RemoveTrackingImage: function (name) {
      if (typeof name === 'string' && name.length > 0) {
        trackingImageNames.delete(name);
      }
      if (hasSentInitialized) {
        emitTrackedImagesArrayUpdate();
      }
    },
    RemoveAllTrackingImages: function () {
      trackingImageNames.clear();
      if (hasSentInitialized) {
        emitTrackedImagesArrayUpdate();
      }
    },
    GetCurrentPosition: function () {},
    WatchPosition: function () {},
    ClearWatch: function () {},
    StartHeadingUpdates: function () {},
    StopHeadingUpdates: function () {}
  };

  configureVpsBridge();
})();
