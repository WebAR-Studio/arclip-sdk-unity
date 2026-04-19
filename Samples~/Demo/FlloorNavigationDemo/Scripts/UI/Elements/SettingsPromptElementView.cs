using System;
using NavigationDemo.UI.Base;
using UnityEngine;
using UnityEngine.UI;

namespace NavigationDemo.UI.Elements
{
    public class SettingsPromptElementView : BaseViewElement
    {
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Text _iconText;
        [SerializeField] private Text _titleText;
        [SerializeField] private Text _descriptionText;
        [SerializeField] private Button _openSettingsButton;
        [SerializeField] private Text _openSettingsButtonText;
        [SerializeField] private Sprite _defaultIconSprite;
        [SerializeField] private string _defaultIconValue = "\u26A0";
        [SerializeField] private string _defaultTitleValue = "\u041D\u0435\u0442 \u043F\u043E\u0434\u043A\u043B\u044E\u0447\u0435\u043D\u0438\u044F \u043A \u0438\u043D\u0442\u0435\u0440\u043D\u0435\u0442\u0443";
        [SerializeField] [TextArea(2, 8)] private string _defaultDescriptionValue =
            "\u0414\u043B\u044F \u043A\u043E\u0440\u0440\u0435\u043A\u0442\u043D\u043E\u0439 \u0440\u0430\u0431\u043E\u0442\u044B \u043F\u0440\u0438\u043B\u043E\u0436\u0435\u043D\u0438\u044E \u0442\u0440\u0435\u0431\u0443\u0435\u0442\u0441\u044F \u0434\u043E\u0441\u0442\u0443\u043F \u043A \u0438\u043D\u0442\u0435\u0440\u043D\u0435\u0442\u0443.\n" +
            "\u041F\u0440\u043E\u0432\u0435\u0440\u044C\u0442\u0435 \u043F\u043E\u0434\u043A\u043B\u044E\u0447\u0435\u043D\u0438\u0435 \u043A \u0441\u0435\u0442\u0438 \u0438 \u043F\u043E\u043F\u0440\u043E\u0431\u0443\u0439\u0442\u0435 \u0441\u043D\u043E\u0432\u0430.";
        [SerializeField] private string _defaultButtonValue = "\u041E\u0442\u043A\u0440\u044B\u0442\u044C \u043D\u0430\u0441\u0442\u0440\u043E\u0439\u043A\u0438";
        [SerializeField] private Color _defaultBackgroundColor = new Color(0f, 0f, 0f, 0.32f);
        [SerializeField] private Color _defaultTitleColor = Color.white;
        [SerializeField] private Color _defaultDescriptionColor = Color.white;
        [SerializeField] private Color _defaultButtonTextColor = new Color(0.64705884f, 0.88235295f, 1f, 1f);

        public event Action<SettingsPromptElementView> OpenSettingsClicked;

        public string Value => GetTitleValue();

        protected override void Awake()
        {
            base.Awake();

            if (_openSettingsButton != null)
            {
                _openSettingsButton.onClick.AddListener(HandleOpenSettingsClicked);
            }

            ApplyDefaultState();
        }

        private void OnDestroy()
        {
            if (_openSettingsButton != null)
            {
                _openSettingsButton.onClick.RemoveListener(HandleOpenSettingsClicked);
            }
        }

        public void SetContent(
            Sprite valueIconSprite,
            string valueIconFallback,
            string valueTitle,
            string valueDescription,
            string valueButton)
        {
            SetIconValue(valueIconFallback);
            SetIconSprite(valueIconSprite);
            SetTitleValue(valueTitle);
            SetDescriptionValue(valueDescription);
            SetButtonValue(valueButton);
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

        public void SetIconSprite(Sprite value)
        {
            bool valueHasSprite = value != null;

            if (_iconImage != null)
            {
                _iconImage.sprite = value;
                _iconImage.enabled = valueHasSprite;
            }

            if (_iconText != null)
            {
                _iconText.gameObject.SetActive(!valueHasSprite);
            }
        }

        public Sprite GetIconSprite()
        {
            if (_iconImage == null)
            {
                return null;
            }

            return _iconImage.sprite;
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

        public void SetDescriptionValue(string value)
        {
            if (_descriptionText == null)
            {
                return;
            }

            _descriptionText.text = string.IsNullOrWhiteSpace(value) ? _defaultDescriptionValue : value;
        }

        public string GetDescriptionValue()
        {
            if (_descriptionText == null)
            {
                return string.Empty;
            }

            return _descriptionText.text;
        }

        public void SetButtonValue(string value)
        {
            if (_openSettingsButtonText == null)
            {
                return;
            }

            _openSettingsButtonText.text = string.IsNullOrWhiteSpace(value) ? _defaultButtonValue : value;
        }

        public string GetButtonValue()
        {
            if (_openSettingsButtonText == null)
            {
                return string.Empty;
            }

            return _openSettingsButtonText.text;
        }

        public void SetButtonInteractable(bool value)
        {
            if (_openSettingsButton == null)
            {
                return;
            }

            _openSettingsButton.interactable = value;
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

        private void ApplyDefaultState()
        {
            SetIconValue(_defaultIconValue);
            SetIconSprite(_defaultIconSprite);
            SetTitleValue(_defaultTitleValue);
            SetDescriptionValue(_defaultDescriptionValue);
            SetButtonValue(_defaultButtonValue);
            SetBackgroundColor(_defaultBackgroundColor);

            if (_titleText != null)
            {
                _titleText.color = _defaultTitleColor;
            }

            if (_descriptionText != null)
            {
                _descriptionText.color = _defaultDescriptionColor;
            }

            if (_openSettingsButtonText != null)
            {
                _openSettingsButtonText.color = _defaultButtonTextColor;
            }
        }

        private void HandleOpenSettingsClicked()
        {
            OpenSettingsClicked?.Invoke(this);
        }
    }
}
