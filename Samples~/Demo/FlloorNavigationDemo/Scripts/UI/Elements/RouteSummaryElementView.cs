using NavigationDemo.UI.Base;
using UnityEngine;
using UnityEngine.UI;

namespace NavigationDemo.UI.Elements
{
    public class RouteSummaryElementView : BaseViewElement
    {
        [SerializeField] private Text _primaryValueText;
        [SerializeField] private Text _secondaryValueText;
        [SerializeField] private Text _captionValueText;
        [SerializeField] private string _defaultPrimaryValue = "5 \u043C\u0438\u043D";
        [SerializeField] private string _defaultSecondaryValue = "10 \u043C";
        [SerializeField] private string _defaultCaptionValue = "12 Storeez";

        public string Value => GetSummaryValue();

        protected override void Awake()
        {
            base.Awake();
            SetPrimaryValue(_defaultPrimaryValue);
            SetSecondaryValue(_defaultSecondaryValue);
            SetCaptionValue(_defaultCaptionValue);
        }

        public void SetPrimaryValue(string value)
        {
            if (_primaryValueText == null)
            {
                return;
            }

            _primaryValueText.text = string.IsNullOrWhiteSpace(value) ? _defaultPrimaryValue : value;
        }

        public string GetPrimaryValue()
        {
            if (_primaryValueText == null)
            {
                return string.Empty;
            }

            return _primaryValueText.text;
        }

        public void SetSecondaryValue(string value)
        {
            if (_secondaryValueText == null)
            {
                return;
            }

            _secondaryValueText.text = string.IsNullOrWhiteSpace(value) ? _defaultSecondaryValue : value;
        }

        public string GetSecondaryValue()
        {
            if (_secondaryValueText == null)
            {
                return string.Empty;
            }

            return _secondaryValueText.text;
        }

        public void SetCaptionValue(string value)
        {
            if (_captionValueText == null)
            {
                return;
            }

            _captionValueText.text = string.IsNullOrWhiteSpace(value) ? _defaultCaptionValue : value;
        }

        public string GetCaptionValue()
        {
            if (_captionValueText == null)
            {
                return string.Empty;
            }

            return _captionValueText.text;
        }

        public void SetSummary(string valuePrimary, string valueSecondary, string valueCaption)
        {
            SetPrimaryValue(valuePrimary);
            SetSecondaryValue(valueSecondary);
            SetCaptionValue(valueCaption);
        }

        public string GetSummaryValue()
        {
            string valuePrimary = GetPrimaryValue();
            string valueSecondary = GetSecondaryValue();
            string valueCaption = GetCaptionValue();
            return $"{valuePrimary} | {valueSecondary} | {valueCaption}";
        }

        public override string GetValue()
        {
            return Value;
        }
    }
}
