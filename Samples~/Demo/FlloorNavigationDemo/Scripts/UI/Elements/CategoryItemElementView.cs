using System;
using NavigationDemo.UI.Base;
using UnityEngine;
using UnityEngine.UI;

namespace NavigationDemo.UI.Elements
{
    public class CategoryItemElementView : BaseViewElement
    {
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Text _iconText;
        [SerializeField] private Text _titleText;
        [SerializeField] private Image _arrowImage;
        [SerializeField] private Text _arrowText;
        [SerializeField] private Image _dividerImage;
        [SerializeField] private Button _button;
        [SerializeField] private Sprite _defaultIconSprite;
        [SerializeField] private Sprite _defaultArrowSprite;
        [SerializeField] private string _defaultValue = "\u041A\u0430\u0442\u0435\u0433\u043E\u0440\u0438\u044F";
        [SerializeField] private string _defaultIcon = "\u2022";
        [SerializeField] private string _arrowValue = "\u203A";
        [SerializeField] private bool _showDivider = true;

        public event Action<CategoryItemElementView> Clicked;

        public string Value => _titleText != null ? _titleText.text : string.Empty;
        public bool HasImageIconReferences => _iconImage != null && _arrowImage != null;

        protected override void Awake()
        {
            base.Awake();
            if (_button != null)
            {
                _button.onClick.AddListener(HandleClick);
            }

            string valueTitle = _titleText != null ? _titleText.text : _defaultValue;
            string valueIcon = _iconText != null ? _iconText.text : _defaultIcon;
            Sprite valueIconSprite = _iconImage != null ? _iconImage.sprite : _defaultIconSprite;
            string valueArrow = _arrowText != null ? _arrowText.text : _arrowValue;
            Sprite valueArrowSprite = _arrowImage != null ? _arrowImage.sprite : _defaultArrowSprite;
            bool valueDividerVisible = _dividerImage != null
                ? _dividerImage.gameObject.activeSelf
                : _showDivider;

            SetValue(valueTitle);
            SetIcon(valueIcon);
            SetIconSprite(valueIconSprite);
            SetArrow(valueArrow);
            SetArrowSprite(valueArrowSprite);
            SetDividerVisible(valueDividerVisible);
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
            if (_titleText == null)
            {
                return;
            }

            _titleText.text = string.IsNullOrWhiteSpace(value) ? _defaultValue : value;
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

            if (_iconText != null)
            {
                _iconText.enabled = valueImage == null || valueImage.sprite == null;
            }
        }

        public void SetArrow(string value)
        {
            if (_arrowText == null)
            {
                return;
            }

            _arrowText.text = string.IsNullOrWhiteSpace(value) ? _arrowValue : value;
        }

        public void SetArrowSprite(Sprite value)
        {
            Image valueImage = EnsureArrowImage();
            Sprite valueSprite = value != null ? value : _defaultArrowSprite;

            if (valueImage != null)
            {
                valueImage.sprite = valueSprite;
                valueImage.preserveAspect = true;
                valueImage.raycastTarget = false;
                valueImage.enabled = valueSprite != null;
            }

            if (_arrowText != null)
            {
                _arrowText.enabled = valueImage == null || valueImage.sprite == null;
            }
        }

        public void SetDividerVisible(bool value)
        {
            _showDivider = value;
            if (_dividerImage == null)
            {
                return;
            }

            _dividerImage.gameObject.SetActive(value);
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

        private Image EnsureIconImage()
        {
            if (_iconImage != null)
            {
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

        private Image EnsureArrowImage()
        {
            if (_arrowImage != null)
            {
                return _arrowImage;
            }

            if (_arrowText == null)
            {
                return null;
            }

            _arrowImage = _arrowText.GetComponent<Image>();
            if (_arrowImage == null)
            {
                _arrowImage = _arrowText.gameObject.AddComponent<Image>();
            }

            _arrowImage.color = _arrowText.color;
            return _arrowImage;
        }
    }
}
