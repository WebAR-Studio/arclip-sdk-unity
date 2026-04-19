using System.Collections;
using System.Collections.Generic;
using ARLib;
using NavigationDemo.Navigation.UI;
using NavigationDemo.UI.Controllers;
using UnityEngine;

namespace NavigationDemo.Navigation
{
    [DisallowMultipleComponent]
    public class VPSManager : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private StateController _stateController;
        [SerializeField] private DestinationSelectionFlowController _destinationSelectionFlowController;
        [SerializeField] private NavigationUiStateBinder _navigationUiStateBinder;
        [SerializeField] private TargetManager _targetManager;
        [SerializeField] private PathShowerArrows _pathShowerArrows;
        [SerializeField] private CircleLoadEffect _circleLoadEffect;

        [Header("ARClip Bootstrap")]
        [SerializeField] private bool _initializeArClipOnAwake = true;
        [SerializeField] private bool _enableCameraOnAwake = true;
        [SerializeField] private bool _enableArOnAwake = true;

        [Header("VPS Settings")]
        [SerializeField] private string _serverUrl = "https://was-vps.web-ar.xyz/vps/api/v3";
        [SerializeField] private string[] _locationIds = new string[0];
        [SerializeField] private bool _useGps;
        [SerializeField] [Min(1)] private int _maxFailsCount = 5;

        [Header("Initialization Flow")]
        [SerializeField] [Min(0f)] private float _switchToNavigationDelaySeconds = 3f;
        [SerializeField] private bool _showCircleLoadEffectInInitialization = true;
        [SerializeField] private bool _deactivateCircleLoadObjectWhenHidden = true;

        [Header("Editor Emulation")]
        [SerializeField] private bool _emulateVpsInEditor = true;
        [SerializeField] [Min(0f)] private float _editorVpsEmulationDelaySeconds = 3f;

        [Header("Navigation Flow")]
        [SerializeField] private bool _autoStartRouteInNavigation = true;
        [SerializeField] private bool _showArrows = true;

        private bool _isSubscribed;
        private bool _shouldRunVps;
        private bool _isWaitingForUpdatePosition;
        private bool _warnedAboutMissingLocationIds;
        private bool _areVpsEventsSubscribed;
        private Coroutine _switchToNavigationRoutine;
        private Coroutine _editorVpsEmulationRoutine;

        private void Awake()
        {
            ResolveReferences();
            BootstrapArClip();
        }

        private void OnEnable()
        {
            ResolveReferences();
            SubscribeEvents();
            ApplyState(_stateController == null ? NavigationState.Loading : _stateController.CurrentState);
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
            StopNavigationDelayRoutine();
            StopEditorVpsEmulationRoutine();
            HideCircleLoadEffect();
            StopVps();
        }

        private void HandleStateChanged(NavigationState state)
        {
            ApplyState(state);
        }

        private void ApplyState(NavigationState state)
        {
            bool shouldRunVps = state == NavigationState.NavigationInitialization || state == NavigationState.Navigation;
            _shouldRunVps = shouldRunVps;

            if (!shouldRunVps)
            {
                _isWaitingForUpdatePosition = false;
                StopNavigationDelayRoutine();
                StopEditorVpsEmulationRoutine();
                HideCircleLoadEffect();
                _pathShowerArrows?.NotifyVpsLocalizationFailure();
                StopVps();
                return;
            }

            if (state == NavigationState.NavigationInitialization)
            {
                EnterNavigationInitializationState();
                return;
            }

            EnterNavigationState();
        }

        private void EnterNavigationInitializationState()
        {
            StopNavigationDelayRoutine();
            StopEditorVpsEmulationRoutine();
            ShowCircleLoadEffect();
            _pathShowerArrows?.NotifyVpsLocalizationFailure();

            if (IsEditorVpsEmulationEnabled())
            {
                _isWaitingForUpdatePosition = false;
                _editorVpsEmulationRoutine = StartCoroutine(EditorVpsEmulationRoutine());
                return;
            }

            _isWaitingForUpdatePosition = true;
            StartVpsInitialization();
        }

        private void EnterNavigationState()
        {
            _isWaitingForUpdatePosition = false;
            StopNavigationDelayRoutine();
            StopEditorVpsEmulationRoutine();
            HideCircleLoadEffect();
            EnsureVpsStarted();
            TryStartRouteInNavigationState();
        }

        private void StartVpsInitialization()
        {
            if (IsEditorVpsEmulationEnabled())
            {
                return;
            }

            string[] locationIds = GetValidLocationIds();
            if (locationIds.Length == 0)
            {
                if (!_warnedAboutMissingLocationIds)
                {
                    Debug.LogWarning($"[{nameof(VPSManager)}] VPS location ids are empty. SetupVPS was skipped.", this);
                    _warnedAboutMissingLocationIds = true;
                }

                return;
            }

            VPSSettings settings = new VPSSettings
            {
                serverUrl = _serverUrl,
                locationsIds = locationIds,
                gps = _useGps,
                maxFailsCount = Mathf.Max(1, _maxFailsCount)
            };

            ARLibController.SetupVPS(settings);
        }

        private void EnsureVpsStarted()
        {
            if (!_shouldRunVps || IsEditorVpsEmulationEnabled())
            {
                return;
            }

            ARLibController.StartVPS();
        }

        private void StopVps()
        {
            if (IsEditorVpsEmulationEnabled())
            {
                return;
            }

            ARLibController.StopVPS();
        }

        public void ForceStopVps()
        {
            _shouldRunVps = false;
            _isWaitingForUpdatePosition = false;
            StopNavigationDelayRoutine();
            StopEditorVpsEmulationRoutine();
            HideCircleLoadEffect();
            _pathShowerArrows?.NotifyVpsLocalizationFailure();
            StopVps();
        }

        private void TryStartRouteInNavigationState()
        {
            if (!_autoStartRouteInNavigation || _navigationUiStateBinder == null || _destinationSelectionFlowController == null)
            {
                return;
            }

            if (_targetManager != null && _targetManager.IsRouting)
            {
                return;
            }

            string targetKey = _destinationSelectionFlowController.SelectedNavigationTargetKey;
            if (string.IsNullOrWhiteSpace(targetKey))
            {
                return;
            }

            _navigationUiStateBinder.StartRouteByKey(targetKey, _showArrows);
        }

        private void HandleVpsInitialized()
        {
            if (!_shouldRunVps || IsEditorVpsEmulationEnabled())
            {
                return;
            }

            ARLibController.StartVPS();
        }

        // UpdatePosition in ARClip SDK flow is delivered via VPSPositionUpdated.
        private void HandleUpdatePosition(VPSPoseData poseData)
        {
            if (IsEditorVpsEmulationEnabled())
            {
                return;
            }

            if (_shouldRunVps)
            {
                _pathShowerArrows?.NotifyVpsLocalizationSuccess();
            }

            if (!_shouldRunVps || !_isWaitingForUpdatePosition || _stateController == null)
            {
                return;
            }

            if (_stateController.CurrentState != NavigationState.NavigationInitialization)
            {
                return;
            }

            _isWaitingForUpdatePosition = false;
            StartSwitchToNavigationDelay(_switchToNavigationDelaySeconds);
        }

        private void HandleVpsError(string errorMessage)
        {
            _pathShowerArrows?.NotifyVpsLocalizationFailure();
        }

        private IEnumerator SwitchToNavigationAfterDelay(float delaySeconds)
        {
            if (delaySeconds > 0f)
            {
                yield return new WaitForSeconds(delaySeconds);
            }

            _switchToNavigationRoutine = null;

            if (_stateController == null || !_shouldRunVps)
            {
                yield break;
            }

            if (_stateController.CurrentState != NavigationState.NavigationInitialization)
            {
                yield break;
            }

            _stateController.SetNavigationState();
        }

        private IEnumerator EditorVpsEmulationRoutine()
        {
            if (_editorVpsEmulationDelaySeconds > 0f)
            {
                yield return new WaitForSeconds(_editorVpsEmulationDelaySeconds);
            }

            _editorVpsEmulationRoutine = null;

            if (_stateController == null || !_shouldRunVps)
            {
                yield break;
            }

            if (_stateController.CurrentState != NavigationState.NavigationInitialization)
            {
                yield break;
            }

            _stateController.SetNavigationState();
        }

        private void StartSwitchToNavigationDelay(float delaySeconds)
        {
            StopNavigationDelayRoutine();
            _switchToNavigationRoutine = StartCoroutine(SwitchToNavigationAfterDelay(delaySeconds));
        }

        private void StopNavigationDelayRoutine()
        {
            if (_switchToNavigationRoutine == null)
            {
                return;
            }

            StopCoroutine(_switchToNavigationRoutine);
            _switchToNavigationRoutine = null;
        }

        private void StopEditorVpsEmulationRoutine()
        {
            if (_editorVpsEmulationRoutine == null)
            {
                return;
            }

            StopCoroutine(_editorVpsEmulationRoutine);
            _editorVpsEmulationRoutine = null;
        }

        private void ShowCircleLoadEffect()
        {
            if (!_showCircleLoadEffectInInitialization || _circleLoadEffect == null)
            {
                return;
            }

            if (_deactivateCircleLoadObjectWhenHidden && !_circleLoadEffect.gameObject.activeSelf)
            {
                _circleLoadEffect.gameObject.SetActive(true);
            }

            _circleLoadEffect.SpawnCircles();
        }

        private void HideCircleLoadEffect()
        {
            if (_circleLoadEffect == null)
            {
                return;
            }

            _circleLoadEffect.ClearCircles();

            if (_deactivateCircleLoadObjectWhenHidden && _circleLoadEffect.gameObject.activeSelf)
            {
                _circleLoadEffect.gameObject.SetActive(false);
            }
        }

        private void ResolveReferences()
        {
            if (_stateController == null)
            {
                _stateController = GetComponent<StateController>();
            }

            if (_destinationSelectionFlowController == null)
            {
                _destinationSelectionFlowController = GetComponent<DestinationSelectionFlowController>();
            }

            if (_navigationUiStateBinder == null)
            {
                _navigationUiStateBinder = GetComponent<NavigationUiStateBinder>();
            }

            if (_targetManager == null)
            {
                _targetManager = GetComponent<TargetManager>();
            }

            if (_pathShowerArrows == null)
            {
                _pathShowerArrows = GetComponent<PathShowerArrows>();
            }

            if (_pathShowerArrows == null && _targetManager != null)
            {
                _pathShowerArrows = _targetManager.GetComponent<PathShowerArrows>();
            }

            if (_circleLoadEffect == null)
            {
                _circleLoadEffect = FindObjectOfType<CircleLoadEffect>(true);
            }
        }

        private void BootstrapArClip()
        {
            if (!_initializeArClipOnAwake)
            {
                return;
            }

            ARLibController.Initialize();

            if (_enableCameraOnAwake)
            {
                ARLibController.EnableCamera();
            }

            if (_enableArOnAwake)
            {
                ARLibController.EnableAR();
            }
        }

        private bool IsEditorVpsEmulationEnabled()
        {
            return Application.isEditor && _emulateVpsInEditor;
        }

        private string[] GetValidLocationIds()
        {
            if (_locationIds == null || _locationIds.Length == 0)
            {
                return new string[0];
            }

            List<string> valid = new List<string>(_locationIds.Length);
            for (int i = 0; i < _locationIds.Length; i++)
            {
                string locationId = _locationIds[i];
                if (string.IsNullOrWhiteSpace(locationId))
                {
                    continue;
                }

                valid.Add(locationId.Trim());
            }

            return valid.ToArray();
        }

        private void SubscribeEvents()
        {
            if (_isSubscribed)
            {
                return;
            }

            if (_stateController != null)
            {
                _stateController.StateChanged += HandleStateChanged;
            }

            if (!IsEditorVpsEmulationEnabled() && !_areVpsEventsSubscribed)
            {
                ARLibController.VPSInitialized += HandleVpsInitialized;
                ARLibController.VPSPositionUpdated += HandleUpdatePosition;
                ARLibController.OnVPSErrorHappened += HandleVpsError;
                _areVpsEventsSubscribed = true;
            }

            _isSubscribed = true;
        }

        private void UnsubscribeEvents()
        {
            if (!_isSubscribed)
            {
                return;
            }

            if (_stateController != null)
            {
                _stateController.StateChanged -= HandleStateChanged;
            }

            if (_areVpsEventsSubscribed)
            {
                ARLibController.VPSInitialized -= HandleVpsInitialized;
                ARLibController.VPSPositionUpdated -= HandleUpdatePosition;
                ARLibController.OnVPSErrorHappened -= HandleVpsError;
                _areVpsEventsSubscribed = false;
            }

            _isSubscribed = false;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_maxFailsCount < 1)
            {
                _maxFailsCount = 1;
            }

            if (_editorVpsEmulationDelaySeconds < 0f)
            {
                _editorVpsEmulationDelaySeconds = 0f;
            }

            ResolveReferences();
        }
#endif
    }
}
