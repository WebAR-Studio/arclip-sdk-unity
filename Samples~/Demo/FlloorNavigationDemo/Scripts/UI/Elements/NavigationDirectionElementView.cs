using NavigationDemo.UI.Base;
using UnityEngine;
using UnityEngine.UI;

namespace NavigationDemo.UI.Elements
{
    public class NavigationDirectionElementView : BaseViewElement
    {
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Text _iconText;
        [SerializeField] private Text _titleText;
        [SerializeField] private Text _distanceText;
        [SerializeField] private string _defaultIconValue = "\u2192";
        [SerializeField] private string _defaultTitleValue = "\u041f\u043e\u0432\u0435\u0440\u043d\u0438\u0442\u0435 \u043d\u0430\u043f\u0440\u0430\u0432\u043e";
        [SerializeField] private string _defaultDistanceValue = "5\u043c";
        [SerializeField] private Color _defaultBackgroundColor = new Color(0f, 0f, 0f, 0.2f);
        [SerializeField] private Color _defaultIconColor = new Color(0.29803923f, 0.45490196f, 0.9647059f, 1f);

        public string Value => GetDirectionValue();

        protected override void Awake()
        {
            base.Awake();
            SetDirection(null, _defaultIconValue, _defaultTitleValue, _defaultDistanceValue);
            SetBackgroundColor(_defaultBackgroundColor);
            SetIconColor(_defaultIconColor);
        }

        public void SetDirection(string valueIcon, string valueTitle, string valueDistance)
        {
            SetDirection(null, valueIcon, valueTitle, valueDistance);
        }

        public void SetDirection(Sprite valueIconSprite, string valueIconFallback, string valueTitle, string valueDistance)
        {
            SetIcon(valueIconSprite, valueIconFallback);
            SetTitleValue(valueTitle);
            SetDistanceValue(valueDistance);
        }

        public void SetIcon(Sprite valueSprite, string valueFallback)
        {
            SetIconValue(valueFallback);
            SetIconSprite(valueSprite);
        }

        public void SetIconSprite(Sprite value)
        {
            bool hasSprite = value != null;

            if (_iconImage != null)
            {
                _iconImage.sprite = value;
                _iconImage.enabled = hasSprite;
            }

            if (_iconText != null)
            {
                _iconText.gameObject.SetActive(!hasSprite);
            }
        }

        public Sprite GetIconSprite()
        {
            return _iconImage == null ? null : _iconImage.sprite;
        }

        public string GetDirectionValue()
        {
            string valueTitle = GetTitleValue();
            string valueDistance = GetDistanceValue();
            return $"{valueTitle} | {valueDistance}";
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

        public void SetDistanceValue(string value)
        {
            if (_distanceText == null)
            {
                return;
            }

            _distanceText.text = string.IsNullOrWhiteSpace(value) ? _defaultDistanceValue : value;
        }

        public string GetDistanceValue()
        {
            if (_distanceText == null)
            {
                return string.Empty;
            }

            return _distanceText.text;
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

        public override string GetValue()
        {
            return Value;
        }
    }
}
