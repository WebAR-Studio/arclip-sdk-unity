(function () {
  'use strict';

  window.ARLib = window.ARLib || {};
  window.ARLibReceiver = window.ARLibReceiver || {};

  function log(tag, value) {
    console.log('[ARLib][' + tag + ']:', value);
  }

  function sendToUnity(method, payload) {
    if (!window.myGameInstance || typeof window.myGameInstance.SendMessage !== 'function') {
      return;
    }

    if (payload === undefined) {
      window.myGameInstance.SendMessage('ARLibController', method);
      return;
    }

    window.myGameInstance.SendMessage('ARLibController', method, payload);
  }

  window.ARLib.log = log;
  window.ARLib.Initialize = function () { window.ARLibNative.Initialize(); };
  window.ARLib.EnableAR = function () { window.ARLibNative.EnableAR(); };
  window.ARLib.DisableAR = function () { window.ARLibNative.DisableAR(); };
  window.ARLib.RequestRenderFrame = function () { window.ARLibNative.RequestRenderFrame(); };

  window.ARLib.camera = {
    EnableCamera: function () {
      log('Camera', 'Camera enabled.');
      window.ARLibNative.EnableCamera();
    },
    DisableCamera: function () {
      log('Camera', 'Camera disabled.');
      window.ARLibNative.DisableCamera();
    }
  };

  window.ARLib.tracking = {
    DisableTracking: function () {
      log('Tracking', 'Disable tracking.');
      window.ARLibNative.DisableTracking();
    },
    EnableSurfaceTracking: function (axis) {
      log('Tracking', 'Surface tracking enabled.');
      window.ARLibNative.EnableSurfaceTracking(axis);
    },
    EnableImageTracking: function () {
      log('Tracking', 'Image tracking enabled.');
      window.ARLibNative.EnableImageTracking();
    },
    AddTrackingImage: function (data) {
      log('Tracking', 'Add Tracking Image');
      window.ARLibNative.AddTrackingImage('!' + data);
    },
    RemoveTrackingImage: function (name) {
      log('Tracking', 'Remove Tracking Image');
      window.ARLibNative.RemoveTrackingImage(name);
    },
    RemoveAllTrackingImages: function () {
      log('Tracking', 'Remove All Tracking Images');
      window.ARLibNative.RemoveAllTrackingImages();
    }
  };

  window.ARLib.vps = {
    SetupVPS: function (settings) {
      log('VPS', 'Setup. ' + settings);
      window.ARLibNative.SetupVPS('!' + settings);
    },
    StartVPS: function () {
      log('VPS', 'Start.');
      window.ARLibNative.StartVPS();
    },
    StopVPS: function () {
      log('VPS', 'Stop.');
      window.ARLibNative.StopVPS();
    },
    PauseVPS: function () {
      log('VPS', 'Pause.');
      window.ARLibNative.PauseVPS();
    },
    ResumeVPS: function () {
      log('VPS', 'ResumeVPS.');
      window.ARLibNative.ResumeVPS();
    },
    ResetTracking: function () {
      log('VPS', 'Reset Tracking.');
      window.ARLibNative.ResetTracking();
    },
    SetLocationIds: function (data) {
      log('VPS', 'Set Location Ids: ' + data);
      window.ARLibNative.SetLocationIds(data ? '!' + data : data);
    },
    Log: function (value) {
      window.ARLibNative.Log('> ' + value);
    },
    SetAnimationTime: function (value) { window.ARLibNative.SetAnimationTime(value); },
    SetSendFastPhotoDelay: function (value) { window.ARLibNative.SetSendFastPhotoDelay(value); },
    SetSendPhotoDelay: function (value) { window.ARLibNative.SetSendPhotoDelay(value); },
    SetDistanceForInterp: function (value) { window.ARLibNative.SetDistanceForInterp(value); },
    SetGpsAccuracyBarrier: function (value) { window.ARLibNative.SetGpsAccuracyBarrier(value); },
    SetTimeOutDuration: function (value) { window.ARLibNative.SetTimeOutDuration(value); },
    SetFirstRequestDelay: function (value) { window.ARLibNative.SetFirstRequestDelay(value); },
    SetAngleForInterp: function (value) { window.ARLibNative.SetAngleForInterp(value); }
  };

  window.ARLib.location = {
    getCurrentPosition: function () { window.ARLibNative.GetCurrentPosition(); },
    watchPosition: function () { window.ARLibNative.WatchPosition(); },
    clearWatch: function () { window.ARLibNative.ClearWatch(); },
    startHeadingUpdates: function () { window.ARLibNative.StartHeadingUpdates(); },
    stopHeadingUpdates: function () { window.ARLibNative.StopHeadingUpdates(); }
  };

  window.ARLibReceiver.OnInitialized = function () { sendToUnity('OnInitialized'); };
  window.ARLibReceiver.OnCameraPoseUpdate = function (payload) { sendToUnity('OnCameraPoseUpdate', payload); };
  window.ARLibReceiver.OnSurfaceTrackingUpdate = function (payload) { sendToUnity('OnSurfaceTrackingUpdate', payload); };
  window.ARLibReceiver.OnImageTrackingUpdate = function (payload) { sendToUnity('OnImageTrackingUpdate', payload); };
  window.ARLibReceiver.OnTrackedImagesArrayUpdate = function (payload) { sendToUnity('OnTrackedImagesArrayUpdate', payload); };
  window.ARLibReceiver.OnVPSReady = function () { sendToUnity('OnVPSReady'); };
  window.ARLibReceiver.OnVPSLocalized = function (payload) { sendToUnity('OnVPSLocalized', payload); };
  window.ARLibReceiver.OnVPSError = function (payload) { sendToUnity('OnVPSError', payload); };
  window.ARLibReceiver.OnUpdateSessionId = function (payload) { sendToUnity('OnUpdateSessionId', payload); };
  window.ARLibReceiver.OnUpdateLocation = function (payload) { sendToUnity('OnUpdateLocation', payload); };
  window.ARLibReceiver.OnUpdateHeading = function (payload) { sendToUnity('OnUpdateHeading', payload); };
  window.ARLibReceiver.OnLocationFailed = function (payload) { sendToUnity('OnLocationFailed', payload); };

  window.writeCamera = window.writeCamera || function () {};
  window.writePlanes = window.writePlanes || function () {};
})();
