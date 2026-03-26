/**
 * VPS Client for WebXR — JS port of naviar/vps_sdk (Photo mode only)
 * Uses WebXR Camera Access API (Chrome Android 103+)
 * Works alongside Unity WebXR build without C# changes
 */
(function () {
  'use strict';

  // ===== Section 1: Config & State =====

  var CFG = Object.assign({
    vpsUrl: 'https://was-vps.web-ar.xyz',
    locationIds: ['mariel'],
    captureInterval: 2500,
    jpegQuality: 1.0,
    targetWidth: 540,
    targetHeight: 960
  }, window.VPS_CONFIG || {});

  var state = {
    sessionId: null,
    userId: null,
    xrSession: null,
    glBinding: null,
    glCtx: null,
    isRunning: false,
    pendingRequest: false,
    lastCaptureTime: 0,
    // GL resources
    fbo: null,
    program: null,
    posLoc: -1,
    texLoc: null,
    vao: null,
    quadBuf: null,
    offscreenCanvas: null,
    offscreenCtx: null,
    unityInstance: null,
    rafId: null,
    bridgeCallbacks: null,
    firstRequestDelay: 0,
    startedAt: 0,
    timeoutDurationMs: 20000,
    sendFastPhotoDelay: null,
    sendPhotoDelay: null,
    maxFailsCount: 5
  };

  function normalizeApiUrl(url) {
    if (!url) return 'https://was-vps.web-ar.xyz/vps/api/v3';
    return /\/vps\/api\/v3\/?$/.test(url) ? url.replace(/\/$/, '') : url.replace(/\/$/, '') + '/vps/api/v3';
  }

  CFG.vpsApiUrl = normalizeApiUrl(CFG.vpsApiUrl || CFG.vpsUrl);

  function generateUUID() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
      var r = Math.random() * 16 | 0;
      return (c === 'x' ? r : (r & 0x3 | 0x8)).toString(16);
    });
  }

  state.sessionId = generateUUID();
  state.userId = localStorage.getItem('vps_user_id');
  if (!state.userId) {
    state.userId = generateUUID();
    localStorage.setItem('vps_user_id', state.userId);
  }

  function log() {
    var args = ['[VPS]'].concat(Array.prototype.slice.call(arguments));
    console.log.apply(console, args);
  }
  function warn() {
    var args = ['[VPS]'].concat(Array.prototype.slice.call(arguments));
    console.warn.apply(console, args);
  }

  function notifyBridge(name, payload) {
    var callbacks = state.bridgeCallbacks;
    if (!callbacks || typeof callbacks[name] !== 'function') return;
    try {
      callbacks[name](payload);
    } catch (error) {
      warn('bridge callback failed:', name, error && error.message ? error.message : error);
    }
  }

  function normalizeLocationIds(input) {
    if (!input) return [];
    if (Array.isArray(input)) return input;
    if (typeof input === 'string') {
      try {
        return normalizeLocationIds(JSON.parse(input));
      } catch (_) {
        return [input];
      }
    }

    return input.locationsIds || input.locationIds || input.location_ids || [];
  }

  function applyConfigPatch(patch) {
    if (!patch || typeof patch !== 'object') return;

    if (patch.serverUrl || patch.vpsUrl || patch.vpsApiUrl) {
      CFG.vpsApiUrl = normalizeApiUrl(patch.serverUrl || patch.vpsApiUrl || patch.vpsUrl);
    }

    var locationIds = normalizeLocationIds(patch.locationsIds || patch.locationIds || patch.location_ids);
    if (locationIds.length > 0) {
      CFG.locationIds = locationIds;
    }

    if (patch.captureInterval != null) CFG.captureInterval = patch.captureInterval;
    if (patch.jpegQuality != null) CFG.jpegQuality = patch.jpegQuality;
    if (patch.targetWidth != null) CFG.targetWidth = patch.targetWidth;
    if (patch.targetHeight != null) CFG.targetHeight = patch.targetHeight;
    if (patch.maxFailsCount != null) state.maxFailsCount = patch.maxFailsCount;
  }

  function buildLocalizationPayload(vpsPose, trackingPose, locationId) {
    return JSON.stringify({
      status: 'success',
      localisation: {
        trackingPosition: { x: trackingPose.x, y: trackingPose.y, z: trackingPose.z },
        trackingRotation: { x: trackingPose.rx, y: trackingPose.ry, z: trackingPose.rz },
        vpsPosition: { x: vpsPose.x, y: vpsPose.y, z: vpsPose.z },
        vpsRotation: { x: vpsPose.rx, y: vpsPose.ry, z: vpsPose.rz },
        gpsLatitude: 0,
        gpsLongitude: 0,
        heading: 0,
        accuracy: 0,
        timestamp: Date.now() / 1000,
        locationId: locationId || CFG.locationIds[0] || ''
      }
    });
  }

  function ensureOverlay() {}

  function hudPush() {}

  var lastBlob = null;
  var lastPayload = null;

  function hudShowPreview(blob) {
    lastBlob = blob;
  }

  // ===== Section 2: Session Hooks — monkey-patch requestSession =====

  if (navigator.xr && navigator.xr.requestSession) {
    var _origRequestSession = navigator.xr.requestSession.bind(navigator.xr);
    navigator.xr.requestSession = function (mode, options) {
      if (mode === 'immersive-ar') {
        options = options || {};
        options.optionalFeatures = options.optionalFeatures || [];
        if (options.optionalFeatures.indexOf('camera-access') === -1) {
          options.optionalFeatures.push('camera-access');
        }
        log('injected camera-access into session features');
      }
      return _origRequestSession(mode, options);
    };
    log('hooks installed');
  } else {
    warn('navigator.xr.requestSession not available — VPS disabled');
  }

  // ===== Section 3: GL Resources =====

  var VS_SRC =
    '#version 300 es\n' +
    'in vec4 a_pos;\n' +
    'out vec2 v_uv;\n' +
    'void main() {\n' +
    '  gl_Position = a_pos;\n' +
    '  v_uv = a_pos.xy * 0.5 + 0.5;\n' +
    '}';

  var FS_SRC =
    '#version 300 es\n' +
    'precision mediump float;\n' +
    'uniform sampler2D u_tex;\n' +
    'in vec2 v_uv;\n' +
    'out vec4 fragColor;\n' +
    'void main() {\n' +
    '  fragColor = texture(u_tex, v_uv);\n' +
    '}';

  function compileShader(gl, type, src) {
    var s = gl.createShader(type);
    gl.shaderSource(s, src);
    gl.compileShader(s);
    if (!gl.getShaderParameter(s, gl.COMPILE_STATUS)) {
      warn('shader compile:', gl.getShaderInfoLog(s));
      gl.deleteShader(s);
      return null;
    }
    return s;
  }

  function initGLResources(gl) {
    var vs = compileShader(gl, gl.VERTEX_SHADER, VS_SRC);
    var fs = compileShader(gl, gl.FRAGMENT_SHADER, FS_SRC);
    if (!vs || !fs) return false;

    var prog = gl.createProgram();
    gl.attachShader(prog, vs);
    gl.attachShader(prog, fs);
    gl.linkProgram(prog);
    gl.deleteShader(vs);
    gl.deleteShader(fs);

    if (!gl.getProgramParameter(prog, gl.LINK_STATUS)) {
      warn('program link:', gl.getProgramInfoLog(prog));
      gl.deleteProgram(prog);
      return false;
    }

    state.program = prog;
    state.posLoc = gl.getAttribLocation(prog, 'a_pos');
    state.texLoc = gl.getUniformLocation(prog, 'u_tex');

    // Fullscreen quad VAO
    var vao = gl.createVertexArray();
    gl.bindVertexArray(vao);
    var buf = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER, buf);
    gl.bufferData(gl.ARRAY_BUFFER, new Float32Array([
      -1, -1, 1, -1, -1, 1, 1, 1
    ]), gl.STATIC_DRAW);
    gl.enableVertexAttribArray(state.posLoc);
    gl.vertexAttribPointer(state.posLoc, 2, gl.FLOAT, false, 0, 0);
    gl.bindVertexArray(null);
    gl.bindBuffer(gl.ARRAY_BUFFER, null);
    state.vao = vao;
    state.quadBuf = buf;

    // FBO for camera readback
    state.fbo = gl.createFramebuffer();

    // Offscreen 2D canvas for JPEG encoding
    state.offscreenCanvas = document.createElement('canvas');
    state.offscreenCtx = state.offscreenCanvas.getContext('2d');

    log('GL resources initialized');
    return true;
  }

  function cleanupGLResources() {
    var gl = state.glCtx;
    if (!gl) return;
    if (state.fbo) { gl.deleteFramebuffer(state.fbo); state.fbo = null; }
    if (state.program) { gl.deleteProgram(state.program); state.program = null; }
    if (state.vao) { gl.deleteVertexArray(state.vao); state.vao = null; }
    if (state.quadBuf) { gl.deleteBuffer(state.quadBuf); state.quadBuf = null; }
    state.offscreenCanvas = null;
    state.offscreenCtx = null;
  }

  // ===== Section 4: Camera Frame Capture =====

  /** Save all GL state that our blit touches, so Unity render is unaffected. */
  function saveGLState(gl) {
    return {
      fbo: gl.getParameter(gl.FRAMEBUFFER_BINDING),
      viewport: gl.getParameter(gl.VIEWPORT),
      program: gl.getParameter(gl.CURRENT_PROGRAM),
      activeTex: gl.getParameter(gl.ACTIVE_TEXTURE),
      tex2D: gl.getParameter(gl.TEXTURE_BINDING_2D),
      vao: gl.getParameter(gl.VERTEX_ARRAY_BINDING),
      arrayBuf: gl.getParameter(gl.ARRAY_BUFFER_BINDING),
      blend: gl.isEnabled(gl.BLEND),
      depthTest: gl.isEnabled(gl.DEPTH_TEST),
      cullFace: gl.isEnabled(gl.CULL_FACE),
      scissorTest: gl.isEnabled(gl.SCISSOR_TEST),
      stencilTest: gl.isEnabled(gl.STENCIL_TEST),
      colorMask: gl.getParameter(gl.COLOR_WRITEMASK),
      depthMask: gl.getParameter(gl.DEPTH_WRITEMASK)
    };
  }

  function restoreGLState(gl, s) {
    // Use oldBindFramebuffer to bypass Unity's null-FBO redirect
    var bindFB = gl.oldBindFramebuffer || gl.bindFramebuffer;
    bindFB.call(gl, gl.FRAMEBUFFER, s.fbo);
    gl.viewport(s.viewport[0], s.viewport[1], s.viewport[2], s.viewport[3]);
    gl.useProgram(s.program);
    gl.activeTexture(s.activeTex);
    gl.bindTexture(gl.TEXTURE_2D, s.tex2D);
    gl.bindVertexArray(s.vao);
    gl.bindBuffer(gl.ARRAY_BUFFER, s.arrayBuf);
    if (s.blend) gl.enable(gl.BLEND); else gl.disable(gl.BLEND);
    if (s.depthTest) gl.enable(gl.DEPTH_TEST); else gl.disable(gl.DEPTH_TEST);
    if (s.cullFace) gl.enable(gl.CULL_FACE); else gl.disable(gl.CULL_FACE);
    if (s.scissorTest) gl.enable(gl.SCISSOR_TEST); else gl.disable(gl.SCISSOR_TEST);
    if (s.stencilTest) gl.enable(gl.STENCIL_TEST); else gl.disable(gl.STENCIL_TEST);
    gl.colorMask(s.colorMask[0], s.colorMask[1], s.colorMask[2], s.colorMask[3]);
    gl.depthMask(s.depthMask);
  }

  /**
   * Capture camera frame as JPEG blob.
   * Pipeline: getCameraImage → blit to FBO → readPixels → 2D canvas (grayscale, rotate 90° CW, scale) → toBlob
   * Must be called synchronously within XR rAF for valid frame/camera.
   * Returns a Promise<Blob> (async because of toBlob).
   */
  function captureFrame(frame, view) {
    return new Promise(function (resolve, reject) {
      try {
        var camera = view.camera;
        if (!camera) { reject(new Error('view.camera unavailable')); return; }

        var gl = state.glCtx;
        var camTex = state.glBinding.getCameraImage(camera);
        if (!camTex) { reject(new Error('getCameraImage returned null')); return; }

        var camW = camera.width;
        var camH = camera.height;

        // --- GPU blit: camera texture → our FBO → readPixels ---
        var saved = saveGLState(gl);
        var bindFB = gl.oldBindFramebuffer || gl.bindFramebuffer;

        // Temporary color attachment
        var tmpTex = gl.createTexture();
        gl.activeTexture(gl.TEXTURE0);
        gl.bindTexture(gl.TEXTURE_2D, tmpTex);
        gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, camW, camH, 0, gl.RGBA, gl.UNSIGNED_BYTE, null);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);

        bindFB.call(gl, gl.FRAMEBUFFER, state.fbo);
        gl.framebufferTexture2D(gl.FRAMEBUFFER, gl.COLOR_ATTACHMENT0, gl.TEXTURE_2D, tmpTex, 0);

        gl.viewport(0, 0, camW, camH);
        gl.useProgram(state.program);
        gl.disable(gl.BLEND);
        gl.disable(gl.DEPTH_TEST);
        gl.disable(gl.CULL_FACE);
        gl.disable(gl.SCISSOR_TEST);
        gl.disable(gl.STENCIL_TEST);
        gl.colorMask(true, true, true, true);
        gl.depthMask(false);

        // Bind camera texture and draw
        gl.bindTexture(gl.TEXTURE_2D, camTex);
        gl.uniform1i(state.texLoc, 0);
        gl.bindVertexArray(state.vao);
        gl.drawArrays(gl.TRIANGLE_STRIP, 0, 4);

        // Read pixels (sync GPU stall — ~5-20ms, acceptable at 1/2.5s)
        var pixels = new Uint8Array(camW * camH * 4);
        gl.readPixels(0, 0, camW, camH, gl.RGBA, gl.UNSIGNED_BYTE, pixels);

        // Detach and delete temp texture
        gl.framebufferTexture2D(gl.FRAMEBUFFER, gl.COLOR_ATTACHMENT0, gl.TEXTURE_2D, null, 0);
        gl.deleteTexture(tmpTex);

        // Restore all GL state
        restoreGLState(gl, saved);

        // --- CPU: flip, grayscale, rotate 90° CW, scale, encode JPEG ---
        var cvs = state.offscreenCanvas;
        var ctx = state.offscreenCtx;
        cvs.width = camW;
        cvs.height = camH;

        var imgData = ctx.createImageData(camW, camH);
        var dst = imgData.data;

        // readPixels gives bottom-to-top; flip vertically + grayscale in one pass
        for (var y = 0; y < camH; y++) {
          var srcOff = (camH - 1 - y) * camW * 4;
          var dstOff = y * camW * 4;
          for (var x = 0; x < camW; x++) {
            var si = srcOff + x * 4;
            var di = dstOff + x * 4;
            var gray = 0.299 * pixels[si] + 0.587 * pixels[si + 1] + 0.114 * pixels[si + 2];
            dst[di] = dst[di + 1] = dst[di + 2] = gray;
            dst[di + 3] = 255;
          }
        }
        ctx.putImageData(imgData, 0, 0);

        // Crop center to 9:16 aspect ratio (matching Android SDK cropTo16x9)
        var targetRatio = 9 / 16;
        var srcRatio = camW / camH;
        var cropX = 0, cropY = 0, cropW = camW, cropH = camH;
        if (srcRatio > targetRatio) {
          // Too wide — crop sides
          cropW = Math.round(camH * targetRatio);
          cropX = Math.round((camW - cropW) / 2);
        } else if (srcRatio < targetRatio) {
          // Too tall — crop top/bottom
          cropH = Math.round(camW / targetRatio);
          cropY = Math.round((camH - cropH) / 2);
        }

        // Scale cropped region to 540×960
        var outCvs = document.createElement('canvas');
        outCvs.width = CFG.targetWidth;
        outCvs.height = CFG.targetHeight;
        var outCtx = outCvs.getContext('2d');
        outCtx.drawImage(cvs, cropX, cropY, cropW, cropH,
                          0, 0, CFG.targetWidth, CFG.targetHeight);

        outCvs.toBlob(function (blob) {
          blob ? resolve(blob) : reject(new Error('toBlob returned null'));
        }, 'image/jpeg', CFG.jpegQuality);

      } catch (e) {
        reject(e);
      }
    });
  }

  // ===== Section 5: Pose Reader =====

  /**
   * Quaternion (x,y,z,w) → YXZ intrinsic Euler angles in degrees.
   * Matches ARCore / VPS API convention.
   */
  function quaternionToEulerYXZ(qx, qy, qz, qw) {
    // Rotation matrix elements from unit quaternion
    var m02 = 2 * (qx * qz + qy * qw);
    var m12 = 2 * (qy * qz - qx * qw);
    var m22 = 1 - 2 * (qx * qx + qy * qy);
    var m10 = 2 * (qx * qy + qz * qw);
    var m11 = 1 - 2 * (qx * qx + qz * qz);

    // YXZ: rx = asin(-m12), ry = atan2(m02, m22), rz = atan2(m10, m11)
    var rx = Math.asin(-Math.max(-1, Math.min(1, m12)));
    var ry, rz;
    if (Math.abs(m12) < 0.99999) {
      ry = Math.atan2(m02, m22);
      rz = Math.atan2(m10, m11);
    } else {
      // Gimbal lock
      var m00 = 1 - 2 * (qy * qy + qz * qz);
      var m20 = 2 * (qx * qz - qy * qw);
      ry = Math.atan2(-m20, m00);
      rz = 0;
    }
    var D = 180 / Math.PI;
    return { rx: rx * D, ry: ry * D, rz: rz * D };
  }

  /** Read viewer pose directly from XRFrame (right-handed, not Unity HEAP). */
  function getTrackingPose(frame, session) {
    if (!session.refSpace) return null;
    var pose = frame.getViewerPose(session.refSpace);
    if (!pose || !pose.views || !pose.views.length) return null;

    var view = pose.views[0];
    var pos = view.transform.position;
    var ori = view.transform.orientation;
    var euler = quaternionToEulerYXZ(ori.x, ori.y, ori.z, ori.w);

    return {
      x: pos.x, y: pos.y, z: pos.z,
      rx: euler.rx, ry: euler.ry, rz: euler.rz,
      qx: ori.x, qy: ori.y, qz: ori.z, qw: ori.w,
      view: view
    };
  }

  // ===== Section 6: Intrinsics Calculator =====

  /**
   * Derive camera intrinsics from view.projectionMatrix + view.camera dimensions.
   * Returns post-90°-CW-rotation intrinsics (portrait).
   */
  var intrinsicsLogged = false;

  function computeIntrinsics(view) {
    var camera = view.camera;
    if (!camera) return null;

    var p = view.projectionMatrix; // column-major float[16]
    var camW = camera.width;
    var camH = camera.height;

    // Focal length from horizontal FOV (proj[0]) — reliable, matches camera for AR
    // For square pixels (all phone cameras): fx = fy
    var f_cam = p[0] * camW / 2;  // focal length in camera pixels
    var scale = CFG.targetWidth / camW;  // uniform scale (e.g. 540/1080 = 0.5)
    var f = f_cam * scale;  // focal length for target image

    // Principal point at center (standard for phone cameras)
    var result = {
      fx: f,
      fy: f,
      cx: CFG.targetWidth / 2,    // 270
      cy: CFG.targetHeight / 2,   // 480
      width: CFG.targetWidth,      // 540
      height: CFG.targetHeight     // 960
    };

    if (!intrinsicsLogged) {
      intrinsicsLogged = true;
      log('=== INTRINSICS DEBUG ===');
      log('camera:', camW, 'x', camH);
      log('projMatrix: p[0]=' + p[0].toFixed(6) + ' p[5]=' + p[5].toFixed(6) +
          ' p[8]=' + p[8].toFixed(6) + ' p[9]=' + p[9].toFixed(6));
      log('f_cam=' + f_cam.toFixed(1) + ' scale=' + scale.toFixed(4) + ' f=' + f.toFixed(1));
      log('hFOV=' + (2 * Math.atan(1/p[0]) * 180/Math.PI).toFixed(1) + '° vFOV=' + (2 * Math.atan(1/p[5]) * 180/Math.PI).toFixed(1) + '°');
      hudPush('cam=' + camW + 'x' + camH, '#0ff');
      hudPush('p[0]=' + p[0].toFixed(4) + ' p[5]=' + p[5].toFixed(4), '#0ff');
      hudPush('hFOV=' + (2*Math.atan(1/p[0])*180/Math.PI).toFixed(1) + ' vFOV=' + (2*Math.atan(1/p[5])*180/Math.PI).toFixed(1), '#0ff');
      hudPush('f=' + f.toFixed(1) + ' (fx=fy)', '#0ff');
    }

    return result;
  }

  // ===== Section 7: VPS HTTP Client =====

  var failCount = 0;

  function resetSessionIfNeeded() {
    if (failCount >= state.maxFailsCount) {
      state.sessionId = generateUUID();
      failCount = 0;
      log('session reset after', state.maxFailsCount, 'failures, new id:', state.sessionId.substring(0, 8));
      hudPush('session reset (' + state.maxFailsCount + ' fails)', '#f80');
    }
  }

  function sendVpsRequest(blob, pose, intrinsics) {
    resetSessionIfNeeded();
    state.pendingRequest = true;
    var controller = typeof AbortController !== 'undefined' ? new AbortController() : null;
    var timeoutId = controller ? setTimeout(function () {
      controller.abort();
    }, state.timeoutDurationMs) : null;

    var payload = {
      data: {
        attributes: {
          location_ids: CFG.locationIds,
          session_id: state.sessionId,
          user_id: state.userId,
          timestamp: Date.now() / 1000,
          location: null,
          client_coordinate_system: 'arcore',
          tracking_pose: {
            x: pose.x, y: pose.y, z: pose.z,
            rx: pose.rx, ry: pose.ry, rz: pose.rz
          },
          intrinsics: intrinsics
        }
      }
    };

    lastPayload = payload;

    var fd = new FormData();
    fd.append('json', JSON.stringify(payload));
    fd.append('image', blob, 'image');

    var a = payload.data.attributes;
    var tp = a.tracking_pose;
    var intr = a.intrinsics;
    log('sending request — full payload:', JSON.stringify(payload));
    hudPush('>> REQUEST:', '#ff0');
    hudPush('loc=' + a.location_ids.join(',') + ' sess=' + a.session_id.substring(0,8) + '...', '#ff0');
    hudPush('pose: x=' + tp.x.toFixed(3) + ' y=' + tp.y.toFixed(3) + ' z=' + tp.z.toFixed(3), '#ff0');
    hudPush('rot: rx=' + tp.rx.toFixed(1) + ' ry=' + tp.ry.toFixed(1) + ' rz=' + tp.rz.toFixed(1), '#ff0');
    hudPush('quat: ' + pose.qx.toFixed(3) + ' ' + pose.qy.toFixed(3) + ' ' + pose.qz.toFixed(3) + ' ' + pose.qw.toFixed(3), '#ff0');
    hudPush('intr: fx=' + intr.fx.toFixed(1) + ' fy=' + intr.fy.toFixed(1) + ' cx=' + intr.cx.toFixed(1) + ' cy=' + intr.cy.toFixed(1), '#ff0');
    hudPush('img: ' + intr.width + 'x' + intr.height + ' blob=' + blob.size + 'B q=' + CFG.jpegQuality, '#ff0');

    return fetch(CFG.vpsApiUrl, {
      method: 'POST',
      body: fd,
      signal: controller ? controller.signal : undefined
    })
      .then(function (resp) {
        if (!resp.ok) {
          return resp.text().then(function (body) {
            warn('HTTP', resp.status, 'response body:', body);
            throw new Error('HTTP ' + resp.status + ': ' + body);
          });
        }
        return resp.json();
      })
      .then(function (data) {
        log('response:', JSON.stringify(data));
        var d = data && data.data;
        var attrs = d && d.attributes;
        var isDone = d && d.status === 'done' && attrs;

        if (!isDone) {
          failCount++;
          hudPush('<< ' + (d ? d.status + ': ' + (d.status_description || '') : JSON.stringify(data).substring(0, 150))
            + ' [fail ' + failCount + ']', '#0ff');
        } else {
          failCount = 0;
          var vp = attrs.vps_pose;
          hudPush('<< DONE ' + (vp ? 'pose=' + vp.x.toFixed(2) + ',' + vp.y.toFixed(2) + ',' + vp.z.toFixed(2) : ''), '#0f0');
        }
        state.pendingRequest = false;
        if (timeoutId) clearTimeout(timeoutId);
        return data;
      })
      .catch(function (err) {
        failCount++;
        warn('request failed:', err.message);
        hudPush('ERR ' + err.message.substring(0, 150) + ' [fail ' + failCount + ']', '#f44');
        notifyBridge('onError', err.message);
        state.pendingRequest = false;
        if (timeoutId) clearTimeout(timeoutId);
        return null;
      });
  }

  // ===== Section 8: Pose Applier =====

  /** YXZ Euler degrees → unit quaternion {x,y,z,w}. */
  function eulerYXZToQuaternion(rx, ry, rz) {
    var R = Math.PI / 180;
    var hx = rx * R * 0.5, hy = ry * R * 0.5, hz = rz * R * 0.5;
    var cx = Math.cos(hx), sx = Math.sin(hx);
    var cy = Math.cos(hy), sy = Math.sin(hy);
    var cz = Math.cos(hz), sz = Math.sin(hz);
    // Q = Qy * Qx * Qz
    return {
      x: cy * sx * cz + sy * cx * sz,
      y: sy * cx * cz - cy * sx * sz,
      z: cy * cx * sz - sy * sx * cz,
      w: cy * cx * cz + sy * sx * sz
    };
  }

  function qInverse(q) { return { x: -q.x, y: -q.y, z: -q.z, w: q.w }; }

  function qMul(a, b) {
    return {
      x: a.w * b.x + a.x * b.w + a.y * b.z - a.z * b.y,
      y: a.w * b.y - a.x * b.z + a.y * b.w + a.z * b.x,
      z: a.w * b.z + a.x * b.y - a.y * b.x + a.z * b.w,
      w: a.w * b.w - a.x * b.x - a.y * b.y - a.z * b.z
    };
  }

  function qRotatePoint(q, p) {
    var r = qMul(qMul(q, { x: p.x, y: p.y, z: p.z, w: 0 }), qInverse(q));
    return { x: r.x, y: r.y, z: r.z };
  }

  /**
   * Apply VPS-corrected pose: compute offset between VPS pose and tracking pose
   * at capture time, then shift the XR reference space so Unity sees corrected coords.
   */
  function applyVpsPose(vpsPose, trackingPose) {
    var session = state.xrSession;
    if (!session || !session.refSpace) {
      warn('no session/refSpace for pose apply');
      return;
    }

    // VPS server pose → quaternion
    var vQ = eulerYXZToQuaternion(vpsPose.rx, vpsPose.ry, vpsPose.rz);
    var vP = { x: vpsPose.x, y: vpsPose.y, z: vpsPose.z };

    // Tracking pose at capture time (raw quaternion)
    var tQ = { x: trackingPose.qx, y: trackingPose.qy, z: trackingPose.qz, w: trackingPose.qw };
    var tP = { x: trackingPose.x, y: trackingPose.y, z: trackingPose.z };

    // offset = vps × inv(tracking)
    var offQ = qMul(vQ, qInverse(tQ));
    var rotTP = qRotatePoint(offQ, tP);
    var offP = {
      x: vP.x - rotTP.x,
      y: vP.y - rotTP.y,
      z: vP.z - rotTP.z
    };

    try {
      var xform = new XRRigidTransform(
        { x: offP.x, y: offP.y, z: offP.z, w: 1 },
        { x: offQ.x, y: offQ.y, z: offQ.z, w: offQ.w }
      );
      session.refSpace = session.refSpace.getOffsetReferenceSpace(xform);
      log('pose applied:', JSON.stringify(vpsPose));
    } catch (e) {
      warn('getOffsetReferenceSpace failed:', e.message);
    }
  }

  // ===== Section 9: Main Loop =====

  function onXRFrame(time, frame) {
    if (!state.isRunning) return;
    var session = state.xrSession;
    if (!session) return;

    // Re-register immediately
    state.rafId = session.requestAnimationFrame(onXRFrame);

    // Throttle: skip if a request is in-flight or interval hasn't elapsed
    if (state.pendingRequest) return;
    if (time - state.lastCaptureTime < CFG.captureInterval) return;
    if (state.firstRequestDelay > 0 && time - state.startedAt < state.firstRequestDelay) return;
    state.lastCaptureTime = time;

    // 1. Pose (sync — must be within rAF)
    var pose = getTrackingPose(frame, session);
    if (!pose) return;

    // 2. Intrinsics (sync)
    var intrinsics = computeIntrinsics(pose.view);
    if (!intrinsics) {
      warn('intrinsics unavailable (camera-access not granted?)');
      return;
    }

    var cam = pose.view.camera;
    log('capturing frame... cam=' + (cam ? cam.width + 'x' + cam.height : 'null'));
    hudPush('capture ' + (cam ? cam.width + 'x' + cam.height : '?') + ' -> 540x960', '#ff0');

    // 3. Capture (sync GPU read + async toBlob)
    captureFrame(frame, pose.view)
      .then(function (blob) {
        log('captured', blob.size, 'bytes, sending...');
        hudShowPreview(blob);
        // 4. Send to VPS
        return sendVpsRequest(blob, pose, intrinsics);
      })
      .then(function (response) {
        // 5. Apply corrected pose
        if (!response) return;

        var d = response.data;
        if (d && d.status === 'done' && d.attributes) {
          var vp = d.attributes.vps_pose;
          if (vp) {
            applyVpsPose(vp, pose);
            notifyBridge('onLocalized', buildLocalizationPayload(vp, pose, d.attributes.location_id || CFG.locationIds[0]));
          }
        } else if (d) {
          log('status:', d.status, d.status_description || '');
        }
      })
      .catch(function (err) {
        warn('frame pipeline error:', err.message || err);
        state.pendingRequest = false;
      });
  }

  function startLoop() {
    if (!state.xrSession) return;
    state.isRunning = true;
    state.lastCaptureTime = 0;
    state.pendingRequest = false;
    state.startedAt = performance.now();
    if (state.rafId != null) {
      try { state.xrSession.cancelAnimationFrame(state.rafId); } catch (_) {}
    }
    state.rafId = state.xrSession.requestAnimationFrame(onXRFrame);
    log('frame loop started');
  }

  function pauseLoop() {
    state.isRunning = false;
    if (state.rafId != null && state.xrSession) {
      try { state.xrSession.cancelAnimationFrame(state.rafId); } catch (_) {}
    }
    state.rafId = null;
    log('frame loop paused');
  }

  function stopLoop() {
    pauseLoop();
    cleanupGLResources();
    state.xrSession = null;
    state.glBinding = null;
    log('frame loop stopped');
  }

  // ===== Public API =====

  window.VPSClient = {
    /**
     * Call after Unity instance is created to hook into WebXR lifecycle.
     * @param {object} unityInstance — createUnityInstance() result
     */
    init: function (unityInstance) {
      state.unityInstance = unityInstance;
      var M = unityInstance.Module;

      // Hook OnStartAR
      var _origStart = M.WebXR.OnStartAR;
      M.WebXR.OnStartAR = function () {
        _origStart.apply(this, arguments);

        var session = M.WebXR.xrSession;
        if (!session) { warn('no xrSession after OnStartAR'); return; }

        state.xrSession = session;
        state.glCtx = M.ctx;

        try {
          state.glBinding = new XRWebGLBinding(session, state.glCtx);
          log('AR started, binding created');
          hudPush('AR started, binding created', '#0f0');
        } catch (e) {
          warn('XRWebGLBinding failed:', e.message);
          hudPush('XRWebGLBinding FAILED: ' + e.message, '#f44');
          return;
        }

        if (!initGLResources(state.glCtx)) {
          warn('GL resource init failed');
          return;
        }

        if (state.isRunning) {
          startLoop();
        }
      };

      // Hook OnEndXR
      var _origEnd = M.WebXR.OnEndXR;
      M.WebXR.OnEndXR = function () {
        stopLoop();
        _origEnd.apply(this, arguments);
      };

      log('VPSClient initialized, url=' + CFG.vpsApiUrl + ', locations=' + CFG.locationIds.join(','));
    },

    setBridgeCallbacks: function (callbacks) {
      state.bridgeCallbacks = callbacks || null;
    },

    configureFromUnity: function (settingsJson) {
      if (!settingsJson) return;
      try {
        applyConfigPatch(JSON.parse(settingsJson));
        log('SetupVPS settings from Unity', settingsJson);
      } catch (_) {
        warn('SetupVPS called with non-JSON settings', settingsJson);
      }
    },

    setEnabled: function (enabled) {
      if (enabled) {
        state.lastCaptureTime = 0;
        state.pendingRequest = false;
        state.startedAt = performance.now();
        if (!state.isRunning && state.xrSession) {
          startLoop();
        } else {
          state.isRunning = true;
        }
      } else {
        pauseLoop();
      }
      log('VPS enabled:', enabled);
    },

    resetSession: function () {
      state.sessionId = generateUUID();
      failCount = 0;
      log('VPS session reset');
    },

    setLocationIds: function (data) {
      var ids = normalizeLocationIds(data);
      if (!ids.length) return;
      CFG.locationIds = ids;
      this.resetSession();
      log('Location ids set:', ids.join(','));
    },

    setSendFastPhotoDelay: function (value) {
      state.sendFastPhotoDelay = value;
      if (value > 0) CFG.captureInterval = value;
    },

    setSendPhotoDelay: function (value) {
      state.sendPhotoDelay = value;
      if (value > 0) CFG.captureInterval = value;
    },

    setTimeoutDuration: function (value) {
      if (value > 0) state.timeoutDurationMs = value;
    },

    setFirstRequestDelay: function (value) {
      if (value >= 0) state.firstRequestDelay = value;
    }
  };
})();
