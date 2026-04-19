using NavigationDemo.UI.Base;
using UnityEngine;
using UnityEngine.UI;

namespace NavigationDemo.UI.Elements
{
    public class LoadingBrandElementView : BaseViewElement
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private Text _arText;
        [SerializeField] private Text _navigationText;
        [SerializeField] private Sprite _defaultIconSprite;
        [SerializeField] private string _defaultArValue = "AR";
        [SerializeField] private string _defaultNavigationValue = "Navigation";

        public string Value => GetValue();

        protected override void Awake()
        {
            base.Awake();
            Sprite valueIcon = _defaultIconSprite;
            if (valueIcon == null && _iconImage != null)
            {
                valueIcon = _iconImage.sprite;
            }

            SetIconSprite(valueIcon);
            SetArValue(_defaultArValue);
            SetNavigationValue(_defaultNavigationValue);
        }

        public void SetIconSprite(Sprite value)
        {
            if (_iconImage == null)
            {
                return;
            }

            _iconImage.sprite = value != null ? value : _defaultIconSprite;
            _iconImage.enabled = _iconImage.sprite != null;
        }

        public Sprite GetIconSprite()
        {
            if (_iconImage == null)
            {
                return null;
            }

            return _iconImage.sprite;
        }

        public void SetIconValue(string value)
        {
            SetIconSprite(_defaultIconSprite);
        }

        public string GetIconValue()
        {
            Sprite value = GetIconSprite();
            return value != null ? value.name : string.Empty;
        }

        public void SetArValue(string value)
        {
            if (_arText == null)
            {
                return;
            }

            _arText.text = string.IsNullOrWhiteSpace(value) ? _defaultArValue : value;
        }

        public string GetArValue()
        {
            if (_arText == null)
            {
                return string.Empty;
            }

            return _arText.text;
        }

        public void SetNavigationValue(string value)
        {
            if (_navigationText == null)
            {
                return;
            }

            _navigationText.text = string.IsNullOrWhiteSpace(value) ? _defaultNavigationValue : value;
        }

        public string GetNavigationValue()
        {
            if (_navigationText == null)
            {
                return string.Empty;
            }

            return _navigationText.text;
        }

        public override string GetValue()
        {
            string valueAr = GetArValue();
            string valueNavigation = GetNavigationValue();
            return $"{valueAr} {valueNavigation}";
        }
    }
}
