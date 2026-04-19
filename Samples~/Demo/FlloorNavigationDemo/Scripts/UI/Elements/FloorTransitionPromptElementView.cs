using System;
using NavigationDemo.UI.Base;
using UnityEngine;
using UnityEngine.UI;

namespace NavigationDemo.UI.Elements
{
    public class FloorTransitionPromptElementView : BaseViewElement
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private Text _iconFallbackText;
        [SerializeField] private Text _subtitleText;
        [SerializeField] private Text _titleText;
        [SerializeField] private Button _doneButton;
        [SerializeField] private Image _doneButtonBackgroundImage;
        [SerializeField] private Text _doneButtonText;
        [SerializeField] private Button _finishButton;
        [SerializeField] private Image _finishButtonOutlineImage;
        [SerializeField] private Image _finishButtonBackgroundImage;
        [SerializeField] private Text _finishButtonText;
        [SerializeField] private Sprite _defaultIconSprite;
        [SerializeField] private string _defaultIconValue = "\u21CA";
        [SerializeField] private string _defaultSubtitleValue = "\u0427\u0442\u043E\u0431\u044B \u043F\u0440\u043E\u0434\u043E\u043B\u0436\u0438\u0442\u044C,";
        [SerializeField] private string _defaultTitleValue = "\u0421\u043F\u0443\u0441\u0442\u0438\u0442\u0435\u0441\u044C \u043D\u0430 2 \u044D\u0442\u0430\u0436";
        [SerializeField] private string _defaultDoneButtonValue = "\u0413\u043E\u0442\u043E\u0432\u043E!";
        [SerializeField] private string _defaultFinishButtonValue = "\u0417\u0430\u0432\u0435\u0440\u0448\u0438\u0442\u044C";
        [SerializeField] private Color _defaultDoneButtonBackgroundColor = new Color(0.42745098f, 0.42745098f, 0.42745098f, 1f);
        [SerializeField] private Color _defaultDoneButtonTextColor = Color.white;
        [SerializeField] private Color _defaultFinishButtonOutlineColor = new Color(0.29803923f, 0.45490196f, 0.9647059f, 1f);
        [SerializeField] private Color _defaultFinishButtonBackgroundColor = new Color(0f, 0f, 0f, 0f);
        [SerializeField] private Color _defaultFinishButtonTextColor = new Color(0.29803923f, 0.45490196f, 0.9647059f, 1f);

        public event Action<FloorTransitionPromptElementView> DoneClicked;
        public event Action<FloorTransitionPromptElementView> FinishClicked;

        public string Value => GetTitleValue();

        protected override void Awake()
        {
            base.Awake();

            if (_doneButton != null)
            {
                _doneButton.onClick.AddListener(HandleDoneButtonClicked);
            }

            if (_finishButton != null)
            {
                _finishButton.onClick.AddListener(HandleFinishButtonClicked);
            }

            ApplyDefaultState();
        }

        private void OnDestroy()
        {
            if (_doneButton != null)
            {
                _doneButton.onClick.RemoveListener(HandleDoneButtonClicked);
            }

            if (_finishButton != null)
            {
                _finishButton.onClick.RemoveListener(HandleFinishButtonClicked);
            }
        }

        public void SetContent(
            Sprite valueIconSprite,
            string valueIconFallback,
            string valueSubtitle,
            string valueTitle)
        {
            SetIconFallbackValue(valueIconFallback);
            SetIconSprite(valueIconSprite);
            SetSubtitleValue(valueSubtitle);
            SetTitleValue(valueTitle);
        }

        public void SetIconSprite(Sprite value)
        {
            bool valueHasSprite = value != null;

            if (_iconImage != null)
            {
                _iconImage.sprite = value;
                _iconImage.enabled = valueHasSprite;
            }

            if (_iconFallbackText != null)
            {
                _iconFallbackText.gameObject.SetActive(!valueHasSprite);
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

        public void SetIconFallbackValue(string value)
        {
            if (_iconFallbackText == null)
            {
                return;
            }

            _iconFallbackText.text = string.IsNullOrWhiteSpace(value) ? _defaultIconValue : value;
        }

        public string GetIconFallbackValue()
        {
            if (_iconFallbackText == null)
            {
                return string.Empty;
            }

            return _iconFallbackText.text;
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

        public void SetDoneButtonValue(string value)
        {
            if (_doneButtonText == null)
            {
                return;
            }

            _doneButtonText.text = string.IsNullOrWhiteSpace(value) ? _defaultDoneButtonValue : value;
        }

        public string GetDoneButtonValue()
        {
            if (_doneButtonText == null)
            {
                return string.Empty;
            }

            return _doneButtonText.text;
        }

        public void SetFinishButtonValue(string value)
        {
            if (_finishButtonText == null)
            {
                return;
            }

            _finishButtonText.text = string.IsNullOrWhiteSpace(value) ? _defaultFinishButtonValue : value;
        }

        public string GetFinishButtonValue()
        {
            if (_finishButtonText == null)
            {
                return string.Empty;
            }

            return _finishButtonText.text;
        }

        public void SetDoneButtonInteractable(bool value)
        {
            if (_doneButton == null)
            {
                return;
            }

            _doneButton.interactable = value;
        }

        public void SetFinishButtonInteractable(bool value)
        {
            if (_finishButton == null)
            {
                return;
            }

            _finishButton.interactable = value;
        }

        public override string GetValue()
        {
            return Value;
        }

        private void ApplyDefaultState()
        {
            SetContent(_defaultIconSprite, _defaultIconValue, _defaultSubtitleValue, _defaultTitleValue);
            SetDoneButtonValue(_defaultDoneButtonValue);
            SetFinishButtonValue(_defaultFinishButtonValue);

            if (_doneButtonBackgroundImage != null)
            {
                _doneButtonBackgroundImage.color = _defaultDoneButtonBackgroundColor;
            }

            if (_doneButtonText != null)
            {
                _doneButtonText.color = _defaultDoneButtonTextColor;
            }

            if (_finishButtonOutlineImage != null)
            {
                _finishButtonOutlineImage.color = _defaultFinishButtonOutlineColor;
            }

            if (_finishButtonBackgroundImage != null)
            {
                _finishButtonBackgroundImage.color = _defaultFinishButtonBackgroundColor;
            }

            if (_finishButtonText != null)
            {
                _finishButtonText.color = _defaultFinishButtonTextColor;
            }
        }

        private void HandleDoneButtonClicked()
        {
            DoneClicked?.Invoke(this);
        }

        private void HandleFinishButtonClicked()
        {
            FinishClicked?.Invoke(this);
        }
    }
}
