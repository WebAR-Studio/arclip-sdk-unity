using System;
using NavigationDemo.UI.Base;
using UnityEngine;
using UnityEngine.UI;

namespace NavigationDemo.UI.Elements
{
    public class SquareIconButtonElementView : BaseViewElement
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Text _iconText;
        [SerializeField] private string _defaultValue = "\u00D7";
        [SerializeField] private Color _defaultBackgroundColor = new Color(0f, 0f, 0f, 0.2f);
        [SerializeField] private Color _defaultIconColor = new Color(0.9647059f, 0.5686275f, 0.5686275f, 1f);

        public event Action<SquareIconButtonElementView> Clicked;

        public string Value => _iconText != null ? _iconText.text : string.Empty;

        protected override void Awake()
        {
            base.Awake();
            if (_button != null)
            {
                _button.onClick.AddListener(HandleClick);
            }

            SetValue(_defaultValue);
            SetBackgroundColor(_defaultBackgroundColor);
            SetIconColor(_defaultIconColor);
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(HandleClick);
            }
        }

        public void SetValue(string value)
        {
            if (_iconText == null)
            {
                return;
            }

            _iconText.text = string.IsNullOrWhiteSpace(value) ? _defaultValue : value;
        }

        public void SetBackgroundColor(Color value)
        {
            if (_backgroundImage == null)
            {
                return;
            }

            _backgroundImage.color = value;
        }

        public Color GetBackgroundColor()
        {
            if (_backgroundImage == null)
            {
                return Color.clear;
            }

            return _backgroundImage.color;
        }

        public void SetIconColor(Color value)
        {
            if (_iconText == null)
            {
                return;
            }

            _iconText.color = value;
        }

        public Color GetIconColor()
        {
            if (_iconText == null)
            {
                return Color.clear;
            }

            return _iconText.color;
        }

        public void SetInteractable(bool value)
        {
            if (_button == null)
            {
                return;
            }

            _button.interactable = value;
        }

        public override string GetValue()
        {
            return Value;
        }

        private void HandleClick()
        {
            Clicked?.Invoke(this);
        }
    }
}
