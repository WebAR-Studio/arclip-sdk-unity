using System.Collections;
using ARLib;
using NavigationDemo.UI.Views;
using UnityEngine;

namespace NavigationDemo.UI.Controllers
{
    [DisallowMultipleComponent]
    public class PermissionStateMonitorController : MonoBehaviour
    {
        private enum PermissionIssue
        {
            None = 0,
            CameraAccess = 1,
            GeoAccess = 2,
            VpsNetwork = 3,
        }

        private static readonly string[] _networkErrorKeywords =
        {
            "network",
            "internet",
            "connection",
            "timeout",
            "timed out",
            "socket",
            "offline",
            "dns",
            "net::",
            "сеть",
            "интернет",
            "соединени",
            "таймаут",
        };

        [Header("Dependencies")]
        [SerializeField] private StateController _stateController;
        [SerializeField] private UIViewsController _uiViewsController;
        [SerializeField] private SettingsPromptView _settingsPromptView;

        [Header("Monitoring")]
        [SerializeField] [Min(0.1f)] private float _checkIntervalSeconds = 1f;
        [SerializeField] private bool _startMonitoringOnEnable = true;

        [Header("State Flow")]
        [SerializeField] private NavigationState _noPermissionState = NavigationState.NoPermission;
        [SerializeField] private NavigationState _fallbackRestoreState = NavigationState.CategorySelection;

        [Header("SettingsPrompt Variants")]
        [SerializeField] private string _cameraVariantId = "NoCamera";
        [SerializeField] private string _geoVariantId = "NoGeo";
        [SerializeField] private string _vpsNetworkVariantId = "NoInternet";

        private Coroutine _monitorRoutine;
        private bool _isSubscribed;
        private bool _isMonitoring;
        private bool _isBlockedByPermission;
        private bool _hasRestoreState;
        private NavigationState _restoreState;

        private void Reset()
        {
            ResolveReferences();
        }

        private void Awake()
        {
            ResolveReferences();
            _restoreState = _fallbackRestoreState;
        }

        private void OnEnable()
        {
            ResolveReferences();
            SubscribeEvents();

            _isBlockedByPermission = _stateController != null && _stateController.CurrentState == _noPermissionState;

            if (_isBlockedByPermission)
            {
                StopMonitoring();
                ShowSettingsPrompt();
                return;
            }

            if (_startMonitoringOnEnable)
            {
                StartMonitoring();
            }
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
            StopMonitoring();
        }

        private void OnApplicationFocus(bool valueHasFocus)
        {
            if (!valueHasFocus || !_isMonitoring || _isBlockedByPermission)
            {
                return;
            }

            CheckAccessNow();
        }

        [ContextMenu("Check Access Now")]
        public void CheckAccessNow()
        {
            if (_isBlockedByPermission)
            {
                return;
            }

            if (!HasCameraAccess())
            {
                EnterNoPermissionState(PermissionIssue.CameraAccess);
                return;
            }

            if (!HasGeoAccess())
            {
                EnterNoPermissionState(PermissionIssue.GeoAccess);
                return;
            }
        }

        public void StartMonitoring()
        {
            if (_isMonitoring || _isBlockedByPermission)
            {
                return;
            }

            _isMonitoring = true;
            _monitorRoutine = StartCoroutine(MonitorRoutine());
        }

        public void StopMonitoring()
        {
            _isMonitoring = false;
            if (_monitorRoutine == null)
            {
                return;
            }

            StopCoroutine(_monitorRoutine);
            _monitorRoutine = null;
        }

        private IEnumerator MonitorRoutine()
        {
            while (_isMonitoring)
            {
                CheckAccessNow();
                yield return new WaitForSeconds(_checkIntervalSeconds);
            }

            _monitorRoutine = null;
        }

        private void EnterNoPermissionState(PermissionIssue issue)
        {
            if (_stateController == null)
            {
                return;
            }

            NavigationState currentState = _stateController.CurrentState;
            if (currentState != _noPermissionState)
            {
                _restoreState = currentState;
                _hasRestoreState = true;
            }

            _isBlockedByPermission = true;
            StopMonitoring();
            ApplySettingsPromptVariant(issue);
            _stateController.SetState(_noPermissionState);
            ShowSettingsPrompt();
        }

        private void RestorePreviousStateAndResume()
        {
            if (!_isBlockedByPermission || _stateController == null)
            {
                return;
            }

            _isBlockedByPermission = false;

            NavigationState restoreState = _hasRestoreState ? _restoreState : _fallbackRestoreState;
            _hasRestoreState = false;

            _stateController.SetState(restoreState);
            StartMonitoring();
        }

        private void ApplySettingsPromptVariant(PermissionIssue issue)
        {
            if (_settingsPromptView == null)
            {
                return;
            }

            string variantId = issue switch
            {
                PermissionIssue.CameraAccess => _cameraVariantId,
                PermissionIssue.GeoAccess => _geoVariantId,
                PermissionIssue.VpsNetwork => _vpsNetworkVariantId,
                _ => string.Empty
            };

            if (!string.IsNullOrWhiteSpace(variantId) && _settingsPromptView.TryApplyVariant(variantId))
            {
                return;
            }

            _settingsPromptView.ApplyVariantByIndex(0);
        }

        private void ShowSettingsPrompt()
        {
            if (_uiViewsController != null)
            {
                _uiViewsController.ShowOnlySettingsPromptView();
                return;
            }

            _settingsPromptView?.Show();
        }

        private void HandleOpenSettingsClicked(SettingsPromptView value)
        {
            RestorePreviousStateAndResume();
        }

        private void HandleVpsError(string errorMessage)
        {
            if (_isBlockedByPermission)
            {
                return;
            }

            if (!IsNetworkRelatedVpsError(errorMessage))
            {
                return;
            }

            EnterNoPermissionState(PermissionIssue.VpsNetwork);
        }

        private void HandleStateChanged(NavigationState state)
        {
            if (state == _noPermissionState)
            {
                _isBlockedByPermission = true;
                StopMonitoring();
                ShowSettingsPrompt();
                return;
            }

            if (!_isBlockedByPermission)
            {
                return;
            }

            _isBlockedByPermission = false;
            StartMonitoring();
        }

        private static bool HasGeoAccess()
        {
#if UNITY_EDITOR
            return true;
#else
            if (!Application.HasUserAuthorization(UserAuthorization.Location))
            {
                return false;
            }

            return Input.location.isEnabledByUser;
#endif
        }

        private static bool HasCameraAccess()
        {
#if UNITY_EDITOR
            return true;
#else
            if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
            {
                return false;
            }

            return WebCamTexture.devices != null && WebCamTexture.devices.Length > 0;
#endif
        }

        private static bool IsNetworkRelatedVpsError(string errorMessage)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(errorMessage))
            {
                return false;
            }

            string lowerCaseMessage = errorMessage.ToLowerInvariant();
            for (int index = 0; index < _networkErrorKeywords.Length; index++)
            {
                string keyword = _networkErrorKeywords[index];
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    continue;
                }

                if (lowerCaseMessage.Contains(keyword))
                {
                    return true;
                }
            }

            return false;
        }

        private void ResolveReferences()
        {
            if (_stateController == null)
            {
                _stateController = GetComponent<StateController>();
            }

            if (_uiViewsController == null && _stateController != null)
            {
                _uiViewsController = _stateController.UIViewsController;
            }

            if (_uiViewsController == null)
            {
                _uiViewsController = GetComponent<UIViewsController>();
            }

            if (_settingsPromptView == null && _uiViewsController != null)
            {
                _settingsPromptView = _uiViewsController.SettingsPromptView;
            }
        }

        private void SubscribeEvents()
        {
            if (_isSubscribed)
            {
                return;
            }

            ARLibController.OnVPSErrorHappened += HandleVpsError;

            if (_settingsPromptView != null)
            {
                _settingsPromptView.OpenSettingsClicked -= HandleOpenSettingsClicked;
                _settingsPromptView.OpenSettingsClicked += HandleOpenSettingsClicked;
            }

            if (_stateController != null)
            {
                _stateController.StateChanged += HandleStateChanged;
            }

            _isSubscribed = true;
        }

        private void UnsubscribeEvents()
        {
            if (!_isSubscribed)
            {
                return;
            }

            ARLibController.OnVPSErrorHappened -= HandleVpsError;

            if (_settingsPromptView != null)
            {
                _settingsPromptView.OpenSettingsClicked -= HandleOpenSettingsClicked;
            }

            if (_stateController != null)
            {
                _stateController.StateChanged -= HandleStateChanged;
            }

            _isSubscribed = false;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_checkIntervalSeconds < 0.1f)
            {
                _checkIntervalSeconds = 0.1f;
            }

            ResolveReferences();
        }
#endif
    }
}
