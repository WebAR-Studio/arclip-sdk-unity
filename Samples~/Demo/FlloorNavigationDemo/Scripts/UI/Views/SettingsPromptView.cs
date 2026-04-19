using System;
using System.Collections.Generic;
using NavigationDemo.UI.Base;
using NavigationDemo.UI.Elements;
using UnityEngine;

namespace NavigationDemo.UI.Views
{
    [Serializable]
    public class SettingsPromptVariant
    {
        [SerializeField] private string _id = "NoInternet";
        [SerializeField] private Sprite _iconSprite;
        [SerializeField] private string _iconValue = "\u26A0";
        [SerializeField] private string _titleValue = "\u041D\u0435\u0442 \u043F\u043E\u0434\u043A\u043B\u044E\u0447\u0435\u043D\u0438\u044F \u043A \u0438\u043D\u0442\u0435\u0440\u043D\u0435\u0442\u0443";
        [SerializeField] [TextArea(2, 8)] private string _descriptionValue =
            "\u0414\u043B\u044F \u043A\u043E\u0440\u0440\u0435\u043A\u0442\u043D\u043E\u0439 \u0440\u0430\u0431\u043E\u0442\u044B \u043F\u0440\u0438\u043B\u043E\u0436\u0435\u043D\u0438\u044E \u0442\u0440\u0435\u0431\u0443\u0435\u0442\u0441\u044F \u0434\u043E\u0441\u0442\u0443\u043F \u043A \u0438\u043D\u0442\u0435\u0440\u043D\u0435\u0442\u0443.\n" +
            "\u041F\u0440\u043E\u0432\u0435\u0440\u044C\u0442\u0435 \u043F\u043E\u0434\u043A\u043B\u044E\u0447\u0435\u043D\u0438\u0435 \u043A \u0441\u0435\u0442\u0438 \u0438 \u043F\u043E\u043F\u0440\u043E\u0431\u0443\u0439\u0442\u0435 \u0441\u043D\u043E\u0432\u0430.";
        [SerializeField] private string _buttonValue = "\u041E\u0442\u043A\u0440\u044B\u0442\u044C \u043D\u0430\u0441\u0442\u0440\u043E\u0439\u043A\u0438";

        public string Id => _id;
        public Sprite IconSprite => _iconSprite;
        public string IconValue => _iconValue;
        public string TitleValue => _titleValue;
        public string DescriptionValue => _descriptionValue;
        public string ButtonValue => _buttonValue;
    }

    [ExecuteAlways]
    public class SettingsPromptView : BaseView
    {
        [SerializeField] private RectTransform _contentContainer;
        [SerializeField] private string _contentElementResourcePath = "UI/Elements/SettingsPromptElement";
        [SerializeField] private Vector2 _contentElementSize = new Vector2(322f, 380f);
        [SerializeField] private bool _rebuildInEditMode = true;
        [SerializeField] private bool _openSystemSettingsOnButtonClick = true;
        [SerializeField] private int _defaultVariantIndex = 0;
        [SerializeField] private List<SettingsPromptVariant> _variants = new List<SettingsPromptVariant>();
        [SerializeField] private string _defaultIconValue = "\u26A0";
        [SerializeField] private string _defaultTitleValue = "\u041D\u0435\u0442 \u043F\u043E\u0434\u043A\u043B\u044E\u0447\u0435\u043D\u0438\u044F \u043A \u0438\u043D\u0442\u0435\u0440\u043D\u0435\u0442\u0443";
        [SerializeField] [TextArea(2, 8)] private string _defaultDescriptionValue =
            "\u0414\u043B\u044F \u043A\u043E\u0440\u0440\u0435\u043A\u0442\u043D\u043E\u0439 \u0440\u0430\u0431\u043E\u0442\u044B \u043F\u0440\u0438\u043B\u043E\u0436\u0435\u043D\u0438\u044E \u0442\u0440\u0435\u0431\u0443\u0435\u0442\u0441\u044F \u0434\u043E\u0441\u0442\u0443\u043F \u043A \u0438\u043D\u0442\u0435\u0440\u043D\u0435\u0442\u0443.\n" +
            "\u041F\u0440\u043E\u0432\u0435\u0440\u044C\u0442\u0435 \u043F\u043E\u0434\u043A\u043B\u044E\u0447\u0435\u043D\u0438\u0435 \u043A \u0441\u0435\u0442\u0438 \u0438 \u043F\u043E\u043F\u0440\u043E\u0431\u0443\u0439\u0442\u0435 \u0441\u043D\u043E\u0432\u0430.";
        [SerializeField] private string _defaultButtonValue = "\u041E\u0442\u043A\u0440\u044B\u0442\u044C \u043D\u0430\u0441\u0442\u0440\u043E\u0439\u043A\u0438";

        private SettingsPromptElementView _settingsPromptElementView;

        public event Action<SettingsPromptView> OpenSettingsClicked;

        public string Value => GetTitleValue();

        protected override void Awake()
        {
            base.Awake();
            BuildView();
        }

        private void OnEnable()
        {
            if (!Application.isPlaying && !_rebuildInEditMode)
            {
                return;
            }

            BuildView();
        }

        private void OnValidate()
        {
            if (!Application.isPlaying && !_rebuildInEditMode)
            {
                return;
            }

            BuildView();
        }

        public string GetTitleValue()
        {
            if (_settingsPromptElementView == null)
            {
                return string.Empty;
            }

            return _settingsPromptElementView.GetTitleValue();
        }

        public string GetDescriptionValue()
        {
            if (_settingsPromptElementView == null)
            {
                return string.Empty;
            }

            return _settingsPromptElementView.GetDescriptionValue();
        }

        public string GetButtonValue()
        {
            if (_settingsPromptElementView == null)
            {
                return string.Empty;
            }

            return _settingsPromptElementView.GetButtonValue();
        }

        public void SetContent(
            Sprite valueIconSprite,
            string valueIconFallback,
            string valueTitle,
            string valueDescription,
            string valueButton)
        {
            if (_settingsPromptElementView == null)
            {
                return;
            }

            _settingsPromptElementView.SetContent(
                valueIconSprite,
                valueIconFallback,
                valueTitle,
                valueDescription,
                valueButton);
        }

        public void SetTitleValue(string value)
        {
            if (_settingsPromptElementView == null)
            {
                return;
            }

            _settingsPromptElementView.SetTitleValue(value);
        }

        public void SetDescriptionValue(string value)
        {
            if (_settingsPromptElementView == null)
            {
                return;
            }

            _settingsPromptElementView.SetDescriptionValue(value);
        }

        public void SetButtonValue(string value)
        {
            if (_settingsPromptElementView == null)
            {
                return;
            }

            _settingsPromptElementView.SetButtonValue(value);
        }

        public void SetButtonInteractable(bool value)
        {
            if (_settingsPromptElementView == null)
            {
                return;
            }

            _settingsPromptElementView.SetButtonInteractable(value);
        }

        public void ApplyVariantByIndex(int value)
        {
            if (_variants == null || _variants.Count == 0)
            {
                ApplyFallbackState();
                return;
            }

            int valueIndex = Mathf.Clamp(value, 0, _variants.Count - 1);
            ApplyVariant(_variants[valueIndex]);
        }

        public bool TryApplyVariant(string valueVariantId)
        {
            if (string.IsNullOrWhiteSpace(valueVariantId) || _variants == null)
            {
                return false;
            }

            for (int index = 0; index < _variants.Count; index++)
            {
                SettingsPromptVariant valueVariant = _variants[index];
                if (valueVariant == null)
                {
                    continue;
                }

                if (!string.Equals(valueVariant.Id, valueVariantId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                ApplyVariant(valueVariant);
                return true;
            }

            return false;
        }

        public void OpenSystemSettings()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            OpenAndroidSystemSettings();
#elif UNITY_IOS && !UNITY_EDITOR
            Application.OpenURL("app-settings:");
#elif UNITY_STANDALONE_WIN
            Application.OpenURL("ms-settings:");
#elif UNITY_STANDALONE_OSX
            Application.OpenURL("x-apple.systempreferences:");
#else
            Debug.Log("SettingsPromptView.OpenSystemSettings: platform is not supported by built-in handler.");
#endif
        }

        private void BuildView()
        {
            BuildContentElement();
            ApplyDefaultState();
        }

        private void BuildContentElement()
        {
            if (_contentContainer == null)
            {
                return;
            }

            if (_settingsPromptElementView == null)
            {
                _settingsPromptElementView = _contentContainer.GetComponentInChildren<SettingsPromptElementView>(true);
            }

            if (_settingsPromptElementView == null)
            {
                SettingsPromptElementView value = Resources.Load<SettingsPromptElementView>(_contentElementResourcePath);
                if (value == null)
                {
                    return;
                }

                _settingsPromptElementView = Instantiate(value, _contentContainer);
                _settingsPromptElementView.name = value.name;
            }

            ApplyContentElementLayout(_settingsPromptElementView.GetComponent<RectTransform>());
            _settingsPromptElementView.OpenSettingsClicked -= HandleOpenSettingsClicked;
            _settingsPromptElementView.OpenSettingsClicked += HandleOpenSettingsClicked;
        }

        private void ApplyDefaultState()
        {
            if (_variants != null && _variants.Count > 0)
            {
                ApplyVariantByIndex(_defaultVariantIndex);
                return;
            }

            ApplyFallbackState();
        }

        private void ApplyFallbackState()
        {
            SetContent(
                null,
                _defaultIconValue,
                _defaultTitleValue,
                _defaultDescriptionValue,
                _defaultButtonValue);
        }

        private void ApplyVariant(SettingsPromptVariant value)
        {
            if (value == null)
            {
                ApplyFallbackState();
                return;
            }

            SetContent(
                value.IconSprite,
                value.IconValue,
                value.TitleValue,
                value.DescriptionValue,
                value.ButtonValue);
        }

        private void ApplyContentElementLayout(RectTransform value)
        {
            if (value == null)
            {
                return;
            }

            value.anchorMin = new Vector2(0.5f, 0.5f);
            value.anchorMax = new Vector2(0.5f, 0.5f);
            value.pivot = new Vector2(0.5f, 0.5f);
            value.anchoredPosition = Vector2.zero;
            value.sizeDelta = _contentElementSize;
            value.localScale = Vector3.one;
        }

        private void HandleOpenSettingsClicked(SettingsPromptElementView value)
        {
            if (_openSystemSettingsOnButtonClick)
            {
                OpenSystemSettings();
            }

            OpenSettingsClicked?.Invoke(this);
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private static void OpenAndroidSystemSettings()
        {
            try
            {
                using (AndroidJavaClass valueUnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (AndroidJavaObject valueActivity = valueUnityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (AndroidJavaObject valueIntent = new AndroidJavaObject("android.content.Intent", "android.settings.SETTINGS"))
                {
                    valueActivity.Call("startActivity", valueIntent);
                }
            }
            catch (Exception valueException)
            {
                Debug.LogWarning($"SettingsPromptView.OpenAndroidSystemSettings failed: {valueException.Message}");
            }
        }
#endif

        [ContextMenu("Rebuild View")]
        private void RebuildView()
        {
            BuildView();
        }
    }
}
