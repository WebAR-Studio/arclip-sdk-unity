using NavigationDemo.UI.Base;
using NavigationDemo.UI.Elements;
using UnityEngine;

namespace NavigationDemo.UI.Views
{
    [ExecuteAlways]
    public class LoadingScreenView : BaseView
    {
        [SerializeField] private RectTransform _statusBarContainer;
        [SerializeField] private RectTransform _brandContainer;
        [SerializeField] private RectTransform _progressContainer;
        [SerializeField] private string _statusBarElementResourcePath = "UI/Elements/LoadingStatusBarElement";
        [SerializeField] private string _brandElementResourcePath = "UI/Elements/LoadingBrandElement";
        [SerializeField] private string _progressElementResourcePath = "UI/Elements/LoadingProgressElement";
        [SerializeField] private bool _rebuildInEditMode = true;
        [SerializeField] private string _defaultTimeValue = "9:15";
        [SerializeField] private Sprite _defaultBrandIconSprite;
        [SerializeField] private string _defaultBrandArValue = "AR";
        [SerializeField] private string _defaultBrandNavigationValue = "Navigation";
        [SerializeField] private string _defaultLoadingLabelValue = "Загрузка локаций";
        [SerializeField] [Range(0f, 1f)] private float _defaultLoadingProgressValue = 0.33f;

        private LoadingStatusBarElementView _loadingStatusBarElementView;
        private LoadingBrandElementView _loadingBrandElementView;
        private LoadingProgressElementView _loadingProgressElementView;

        public string Value => GetLoadingLabelValue();

        protected override void Awake()
        {
            base.Awake();
            BuildView();
        }

        private void OnEnable()
        {
            if (!Application.isPlaying && !_rebuildInEditMode)
            {
                return;
            }

            BuildView();
        }

        private void OnValidate()
        {
            if (!Application.isPlaying && !_rebuildInEditMode)
            {
                return;
            }

            BuildView();
        }

        public string GetLoadingLabelValue()
        {
            if (_loadingProgressElementView == null)
            {
                return string.Empty;
            }

            return _loadingProgressElementView.GetLabelValue();
        }

        public void SetLoadingLabelValue(string value)
        {
            if (_loadingProgressElementView == null)
            {
                return;
            }

            _loadingProgressElementView.SetLabelValue(value);
        }

        public float GetLoadingProgressValue()
        {
            if (_loadingProgressElementView == null)
            {
                return 0f;
            }

            return _loadingProgressElementView.GetProgressValue();
        }

        public void SetLoadingProgressValue(float value)
        {
            if (_loadingProgressElementView == null)
            {
                return;
            }

            _loadingProgressElementView.SetProgressValue(value);
        }

        public void SetBrandValue(Sprite valueIcon, string valueAr, string valueNavigation)
        {
            if (_loadingBrandElementView == null)
            {
                return;
            }

            _loadingBrandElementView.SetIconSprite(valueIcon);
            _loadingBrandElementView.SetArValue(valueAr);
            _loadingBrandElementView.SetNavigationValue(valueNavigation);
        }

        public void SetBrandValue(string valueIcon, string valueAr, string valueNavigation)
        {
            SetBrandValue(_defaultBrandIconSprite, valueAr, valueNavigation);
        }

        public void SetTimeValue(string value)
        {
            if (_loadingStatusBarElementView == null)
            {
                return;
            }

            _loadingStatusBarElementView.SetTimeValue(value);
        }

        private void BuildView()
        {
            BuildStatusBarElement();
            BuildBrandElement();
            BuildProgressElement();
            ApplyDefaultState();
        }

        private void BuildStatusBarElement()
        {
            if (_statusBarContainer == null)
            {
                return;
            }

            if (_loadingStatusBarElementView == null)
            {
                _loadingStatusBarElementView = _statusBarContainer.GetComponentInChildren<LoadingStatusBarElementView>(true);
            }

            if (_loadingStatusBarElementView == null)
            {
                LoadingStatusBarElementView value = Resources.Load<LoadingStatusBarElementView>(_statusBarElementResourcePath);
                if (value == null)
                {
                    return;
                }

                _loadingStatusBarElementView = Instantiate(value, _statusBarContainer);
                _loadingStatusBarElementView.name = value.name;
            }

            StretchToParent(_loadingStatusBarElementView.GetComponent<RectTransform>());
        }

        private void BuildBrandElement()
        {
            if (_brandContainer == null)
            {
                return;
            }

            if (_loadingBrandElementView == null)
            {
                _loadingBrandElementView = _brandContainer.GetComponentInChildren<LoadingBrandElementView>(true);
            }

            if (_loadingBrandElementView == null)
            {
                LoadingBrandElementView value = Resources.Load<LoadingBrandElementView>(_brandElementResourcePath);
                if (value == null)
                {
                    return;
                }

                _loadingBrandElementView = Instantiate(value, _brandContainer);
                _loadingBrandElementView.name = value.name;
            }

            StretchToParent(_loadingBrandElementView.GetComponent<RectTransform>());
        }

        private void BuildProgressElement()
        {
            if (_progressContainer == null)
            {
                return;
            }

            if (_loadingProgressElementView == null)
            {
                _loadingProgressElementView = _progressContainer.GetComponentInChildren<LoadingProgressElementView>(true);
            }

            if (_loadingProgressElementView == null)
            {
                LoadingProgressElementView value = Resources.Load<LoadingProgressElementView>(_progressElementResourcePath);
                if (value == null)
                {
                    return;
                }

                _loadingProgressElementView = Instantiate(value, _progressContainer);
                _loadingProgressElementView.name = value.name;
            }

            StretchToParent(_loadingProgressElementView.GetComponent<RectTransform>());
        }

        private void ApplyDefaultState()
        {
            if (_loadingStatusBarElementView != null)
            {
                _loadingStatusBarElementView.SetTimeValue(_defaultTimeValue);
            }

            if (_loadingBrandElementView != null)
            {
                _loadingBrandElementView.SetIconSprite(_defaultBrandIconSprite);
                _loadingBrandElementView.SetArValue(_defaultBrandArValue);
                _loadingBrandElementView.SetNavigationValue(_defaultBrandNavigationValue);
            }

            if (_loadingProgressElementView != null)
            {
                _loadingProgressElementView.SetLabelValue(_defaultLoadingLabelValue);
                _loadingProgressElementView.SetProgressValue(_defaultLoadingProgressValue);
            }
        }

        private static void StretchToParent(RectTransform value)
        {
            if (value == null)
            {
                return;
            }

            value.anchorMin = Vector2.zero;
            value.anchorMax = Vector2.one;
            value.offsetMin = Vector2.zero;
            value.offsetMax = Vector2.zero;
            value.anchoredPosition = Vector2.zero;
            value.localScale = Vector3.one;
        }

        [ContextMenu("Rebuild View")]
        private void RebuildView()
        {
            BuildView();
        }
    }
}
