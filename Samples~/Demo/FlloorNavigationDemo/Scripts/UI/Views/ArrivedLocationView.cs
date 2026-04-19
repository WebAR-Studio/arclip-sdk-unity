using System;
using NavigationDemo.UI.Base;
using NavigationDemo.UI.Elements;
using UnityEngine;

namespace NavigationDemo.UI.Views
{
    [ExecuteAlways]
    public class ArrivedLocationView : BaseView
    {
        [SerializeField] private RectTransform _statusContainer;
        [SerializeField] private RectTransform _leftButtonContainer;
        [SerializeField] private RectTransform _rightButtonContainer;
        [SerializeField] private string _statusElementResourcePath = "UI/Elements/StatusTextElement";
        [SerializeField] private string _pillButtonElementResourcePath = "UI/Elements/PillButtonElement";
        [SerializeField] private bool _rebuildInEditMode = true;
        [SerializeField] private string _defaultTitleValue = "\u0412\u044b \u043d\u0430 \u043c\u0435\u0441\u0442\u0435";
        [SerializeField] private string _defaultSubtitleValue = "12 Storeez";
        [SerializeField] private string _defaultLeftButtonValue = "\u041e \u043b\u043e\u043a\u0430\u0446\u0438\u0438";
        [SerializeField] private string _defaultRightButtonValue = "\u0417\u0430\u0432\u0435\u0440\u0448\u0438\u0442\u044c";
        [SerializeField] private Color _defaultLeftButtonColor = new Color(0f, 0f, 0f, 0.2f);
        [SerializeField] private Color _defaultRightButtonColor = new Color(0.07450981f, 0.83137256f, 0.69411767f, 0.5f);

        private StatusTextElementView _statusTextElementView;
        private PillButtonElementView _leftButtonElementView;
        private PillButtonElementView _rightButtonElementView;

        public event Action<ArrivedLocationView> LeftButtonClicked;
        public event Action<ArrivedLocationView> RightButtonClicked;

        public string Value => GetTitleValue();

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

        public string GetTitleValue()
        {
            if (_statusTextElementView == null)
            {
                return string.Empty;
            }

            return _statusTextElementView.GetTitleValue();
        }

        public void SetTitleValue(string value)
        {
            if (_statusTextElementView == null)
            {
                return;
            }

            _statusTextElementView.SetTitleValue(value);
        }

        public string GetSubtitleValue()
        {
            if (_statusTextElementView == null)
            {
                return string.Empty;
            }

            return _statusTextElementView.GetSubtitleValue();
        }

        public void SetSubtitleValue(string value)
        {
            if (_statusTextElementView == null)
            {
                return;
            }

            _statusTextElementView.SetSubtitleValue(value);
        }

        public string GetLeftButtonValue()
        {
            if (_leftButtonElementView == null)
            {
                return string.Empty;
            }

            return _leftButtonElementView.GetValue();
        }

        public void SetLeftButtonValue(string value)
        {
            if (_leftButtonElementView == null)
            {
                return;
            }

            _leftButtonElementView.SetValue(value);
        }

        public string GetRightButtonValue()
        {
            if (_rightButtonElementView == null)
            {
                return string.Empty;
            }

            return _rightButtonElementView.GetValue();
        }

        public void SetRightButtonValue(string value)
        {
            if (_rightButtonElementView == null)
            {
                return;
            }

            _rightButtonElementView.SetValue(value);
        }

        public void SetButtonsInteractable(bool value)
        {
            if (_leftButtonElementView != null)
            {
                _leftButtonElementView.SetInteractable(value);
            }

            if (_rightButtonElementView != null)
            {
                _rightButtonElementView.SetInteractable(value);
            }
        }

        private void BuildView()
        {
            BuildStatusElement();
            BuildLeftButtonElement();
            BuildRightButtonElement();
            ApplyDefaultState();
        }

        private void BuildStatusElement()
        {
            if (_statusContainer == null)
            {
                return;
            }

            if (_statusTextElementView == null)
            {
                _statusTextElementView = _statusContainer.GetComponentInChildren<StatusTextElementView>(true);
            }

            if (_statusTextElementView == null)
            {
                StatusTextElementView value = Resources.Load<StatusTextElementView>(_statusElementResourcePath);
                if (value == null)
                {
                    return;
                }

                _statusTextElementView = Instantiate(value, _statusContainer);
                _statusTextElementView.name = value.name;
            }

            StretchToParent(_statusTextElementView.GetComponent<RectTransform>());
        }

        private void BuildLeftButtonElement()
        {
            if (_leftButtonContainer == null)
            {
                return;
            }

            if (_leftButtonElementView == null)
            {
                _leftButtonElementView = _leftButtonContainer.GetComponentInChildren<PillButtonElementView>(true);
            }

            if (_leftButtonElementView == null)
            {
                PillButtonElementView value = Resources.Load<PillButtonElementView>(_pillButtonElementResourcePath);
                if (value == null)
                {
                    return;
                }

                _leftButtonElementView = Instantiate(value, _leftButtonContainer);
                _leftButtonElementView.name = value.name;
            }

            StretchToParent(_leftButtonElementView.GetComponent<RectTransform>());
            _leftButtonElementView.Clicked -= HandleLeftButtonClicked;
            _leftButtonElementView.Clicked += HandleLeftButtonClicked;
        }

        private void BuildRightButtonElement()
        {
            if (_rightButtonContainer == null)
            {
                return;
            }

            if (_rightButtonElementView == null)
            {
                _rightButtonElementView = _rightButtonContainer.GetComponentInChildren<PillButtonElementView>(true);
            }

            if (_rightButtonElementView == null)
            {
                PillButtonElementView value = Resources.Load<PillButtonElementView>(_pillButtonElementResourcePath);
                if (value == null)
                {
                    return;
                }

                _rightButtonElementView = Instantiate(value, _rightButtonContainer);
                _rightButtonElementView.name = value.name;
            }

            StretchToParent(_rightButtonElementView.GetComponent<RectTransform>());
            _rightButtonElementView.Clicked -= HandleRightButtonClicked;
            _rightButtonElementView.Clicked += HandleRightButtonClicked;
        }

        private void ApplyDefaultState()
        {
            if (_statusTextElementView != null)
            {
                _statusTextElementView.SetValue(_defaultTitleValue, _defaultSubtitleValue);
            }

            if (_leftButtonElementView != null)
            {
                _leftButtonElementView.SetValue(_defaultLeftButtonValue);
                _leftButtonElementView.SetBackgroundColor(_defaultLeftButtonColor);
            }

            if (_rightButtonElementView != null)
            {
                _rightButtonElementView.SetValue(_defaultRightButtonValue);
                _rightButtonElementView.SetBackgroundColor(_defaultRightButtonColor);
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

        private void HandleLeftButtonClicked(PillButtonElementView value)
        {
            LeftButtonClicked?.Invoke(this);
        }

        private void HandleRightButtonClicked(PillButtonElementView value)
        {
            RightButtonClicked?.Invoke(this);
        }

        [ContextMenu("Rebuild View")]
        private void RebuildView()
        {
            BuildView();
        }
    }
}
