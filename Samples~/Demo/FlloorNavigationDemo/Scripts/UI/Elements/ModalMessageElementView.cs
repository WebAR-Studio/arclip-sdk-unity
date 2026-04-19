using NavigationDemo.UI.Base;
using UnityEngine;
using UnityEngine.UI;

namespace NavigationDemo.UI.Elements
{
    public class ModalMessageElementView : BaseViewElement
    {
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Text _valueText;
        [SerializeField] private string _defaultValue = "\u0417\u0430\u043f\u043e\u043b\u043d\u0438 \u043a\u0440\u0443\u0433\u0438 \u0432\u043e\u043a\u0440\u0443\u0433 \u0441\u0435\u0431\u044f";
        [SerializeField] private Color _defaultBackgroundColor = new Color(0f, 0f, 0f, 0.32f);

        public string Value => _valueText != null ? _valueText.text : string.Empty;

        protected override void Awake()
        {
            base.Awake();
            SetValue(_defaultValue);
            SetBackgroundColor(_defaultBackgroundColor);
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

        public override string GetValue()
        {
            return Value;
        }
    }
}
