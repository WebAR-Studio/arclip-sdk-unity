using NavigationDemo.UI.Base;
using UnityEngine;
using UnityEngine.UI;

namespace NavigationDemo.UI.Elements
{
    public class StoreDetailsHeaderElementView : BaseViewElement
    {
        [SerializeField] private Text _titleText;
        [SerializeField] private Text _subtitleText;
        [SerializeField] private Text _categoriesText;
        [SerializeField] private string _defaultTitleValue = "12 STOREEZ";
        [SerializeField] private string _defaultSubtitleValue = "Этаж 3, сектор 23";
        [SerializeField] private string _defaultCategoriesValue = "Одежда, Женская одежда, Мужская одежда, Аксессуары, Обувь, Локальные бренды";

        public string Value => GetTitleValue();

        protected override void Awake()
        {
            base.Awake();
            SetValue(_defaultTitleValue, _defaultSubtitleValue, _defaultCategoriesValue);
        }

        public void SetValue(string valueTitle, string valueSubtitle, string valueCategories)
        {
            SetTitleValue(valueTitle);
            SetSubtitleValue(valueSubtitle);
            SetCategoriesValue(valueCategories);
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

        public void SetCategoriesValue(string value)
        {
            if (_categoriesText == null)
            {
                return;
            }

            _categoriesText.text = string.IsNullOrWhiteSpace(value) ? _defaultCategoriesValue : value;
        }

        public string GetCategoriesValue()
        {
            if (_categoriesText == null)
            {
                return string.Empty;
            }

            return _categoriesText.text;
        }

        public override string GetValue()
        {
            return Value;
        }
    }
}