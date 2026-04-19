using NavigationDemo.UI.Base;
using UnityEngine;
using UnityEngine.UI;

namespace NavigationDemo.UI.Elements
{
    public class CategoryHeaderElementView : BaseViewElement
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private Text _iconText;
        [SerializeField] private Text _valueText;
        [SerializeField] private Sprite _defaultIconSprite;
        [SerializeField] private string _defaultValue = "\u041C\u0430\u0433\u0430\u0437\u0438\u043D\u044B";
        [SerializeField] private string _defaultIcon = "\u2302";

        public string Value => _valueText != null ? _valueText.text : string.Empty;
        public bool HasIconImageReference => _iconImage != null;

        protected override void Awake()
        {
            base.Awake();
            SetIcon(_defaultIcon);
            SetIconSprite(_defaultIconSprite);
            SetValue(_defaultValue);
        }

        public void SetValue(string value)
        {
            if (_valueText == null)
            {
                return;
            }

            _valueText.text = string.IsNullOrWhiteSpace(value) ? _defaultValue : value;
        }

        public void SetIcon(string value)
        {
            if (_iconText == null)
            {
                return;
            }

            _iconText.text = string.IsNullOrWhiteSpace(value) ? _defaultIcon : value;
        }

        public void SetIconSprite(Sprite value)
        {
            Image valueImage = EnsureIconImage();
            Sprite valueSprite = value != null ? value : _defaultIconSprite;

            if (valueImage != null)
            {
                valueImage.sprite = valueSprite;
                valueImage.preserveAspect = true;
                valueImage.raycastTarget = false;
                valueImage.enabled = valueSprite != null;
            }

            bool valueShowFallbackText = valueImage == null || valueImage.sprite == null;
            SyncLegacyIconTextVisibility(valueShowFallbackText);
        }

        public override string GetValue()
        {
            return Value;
        }

        private Image EnsureIconImage()
        {
            if (_iconImage != null)
            {
                return _iconImage;
            }

            Image[] valueImages = GetComponentsInChildren<Image>(true);
            for (int index = 0; index < valueImages.Length; index++)
            {
                Image valueImage = valueImages[index];
                if (valueImage == null)
                {
                    continue;
                }

                if (!valueImage.gameObject.name.Contains("Icon"))
                {
                    continue;
                }

                _iconImage = valueImage;
                return _iconImage;
            }

            if (_iconText == null)
            {
                return null;
            }

            _iconImage = _iconText.GetComponent<Image>();
            if (_iconImage == null)
            {
                _iconImage = _iconText.gameObject.AddComponent<Image>();
            }

            _iconImage.color = _iconText.color;
            return _iconImage;
        }

        private void SyncLegacyIconTextVisibility(bool valueVisible)
        {
            Text[] valueTexts = GetComponentsInChildren<Text>(true);
            for (int index = 0; index < valueTexts.Length; index++)
            {
                Text valueText = valueTexts[index];
                if (valueText == null || valueText == _valueText)
                {
                    continue;
                }

                if (!valueText.gameObject.name.Contains("Icon"))
                {
                    continue;
                }

                valueText.enabled = valueVisible;
            }
        }
    }
}
