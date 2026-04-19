using System;
using NavigationDemo.UI.Base;
using NavigationDemo.UI.Elements;
using UnityEngine;

namespace NavigationDemo.UI.Views
{
    [ExecuteAlways]
    public class RouteSummaryBarView : BaseView
    {
        [SerializeField] private RectTransform _leftButtonContainer;
        [SerializeField] private RectTransform _summaryContainer;
        [SerializeField] private RectTransform _rightButtonContainer;
        [SerializeField] private string _squareIconButtonElementResourcePath = "UI/Elements/SquareIconButtonElement";
        [SerializeField] private string _routeSummaryElementResourcePath = "UI/Elements/RouteSummaryElement";
        [SerializeField] private bool _rebuildInEditMode = true;
        [SerializeField] private bool _showRightButton;
        [SerializeField] private string _defaultLeftIconValue = "\u00D7";
        [SerializeField] private string _defaultRightIconValue = "\u2316";
        [SerializeField] private string _defaultPrimaryValue = "5 \u043C\u0438\u043D";
        [SerializeField] private string _defaultSecondaryValue = "10 \u043C";
        [SerializeField] private string _defaultCaptionValue = "12 Storeez";
        [SerializeField] private Color _defaultLeftIconColor = new Color(0.9647059f, 0.5686275f, 0.5686275f, 1f);
        [SerializeField] private Color _defaultRightIconColor = Color.white;

        private SquareIconButtonElementView _leftButtonElementView;
        private RouteSummaryElementView _routeSummaryElementView;
        private SquareIconButtonElementView _rightButtonElementView;

        public event Action<RouteSummaryBarView> LeftButtonClicked;
        public event Action<RouteSummaryBarView> RightButtonClicked;

        public string Value => GetSummaryValue();

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

        public string GetSummaryValue()
        {
            if (_routeSummaryElementView == null)
            {
                return string.Empty;
            }

            return _routeSummaryElementView.GetSummaryValue();
        }

        public void SetSummary(string valuePrimary, string valueSecondary, string valueCaption)
        {
            if (_routeSummaryElementView == null)
            {
                return;
            }

            _routeSummaryElementView.SetSummary(valuePrimary, valueSecondary, valueCaption);
        }

        public string GetPrimaryValue()
        {
            if (_routeSummaryElementView == null)
            {
                return string.Empty;
            }

            return _routeSummaryElementView.GetPrimaryValue();
        }

        public void SetPrimaryValue(string value)
        {
            if (_routeSummaryElementView == null)
            {
                return;
            }

            _routeSummaryElementView.SetPrimaryValue(value);
        }

        public string GetSecondaryValue()
        {
            if (_routeSummaryElementView == null)
            {
                return string.Empty;
            }

            return _routeSummaryElementView.GetSecondaryValue();
        }

        public void SetSecondaryValue(string value)
        {
            if (_routeSummaryElementView == null)
            {
                return;
            }

            _routeSummaryElementView.SetSecondaryValue(value);
        }

        public string GetCaptionValue()
        {
            if (_routeSummaryElementView == null)
            {
                return string.Empty;
            }

            return _routeSummaryElementView.GetCaptionValue();
        }

        public void SetCaptionValue(string value)
        {
            if (_routeSummaryElementView == null)
            {
                return;
            }

            _routeSummaryElementView.SetCaptionValue(value);
        }

        public string GetLeftIconValue()
        {
            if (_leftButtonElementView == null)
            {
                return string.Empty;
            }

            return _leftButtonElementView.GetValue();
        }

        public void SetLeftIconValue(string value)
        {
            if (_leftButtonElementView == null)
            {
                return;
            }

            _leftButtonElementView.SetValue(value);
        }

        public string GetRightIconValue()
        {
            if (_rightButtonElementView == null)
            {
                return string.Empty;
            }

            return _rightButtonElementView.GetValue();
        }

        public void SetRightIconValue(string value)
        {
            if (_rightButtonElementView == null)
            {
                return;
            }

            _rightButtonElementView.SetValue(value);
        }

        public void SetRightButtonVisible(bool value)
        {
            _showRightButton = value;
            if (_rightButtonElementView == null)
            {
                return;
            }

            if (value)
            {
                _rightButtonElementView.Show();
                return;
            }

            _rightButtonElementView.Hide();
        }

        private void BuildView()
        {
            BuildLeftButtonElement();
            BuildRouteSummaryElement();
            BuildRightButtonElement();
            ApplyDefaultState();
        }

        private void BuildLeftButtonElement()
        {
            if (_leftButtonContainer == null)
            {
                return;
            }

            if (_leftButtonElementView == null)
            {
                _leftButtonElementView = _leftButtonContainer.GetComponentInChildren<SquareIconButtonElementView>(true);
            }

            if (_leftButtonElementView == null)
            {
                SquareIconButtonElementView value = Resources.Load<SquareIconButtonElementView>(_squareIconButtonElementResourcePath);
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

        private void BuildRouteSummaryElement()
        {
            if (_summaryContainer == null)
            {
                return;
            }

            if (_routeSummaryElementView == null)
            {
                _routeSummaryElementView = _summaryContainer.GetComponentInChildren<RouteSummaryElementView>(true);
            }

            if (_routeSummaryElementView == null)
            {
                RouteSummaryElementView value = Resources.Load<RouteSummaryElementView>(_routeSummaryElementResourcePath);
                if (value == null)
                {
                    return;
                }

                _routeSummaryElementView = Instantiate(value, _summaryContainer);
                _routeSummaryElementView.name = value.name;
            }

            StretchToParent(_routeSummaryElementView.GetComponent<RectTransform>());
        }

        private void BuildRightButtonElement()
        {
            if (_rightButtonContainer == null)
            {
                return;
            }

            if (_rightButtonElementView == null)
            {
                _rightButtonElementView = _rightButtonContainer.GetComponentInChildren<SquareIconButtonElementView>(true);
            }

            if (_rightButtonElementView == null)
            {
                SquareIconButtonElementView value = Resources.Load<SquareIconButtonElementView>(_squareIconButtonElementResourcePath);
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
            if (_leftButtonElementView != null)
            {
                _leftButtonElementView.SetValue(_defaultLeftIconValue);
                _leftButtonElementView.SetIconColor(_defaultLeftIconColor);
            }

            if (_routeSummaryElementView != null)
            {
                _routeSummaryElementView.SetSummary(_defaultPrimaryValue, _defaultSecondaryValue, _defaultCaptionValue);
            }

            if (_rightButtonElementView != null)
            {
                _rightButtonElementView.SetValue(_defaultRightIconValue);
                _rightButtonElementView.SetIconColor(_defaultRightIconColor);
                SetRightButtonVisible(_showRightButton);
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

        private void HandleLeftButtonClicked(SquareIconButtonElementView value)
        {
            LeftButtonClicked?.Invoke(this);
        }

        private void HandleRightButtonClicked(SquareIconButtonElementView value)
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
