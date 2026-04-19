using NavigationDemo.UI.Base;
using UnityEngine;
using UnityEngine.UI;

namespace NavigationDemo.UI.Elements
{
    public class StoreScheduleStickerElementView : BaseViewElement
    {
        [SerializeField] private Text _iconText;
        [SerializeField] private Text _valueText;
        [SerializeField] private string _defaultIconValue = "?";
        [SerializeField] private string _defaultValue = "10:00-22:00";

        public string Value => GetValue();

        protected override void Awake()
        {
            base.Awake();
            SetIconValue(_defaultIconValue);
            SetValue(_defaultValue);
        }

        public void SetIconValue(string value)
        {
            if (_iconText == null)
            {
                return;
            }

            _iconText.text = string.IsNullOrWhiteSpace(value) ? _defaultIconValue : value;
        }

        public string GetIconValue()
        {
            if (_iconText == null)
            {
                return string.Empty;
            }

            return _iconText.text;
        }

        public void SetValue(string value)
        {
            if (_valueText == null)
            {
                return;
            }

            _valueText.text = string.IsNullOrWhiteSpace(value) ? _defaultValue : value;
        }

        public override string GetValue()
        {
            if (_valueText == null)
            {
                return string.Empty;
            }

            return _valueText.text;
        }
    }
}