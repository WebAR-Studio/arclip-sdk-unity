using System;
using NavigationDemo.UI.Base;
using UnityEngine;
using UnityEngine.UI;

namespace NavigationDemo.UI.Elements
{
    public class StoreCardElementView : BaseViewElement
    {
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _thumbnailImage;
        [SerializeField] private Text _thumbnailText;
        [SerializeField] private Text _valueText;
        [SerializeField] private Text _descriptionText;
        [SerializeField] private Image _trailingIconImage;
        [SerializeField] private Text _trailingIconText;
        [SerializeField] private Button _button;
        [SerializeField] private Sprite _defaultTrailingIconSprite;
        [SerializeField] private string _defaultValue = "12 STOREEZ";
        [SerializeField] private string _defaultDescription = "\u041E\u0434\u0435\u0436\u0434\u0430, \u0416\u0435\u043D\u0441\u043A\u0430\u044F \u043E\u0434\u0435\u0436\u0434\u0430, \u041C\u0443\u0436..";
        [SerializeField] private string _descriptionEllipsis = "...";
        [SerializeField] private string _defaultTrailingIcon = "\u24D8";
        [SerializeField] private bool _showTrailingIcon;

        private string _descriptionValue = string.Empty;

        public event Action<StoreCardElementView> Clicked;

        public string Value => _valueText != null ? _valueText.text : string.Empty;
        public bool HasTrailingIconImageReference => _trailingIconImage != null;

        protected override void Awake()
        {
            base.Awake();
            if (_button != null)
            {
                _button.onClick.AddListener(HandleClick);
            }

            SetValue(_defaultValue);
            SetDescription(_defaultDescription);
            SetThumbnail(null);
            SetThumbnailText(_defaultValue);
            SetTrailingIcon(_defaultTrailingIcon);
            SetTrailingIconSprite(_defaultTrailingIconSprite);
            SetTrailingIconVisible(_showTrailingIcon);
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(HandleClick);
            }
        }

        private void OnRectTransformDimensionsChange()
        {
            ApplyDescriptionWithEllipsis();
        }

        public void SetValue(string value)
        {
            if (_valueText == null)
            {
                return;
            }

            _valueText.text = string.IsNullOrWhiteSpace(value) ? _defaultValue : value;
        }

        public void SetDescription(string value)
        {
            if (_descriptionText == null)
            {
                return;
            }

            _descriptionValue = string.IsNullOrWhiteSpace(value) ? _defaultDescription : value;
            ApplyDescriptionWithEllipsis();
        }

        public string GetDescription()
        {
            if (_descriptionText == null)
            {
                return string.Empty;
            }

            return _descriptionText.text;
        }

        public void SetThumbnail(Sprite value)
        {
            if (_thumbnailImage == null)
            {
                return;
            }

            _thumbnailImage.sprite = value;
            _thumbnailImage.enabled = value != null;
        }

        public void SetThumbnailText(string value)
        {
            if (_thumbnailText == null)
            {
                return;
            }

            _thumbnailText.text = string.IsNullOrWhiteSpace(value) ? _defaultValue : value;
        }

        public void SetTrailingIcon(string value)
        {
            if (_trailingIconText == null)
            {
                return;
            }

            _trailingIconText.text = string.IsNullOrWhiteSpace(value) ? _defaultTrailingIcon : value;
        }

        public void SetTrailingIconSprite(Sprite value)
        {
            Image valueImage = EnsureTrailingIconImage();
            Sprite valueSprite = value != null ? value : _defaultTrailingIconSprite;

            if (valueImage != null)
            {
                valueImage.sprite = valueSprite;
                valueImage.preserveAspect = true;
                valueImage.raycastTarget = false;
                valueImage.enabled = valueSprite != null;
            }

            bool valueShowFallbackText = valueImage == null || valueImage.sprite == null;
            SyncLegacyTrailingIconTextVisibility(valueShowFallbackText);
        }

        public void SetTrailingIconVisible(bool value)
        {
            _showTrailingIcon = value;

            if (_trailingIconText != null)
            {
                _trailingIconText.gameObject.SetActive(value);
            }

            if (_trailingIconImage != null)
            {
                _trailingIconImage.gameObject.SetActive(value);
            }
        }

        public void SetInteractable(bool value)
        {
            if (_button == null)
            {
                return;
            }

            _button.interactable = value;
        }

        public void SetBackgroundColor(Color value)
        {
            if (_backgroundImage == null)
            {
                return;
            }

            _backgroundImage.color = value;
        }

        public override string GetValue()
        {
            return Value;
        }

        private void HandleClick()
        {
            Clicked?.Invoke(this);
        }

        private Image EnsureTrailingIconImage()
        {
            if (_trailingIconImage != null)
            {
                return _trailingIconImage;
            }

            if (_trailingIconText != null)
            {
                _trailingIconImage = _trailingIconText.GetComponent<Image>();
                if (_trailingIconImage == null && _trailingIconText.gameObject != null)
                {
                    _trailingIconImage = _trailingIconText.gameObject.AddComponent<Image>();
                }

                if (_trailingIconImage != null)
                {
                    _trailingIconImage.color = _trailingIconText.color;
                    return _trailingIconImage;
                }
            }

            Image[] valueImages = GetComponentsInChildren<Image>(true);
            for (int index = 0; index < valueImages.Length; index++)
            {
                Image valueImage = valueImages[index];
                if (valueImage == null || valueImage == _backgroundImage || valueImage == _thumbnailImage)
                {
                    continue;
                }

                if (!valueImage.gameObject.name.Contains("TrailingIcon"))
                {
                    continue;
                }

                _trailingIconImage = valueImage;
                break;
            }

            return _trailingIconImage;
        }

        private void ApplyDescriptionWithEllipsis()
        {
            if (_descriptionText == null)
            {
                return;
            }

            string valueText = string.IsNullOrWhiteSpace(_descriptionValue) ? _defaultDescription : _descriptionValue;
            if (string.IsNullOrEmpty(valueText))
            {
                _descriptionText.text = string.Empty;
                return;
            }

            RectTransform valueRectTransform = _descriptionText.rectTransform;
            if (valueRectTransform == null)
            {
                _descriptionText.text = valueText;
                return;
            }

            float valueAvailableWidth = valueRectTransform.rect.width;
            if (valueAvailableWidth <= 0f || DoesDescriptionFit(valueText, valueAvailableWidth))
            {
                _descriptionText.text = valueText;
                return;
            }

            string valueEllipsis = string.IsNullOrEmpty(_descriptionEllipsis) ? "..." : _descriptionEllipsis;
            if (!DoesDescriptionFit(valueEllipsis, valueAvailableWidth))
            {
                _descriptionText.text = string.Empty;
                return;
            }

            int valueMin = 0;
            int valueMax = valueText.Length;
            while (valueMin < valueMax)
            {
                int valueMid = (valueMin + valueMax + 1) / 2;
                string valueCandidate = valueText.Substring(0, valueMid).TrimEnd() + valueEllipsis;
                if (DoesDescriptionFit(valueCandidate, valueAvailableWidth))
                {
                    valueMin = valueMid;
                }
                else
                {
                    valueMax = valueMid - 1;
                }
            }

            _descriptionText.text = valueText.Substring(0, valueMin).TrimEnd() + valueEllipsis;
        }

        private bool DoesDescriptionFit(string value, float availableWidth)
        {
            if (string.IsNullOrEmpty(value))
            {
                return true;
            }

            TextGenerationSettings valueGenerationSettings = _descriptionText.GetGenerationSettings(new Vector2(availableWidth, _descriptionText.rectTransform.rect.height));
            float valuePreferredWidth = _descriptionText.cachedTextGeneratorForLayout.GetPreferredWidth(value, valueGenerationSettings)
                / _descriptionText.pixelsPerUnit;
            return valuePreferredWidth <= availableWidth + 0.01f;
        }

        private void SyncLegacyTrailingIconTextVisibility(bool valueVisible)
        {
            Text[] valueTexts = GetComponentsInChildren<Text>(true);
            for (int index = 0; index < valueTexts.Length; index++)
            {
                Text valueText = valueTexts[index];
                if (valueText == null || valueText == _valueText || valueText == _descriptionText || valueText == _thumbnailText)
                {
                    continue;
                }

                if (!valueText.gameObject.name.Contains("TrailingIcon"))
                {
                    continue;
                }

                valueText.enabled = valueVisible;
            }
        }
    }
}
