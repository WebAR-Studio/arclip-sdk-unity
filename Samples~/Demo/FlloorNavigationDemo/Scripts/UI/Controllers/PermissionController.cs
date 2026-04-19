using NavigationDemo.UI.Base;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NavigationDemo.UI.Controllers
{
    [DisallowMultipleComponent]
    public class PermissionController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private UIViewsController _uiViewsController;
        [SerializeField] private BaseView _noInternetView;
        [SerializeField] private BaseView _noGeoView;
        [SerializeField] private BaseView _noCameraView;

        [Header("Navigation")]
        [SerializeField] private string _targetSceneName;
        [SerializeField] private bool _checkOnStart = true;
        [SerializeField] private bool _checkOnApplicationFocus = true;

        private bool _isLoadingScene;

        private void Reset()
        {
            if (_uiViewsController == null)
            {
                _uiViewsController = GetComponent<UIViewsController>();
            }
        }

        private void Start()
        {
            if (_checkOnStart)
            {
                CheckPermissions();
            }
        }

        private void OnApplicationFocus(bool valueHasFocus)
        {
            if (!valueHasFocus || !_checkOnApplicationFocus || _isLoadingScene)
            {
                return;
            }

            CheckPermissions();
        }

        [ContextMenu("Check Permissions")]
        public void CheckPermissions()
        {
            if (_isLoadingScene)
            {
                return;
            }

            if (!HasInternetAccess())
            {
                ShowPermissionView(_noInternetView);
                return;
            }

            HideView(_noInternetView);

            if (!HasGeoAccess())
            {
                ShowPermissionView(_noGeoView);
                return;
            }

            HideView(_noGeoView);

            if (!HasCameraAccess())
            {
                ShowPermissionView(_noCameraView);
                return;
            }

            HideView(_noCameraView);
            LoadTargetScene();
        }

        private void ShowPermissionView(BaseView valueTargetView)
        {
            if (valueTargetView == null)
            {
                return;
            }

            if (valueTargetView.IsVisible)
            {
                return;
            }

            _uiViewsController?.HideAll();
            HideOtherPermissionViews(valueTargetView);
            valueTargetView.Show();
        }

        private void HideOtherPermissionViews(BaseView valueExcept)
        {
            if (_noInternetView != null && _noInternetView != valueExcept)
            {
                _noInternetView.Hide();
            }

            if (_noGeoView != null && _noGeoView != valueExcept)
            {
                _noGeoView.Hide();
            }

            if (_noCameraView != null && _noCameraView != valueExcept)
            {
                _noCameraView.Hide();
            }
        }

        private static void HideView(BaseView valueView)
        {
            if (valueView == null || !valueView.IsVisible)
            {
                return;
            }

            valueView.Hide();
        }

        private static bool HasInternetAccess()
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
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

        private void LoadTargetScene()
        {
            if (string.IsNullOrWhiteSpace(_targetSceneName))
            {
                Debug.LogWarning("PermissionController: target scene name is empty.");
                return;
            }

            if (!Application.CanStreamedLevelBeLoaded(_targetSceneName))
            {
                Debug.LogWarning(
                    $"PermissionController: scene '{_targetSceneName}' is not available in Build Settings.");
                return;
            }

            _isLoadingScene = true;
            SceneManager.LoadScene(_targetSceneName);
        }
    }
}
