using System;
using NavigationDemo.UI.Base;
using UnityEngine;
using UnityEngine.UI;

namespace NavigationDemo.UI.Elements
{
    public class PillButtonElementView : BaseViewElement
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Text _valueText;
        [SerializeField] private string _defaultValue = "\u041e \u043b\u043e\u043a\u0430\u0446\u0438\u0438";
        [SerializeField] private Color _defaultBackgroundColor = new Color(0f, 0f, 0f, 0.2f);
        [SerializeField] private Color _defaultTextColor = Color.white;

        public event Action<PillButtonElementView> Clicked;

        public string Value => _valueText != null ? _valueText.text : string.Empty;

        protected override void Awake()
        {
            base.Awake();
            if (_button != null)
            {
                _button.onClick.AddListener(HandleClick);
            }

            SetValue(_defaultValue);
            SetBackgroundColor(_defaultBackgroundColor);
            SetTextColor(_defaultTextColor);
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
            if (_valueText == null)
            {
                return;
            }

            _valueText.text = string.IsNullOrWhiteSpace(value) ? _defaultValue : value;
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

        public void SetTextColor(Color value)
        {
            if (_valueText == null)
            {
                return;
            }

            _valueText.color = value;
        }

        public Color GetTextColor()
        {
            if (_valueText == null)
            {
                return Color.clear;
            }

            return _valueText.color;
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
