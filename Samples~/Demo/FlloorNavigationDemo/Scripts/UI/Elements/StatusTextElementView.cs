using NavigationDemo.UI.Base;
using UnityEngine;
using UnityEngine.UI;

namespace NavigationDemo.UI.Elements
{
    public class StatusTextElementView : BaseViewElement
    {
        [SerializeField] private Text _titleText;
        [SerializeField] private Text _subtitleText;
        [SerializeField] private string _defaultTitleValue = "\u0412\u044b \u043d\u0430 \u043c\u0435\u0441\u0442\u0435";
        [SerializeField] private string _defaultSubtitleValue = "12 Storeez";

        public string Value => GetValue();

        protected override void Awake()
        {
            base.Awake();
            SetTitleValue(_defaultTitleValue);
            SetSubtitleValue(_defaultSubtitleValue);
        }

        public void SetTitleValue(string value)
        {
            if (_titleText == null)
            {
                return;
            }

            _titleText.text = string.IsNullOrWhiteSpace(value) ? _defaultTitleValue : value;
        }

        public string GetTitleValue()
        {
            if (_titleText == null)
            {
                return string.Empty;
            }

            return _titleText.text;
        }

        public void SetSubtitleValue(string value)
        {
            if (_subtitleText == null)
            {
                return;
            }

            _subtitleText.text = string.IsNullOrWhiteSpace(value) ? _defaultSubtitleValue : value;
        }

        public string GetSubtitleValue()
        {
            if (_subtitleText == null)
            {
                return string.Empty;
            }

            return _subtitleText.text;
        }

        public void SetValue(string valueTitle, string valueSubtitle)
        {
            SetTitleValue(valueTitle);
            SetSubtitleValue(valueSubtitle);
        }

        public override string GetValue()
        {
            string valueTitle = GetTitleValue();
            string valueSubtitle = GetSubtitleValue();
            return $"{valueTitle} | {valueSubtitle}";
        }
    }
}
