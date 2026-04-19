using System;
using System.Collections.Generic;
using NavigationDemo.UI.Base;
using NavigationDemo.UI.Elements;
using UnityEngine;

namespace NavigationDemo.UI.Views
{
    [ExecuteAlways]
    public class CategoryStoresView : BaseView
    {
        [Serializable]
        public class StoreCardViewData
        {
            [SerializeField] private string _value = "12 STOREEZ";
            [SerializeField] private string _description = "Store description";
            [SerializeField] private Sprite _thumbnail;
            [SerializeField] private string _thumbnailText = "12";
            [SerializeField] private bool _showInfoIcon;

            public string Value => _value;
            public string Description => _description;
            public Sprite Thumbnail => _thumbnail;
            public string ThumbnailText => _thumbnailText;
            public bool ShowInfoIcon => _showInfoIcon;

            public StoreCardViewData(
                string value,
                string description,
                Sprite thumbnail = null,
                string thumbnailText = null,
                bool showInfoIcon = false)
            {
                _value = value;
                _description = description;
                _thumbnail = thumbnail;
                _thumbnailText = thumbnailText;
                _showInfoIcon = showInfoIcon;
            }
        }

        [Serializable]
        private class StoreData
        {
            [SerializeField] private string _value = "12 STOREEZ";
            [SerializeField] private string _description = "\u041E\u0434\u0435\u0436\u0434\u0430, \u0416\u0435\u043D\u0441\u043A\u0430\u044F \u043E\u0434\u0435\u0436\u0434\u0430, \u041C\u0443\u0436..";
            [SerializeField] private Sprite _thumbnail;
            [SerializeField] private string _thumbnailText = "12";
            [SerializeField] private bool _showInfoIcon;

            public string Value => _value;
            public string Description => _description;
            public Sprite Thumbnail => _thumbnail;
            public string ThumbnailText => _thumbnailText;
            public bool ShowInfoIcon => _showInfoIcon;
        }

        private static readonly List<StoreData> _fallbackStoreData = new List<StoreData>
        {
            new StoreData(),
            new StoreData(),
            new StoreData(),
            new StoreData(),
        };

        [SerializeField] private RectTransform _backButtonContainer;
        [SerializeField] private RectTransform _headerContainer;
        [SerializeField] private RectTransform _searchContainer;
        [SerializeField] private RectTransform _storeCardsScrollContainer;
        [SerializeField] private RectTransform _bottomButtonContainer;
        [SerializeField] private string _backButtonResourcePath = "UI/Elements/BackButtonElement";
        [SerializeField] private string _categoryHeaderResourcePath = "UI/Elements/CategoryHeaderElement";
        [SerializeField] private string _searchElementResourcePath = "UI/Elements/SearchElement";
        [SerializeField] private string _verticalScrollViewResourcePath = "UI/Elements/VerticalScrollViewElement";
        [SerializeField] private string _storeCardResourcePath = "UI/Elements/StoreCardElement";
        [SerializeField] private float _storeCardHeight = 80f;
        [SerializeField] private float _storeCardSpacing = 8f;
        [SerializeField] private bool _rebuildInEditMode = true;
        [SerializeField] private Sprite _defaultBackIconSprite;
        [SerializeField] private Sprite _defaultHeaderIconSprite;
        [SerializeField] private Sprite _defaultSearchIconSprite;
        [SerializeField] private Sprite _defaultBottomIconSprite;
        [SerializeField] private Sprite _defaultStoreInfoIconSprite;
        [SerializeField] private string _defaultBackValue = "\u0412\u0441\u0435 \u043A\u0430\u0442\u0435\u0433\u043E\u0440\u0438\u0438";
        [SerializeField] private string _defaultHeaderValue = "\u041C\u0430\u0433\u0430\u0437\u0438\u043D\u044B";
        [SerializeField] private string _defaultHeaderIconFallback = "\u2302";
        [SerializeField] private string _defaultBottomValue = "Button";
        [SerializeField] private List<StoreData> _defaultStoreData = new List<StoreData>
        {
            new StoreData(),
            new StoreData(),
            new StoreData(),
            new StoreData(),
        };

        private readonly List<StoreCardElementView> _storeCardElementViews = new List<StoreCardElementView>();
        private readonly List<StoreCardViewData> _currentStoreData = new List<StoreCardViewData>();
        private BackButtonElementView _backButtonElementView;
        private CategoryHeaderElementView _categoryHeaderElementView;
        private SearchElementView _searchElementView;
        private VerticalScrollViewElementView _storeCardsScrollElementView;
        private RectTransform _storeCardsContainer;
        private BackButtonElementView _bottomButtonElementView;

        public event Action<int> StoreSelected;
        public event Action BackRequested;

        public string Value => GetSearchValue();

        public IReadOnlyList<StoreCardElementView> StoreCardElementViews => _storeCardElementViews;

        protected override void Awake()
        {
            base.Awake();
            EnsureViewState();
            BuildView();
        }

        private void OnEnable()
        {
            if (!Application.isPlaying && !_rebuildInEditMode)
            {
                return;
            }

            EnsureViewState();
            BuildView();
        }

        private void OnDestroy()
        {
            UnbindStoreCardEvents();
            UnbindBackButtonEvents();
        }

        public string GetBackButtonValue()
        {
            if (_backButtonElementView == null)
            {
                return string.Empty;
            }

            return _backButtonElementView.GetValue();
        }

        public void SetBackButtonValue(string value)
        {
            if (_backButtonElementView == null)
            {
                return;
            }

            _backButtonElementView.SetValue(value);
        }

        public string GetHeaderValue()
        {
            if (_categoryHeaderElementView == null)
            {
                return string.Empty;
            }

            return _categoryHeaderElementView.GetValue();
        }

        public void SetHeaderValue(string value)
        {
            if (_categoryHeaderElementView == null)
            {
                return;
            }

            _categoryHeaderElementView.SetValue(value);
        }

        public void SetHeaderIcon(Sprite iconSprite, string iconFallback = null)
        {
            if (_categoryHeaderElementView == null)
            {
                return;
            }

            _categoryHeaderElementView.SetIcon(string.IsNullOrWhiteSpace(iconFallback)
                ? _defaultHeaderIconFallback
                : iconFallback);
            _categoryHeaderElementView.SetIconSprite(iconSprite);
        }

        public string GetSearchValue()
        {
            if (_searchElementView == null)
            {
                return string.Empty;
            }

            return _searchElementView.GetValue();
        }

        public void SetSearchValue(string value)
        {
            if (_searchElementView == null)
            {
                return;
            }

            _searchElementView.SetValue(value);
        }

        public IReadOnlyList<string> GetStoreValues()
        {
            List<string> value = new List<string>(_storeCardElementViews.Count);
            foreach (StoreCardElementView storeCardElementView in _storeCardElementViews)
            {
                value.Add(storeCardElementView.GetValue());
            }

            return value;
        }

        public void SetStores(IReadOnlyList<StoreCardViewData> value)
        {
            SetCurrentStoreData(value);
            SyncStoreCards(_currentStoreData);
        }

        private void BuildView()
        {
            BuildBackButtonElement();
            BuildCategoryHeaderElement();
            BuildSearchElement();
            BuildStoreCardsScrollElement();
            BuildBottomButtonElement();
            EnsureCurrentStoreData();
            SyncStoreCards(_currentStoreData);
        }

        private void BuildBackButtonElement()
        {
            if (_backButtonContainer == null)
            {
                return;
            }

            if (_backButtonElementView == null)
            {
                _backButtonElementView = _backButtonContainer.GetComponentInChildren<BackButtonElementView>(true);
            }

            if (_backButtonElementView != null && !_backButtonElementView.HasIconImageReference)
            {
                DestroyElement(_backButtonElementView.gameObject);
                _backButtonElementView = null;
            }

            if (_backButtonElementView == null)
            {
                BackButtonElementView value = Resources.Load<BackButtonElementView>(_backButtonResourcePath);
                if (value == null)
                {
                    return;
                }

                _backButtonElementView = Instantiate(value, _backButtonContainer);
                _backButtonElementView.name = value.name;
            }

            StretchToParent(_backButtonElementView.GetComponent<RectTransform>());
            _backButtonElementView.SetValue(_defaultBackValue);
            _backButtonElementView.SetIcon("\u2039");
            _backButtonElementView.SetIconSprite(_defaultBackIconSprite);

            _backButtonElementView.Clicked -= HandleBackButtonClicked;
            _backButtonElementView.Clicked += HandleBackButtonClicked;
        }

        private void BuildCategoryHeaderElement()
        {
            if (_headerContainer == null)
            {
                return;
            }

            if (_categoryHeaderElementView == null)
            {
                _categoryHeaderElementView = _headerContainer.GetComponentInChildren<CategoryHeaderElementView>(true);
            }

            if (_categoryHeaderElementView != null && !_categoryHeaderElementView.HasIconImageReference)
            {
                DestroyElement(_categoryHeaderElementView.gameObject);
                _categoryHeaderElementView = null;
            }

            if (_categoryHeaderElementView == null)
            {
                CategoryHeaderElementView value = Resources.Load<CategoryHeaderElementView>(_categoryHeaderResourcePath);
                if (value == null)
                {
                    return;
                }

                _categoryHeaderElementView = Instantiate(value, _headerContainer);
                _categoryHeaderElementView.name = value.name;
            }

            StretchToParent(_categoryHeaderElementView.GetComponent<RectTransform>());
            _categoryHeaderElementView.SetValue(_defaultHeaderValue);
            _categoryHeaderElementView.SetIcon(_defaultHeaderIconFallback);
            _categoryHeaderElementView.SetIconSprite(_defaultHeaderIconSprite);
        }

        private void BuildSearchElement()
        {
            if (_searchContainer == null)
            {
                return;
            }

            if (_searchElementView == null)
            {
                _searchElementView = _searchContainer.GetComponentInChildren<SearchElementView>(true);
            }

            if (_searchElementView != null && !_searchElementView.HasIconImageReference)
            {
                DestroyElement(_searchElementView.gameObject);
                _searchElementView = null;
            }

            if (_searchElementView == null)
            {
                SearchElementView value = Resources.Load<SearchElementView>(_searchElementResourcePath);
                if (value == null)
                {
                    return;
                }

                _searchElementView = Instantiate(value, _searchContainer);
                _searchElementView.name = value.name;
            }

            StretchToParent(_searchElementView.GetComponent<RectTransform>());
            _searchElementView.SetValue("\u041F\u043E\u0438\u0441\u043A");
            _searchElementView.SetIcon("\u2315");
            _searchElementView.SetIconSprite(_defaultSearchIconSprite);
        }

        private void BuildStoreCardsScrollElement()
        {
            if (_storeCardsScrollContainer == null)
            {
                return;
            }

            if (_storeCardsScrollElementView == null)
            {
                _storeCardsScrollElementView = _storeCardsScrollContainer.GetComponentInChildren<VerticalScrollViewElementView>(true);
            }

            if (_storeCardsScrollElementView == null)
            {
                VerticalScrollViewElementView value = Resources.Load<VerticalScrollViewElementView>(_verticalScrollViewResourcePath);
                if (value == null)
                {
                    return;
                }

                _storeCardsScrollElementView = Instantiate(value, _storeCardsScrollContainer);
                _storeCardsScrollElementView.name = value.name;
            }

            StretchToParent(_storeCardsScrollElementView.GetComponent<RectTransform>());
            _storeCardsContainer = _storeCardsScrollElementView.GetContent();
        }

        private void BuildBottomButtonElement()
        {
            if (_bottomButtonContainer == null)
            {
                return;
            }

            if (_bottomButtonElementView == null)
            {
                _bottomButtonElementView = _bottomButtonContainer.GetComponentInChildren<BackButtonElementView>(true);
            }

            if (_bottomButtonElementView != null && !_bottomButtonElementView.HasIconImageReference)
            {
                DestroyElement(_bottomButtonElementView.gameObject);
                _bottomButtonElementView = null;
            }

            if (_bottomButtonElementView == null)
            {
                BackButtonElementView value = Resources.Load<BackButtonElementView>(_backButtonResourcePath);
                if (value == null)
                {
                    return;
                }

                _bottomButtonElementView = Instantiate(value, _bottomButtonContainer);
                _bottomButtonElementView.name = value.name;
            }

            StretchToParent(_bottomButtonElementView.GetComponent<RectTransform>());
            _bottomButtonElementView.SetValue(_defaultBottomValue);
            _bottomButtonElementView.SetIcon("\u2039");
            _bottomButtonElementView.SetIconSprite(_defaultBottomIconSprite);
        }

        private void SyncStoreCards(IReadOnlyList<StoreCardViewData> value)
        {
            RectTransform valueStoreCardsContainer = GetStoreCardsContainer();
            if (valueStoreCardsContainer == null)
            {
                return;
            }

            CollectStoreCards();
            if (_storeCardElementViews.Count != value.Count || HasLegacyStoreCards())
            {
                RebuildStoreCards(value);
                return;
            }

            for (int index = 0; index < value.Count; index++)
            {
                StoreCardElementView valueStoreCardElementView = _storeCardElementViews[index];
                ApplyStoreData(valueStoreCardElementView, value[index], index);
                ApplyStoreCardLayout(valueStoreCardElementView.GetComponent<RectTransform>(), index);
            }

            valueStoreCardsContainer.sizeDelta = new Vector2(valueStoreCardsContainer.sizeDelta.x, GetStoreCardsHeight(value.Count));
            BindStoreCardEvents();
        }

        private void RebuildStoreCards(IReadOnlyList<StoreCardViewData> value)
        {
            RectTransform valueStoreCardsContainer = GetStoreCardsContainer();
            if (valueStoreCardsContainer == null)
            {
                return;
            }

            ClearStoreCards();

            StoreCardElementView valueStoreCardElement = Resources.Load<StoreCardElementView>(_storeCardResourcePath);
            if (valueStoreCardElement == null)
            {
                return;
            }

            for (int index = 0; index < value.Count; index++)
            {
                StoreCardElementView valueStoreCardElementView = Instantiate(valueStoreCardElement, valueStoreCardsContainer);
                valueStoreCardElementView.name = valueStoreCardElement.name;
                ApplyStoreData(valueStoreCardElementView, value[index], index);
                ApplyStoreCardLayout(valueStoreCardElementView.GetComponent<RectTransform>(), index);
                _storeCardElementViews.Add(valueStoreCardElementView);
            }

            valueStoreCardsContainer.sizeDelta = new Vector2(valueStoreCardsContainer.sizeDelta.x, GetStoreCardsHeight(value.Count));
            BindStoreCardEvents();
        }

        private void ApplyStoreData(StoreCardElementView valueStoreCardElementView, StoreCardViewData value, int valueIndex)
        {
            valueStoreCardElementView.SetValue(value.Value);
            valueStoreCardElementView.SetDescription(value.Description);
            valueStoreCardElementView.SetThumbnail(value.Thumbnail);
            valueStoreCardElementView.SetThumbnailText(string.IsNullOrWhiteSpace(value.ThumbnailText)
                ? GetStoreThumbnailText(value.Value, valueIndex)
                : value.ThumbnailText);
            valueStoreCardElementView.SetTrailingIcon("\u24D8");
            valueStoreCardElementView.SetTrailingIconSprite(_defaultStoreInfoIconSprite);
            valueStoreCardElementView.SetTrailingIconVisible(value.ShowInfoIcon);
        }

        private void ApplyStoreCardLayout(RectTransform value, int valueIndex)
        {
            if (value == null)
            {
                return;
            }

            value.anchorMin = new Vector2(0f, 1f);
            value.anchorMax = new Vector2(1f, 1f);
            value.pivot = new Vector2(0.5f, 1f);
            value.anchoredPosition = new Vector2(0f, -valueIndex * (_storeCardHeight + _storeCardSpacing));
            value.sizeDelta = new Vector2(0f, _storeCardHeight);
        }

        private void CollectStoreCards()
        {
            _storeCardElementViews.Clear();
            RectTransform valueStoreCardsContainer = GetStoreCardsContainer();
            if (valueStoreCardsContainer == null)
            {
                return;
            }

            int value = valueStoreCardsContainer.childCount;
            for (int index = 0; index < value; index++)
            {
                Transform valueTransform = valueStoreCardsContainer.GetChild(index);
                StoreCardElementView valueStoreCardElementView = valueTransform.GetComponent<StoreCardElementView>();
                if (valueStoreCardElementView == null)
                {
                    continue;
                }

                _storeCardElementViews.Add(valueStoreCardElementView);
            }
        }

        private void ClearStoreCards()
        {
            RectTransform valueStoreCardsContainer = GetStoreCardsContainer();
            if (valueStoreCardsContainer == null)
            {
                UnbindStoreCardEvents();
                _storeCardElementViews.Clear();
                return;
            }

            UnbindStoreCardEvents();
            for (int index = valueStoreCardsContainer.childCount - 1; index >= 0; index--)
            {
                Transform value = valueStoreCardsContainer.GetChild(index);
                StoreCardElementView valueStoreCardElementView = value.GetComponent<StoreCardElementView>();
                if (valueStoreCardElementView == null)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    Destroy(valueStoreCardElementView.gameObject);
                    continue;
                }

                DestroyImmediate(valueStoreCardElementView.gameObject);
            }

            _storeCardElementViews.Clear();
        }

        private void BindStoreCardEvents()
        {
            for (int index = 0; index < _storeCardElementViews.Count; index++)
            {
                StoreCardElementView valueStoreCard = _storeCardElementViews[index];
                if (valueStoreCard == null)
                {
                    continue;
                }

                valueStoreCard.Clicked -= HandleStoreCardClicked;
                valueStoreCard.Clicked += HandleStoreCardClicked;
            }
        }

        private void UnbindStoreCardEvents()
        {
            for (int index = 0; index < _storeCardElementViews.Count; index++)
            {
                StoreCardElementView valueStoreCard = _storeCardElementViews[index];
                if (valueStoreCard == null)
                {
                    continue;
                }

                valueStoreCard.Clicked -= HandleStoreCardClicked;
            }
        }

        private void UnbindBackButtonEvents()
        {
            if (_backButtonElementView != null)
            {
                _backButtonElementView.Clicked -= HandleBackButtonClicked;
            }
        }

        private void HandleBackButtonClicked(BackButtonElementView value)
        {
            BackRequested?.Invoke();
        }

        private void HandleStoreCardClicked(StoreCardElementView value)
        {
            int valueIndex = _storeCardElementViews.IndexOf(value);
            if (valueIndex < 0)
            {
                return;
            }

            StoreSelected?.Invoke(valueIndex);
        }

        private float GetStoreCardsHeight(int value)
        {
            if (value <= 0)
            {
                return 0f;
            }

            return value * _storeCardHeight + (value - 1) * _storeCardSpacing;
        }

        private RectTransform GetStoreCardsContainer()
        {
            if (_storeCardsContainer != null)
            {
                return _storeCardsContainer;
            }

            if (_storeCardsScrollElementView == null)
            {
                return null;
            }

            _storeCardsContainer = _storeCardsScrollElementView.GetContent();
            return _storeCardsContainer;
        }

        private void EnsureCurrentStoreData()
        {
            if (_currentStoreData.Count > 0)
            {
                return;
            }

            _currentStoreData.Clear();
            AddDefaultStoreData(_currentStoreData);
        }

        private bool HasLegacyStoreCards()
        {
            for (int index = 0; index < _storeCardElementViews.Count; index++)
            {
                StoreCardElementView value = _storeCardElementViews[index];
                if (value == null || !value.HasTrailingIconImageReference)
                {
                    return true;
                }
            }

            return false;
        }

        private void SetCurrentStoreData(IReadOnlyList<StoreCardViewData> value)
        {
            _currentStoreData.Clear();
            if (value != null)
            {
                for (int index = 0; index < value.Count; index++)
                {
                    StoreCardViewData valueStoreData = value[index];
                    if (valueStoreData == null)
                    {
                        continue;
                    }

                    string valueTitle = string.IsNullOrWhiteSpace(valueStoreData.Value)
                        ? GetDefaultStoreValue(index)
                        : valueStoreData.Value;
                    string valueDescription = string.IsNullOrWhiteSpace(valueStoreData.Description)
                        ? GetDefaultStoreDescription(index)
                        : valueStoreData.Description;

                    _currentStoreData.Add(new StoreCardViewData(
                        valueTitle,
                        valueDescription,
                        valueStoreData.Thumbnail,
                        valueStoreData.ThumbnailText,
                        valueStoreData.ShowInfoIcon));
                }
            }

            if (_currentStoreData.Count == 0)
            {
                AddDefaultStoreData(_currentStoreData);
            }
        }

        private void AddDefaultStoreData(List<StoreCardViewData> value)
        {
            List<StoreData> valuesSource = _defaultStoreData != null && _defaultStoreData.Count > 0
                ? _defaultStoreData
                : _fallbackStoreData;
            for (int index = 0; index < valuesSource.Count; index++)
            {
                StoreData valueData = valuesSource[index];
                value.Add(new StoreCardViewData(
                    valueData.Value,
                    valueData.Description,
                    valueData.Thumbnail,
                    valueData.ThumbnailText,
                    valueData.ShowInfoIcon));
            }
        }

        private string GetDefaultStoreValue(int value)
        {
            if (_defaultStoreData != null && value >= 0 && value < _defaultStoreData.Count)
            {
                return _defaultStoreData[value].Value;
            }

            if (_fallbackStoreData.Count > 0 && value >= 0 && value < _fallbackStoreData.Count)
            {
                return _fallbackStoreData[value].Value;
            }

            return "Store";
        }

        private string GetDefaultStoreDescription(int value)
        {
            if (_defaultStoreData != null && value >= 0 && value < _defaultStoreData.Count)
            {
                return _defaultStoreData[value].Description;
            }

            if (_fallbackStoreData.Count > 0 && value >= 0 && value < _fallbackStoreData.Count)
            {
                return _fallbackStoreData[value].Description;
            }

            return string.Empty;
        }

        private static string GetStoreThumbnailText(string value, int valueIndex)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "...";
            }

            switch (valueIndex)
            {
                case 0:
                    return "12";
                case 1:
                    return "Befree";
                case 2:
                    return "BORK";
                case 3:
                    return "2M";
                default:
                    return value.Substring(0, Mathf.Min(value.Length, 4)).ToUpperInvariant();
            }
        }

        private static void StretchToParent(RectTransform value)
        {
            if (value == null)
            {
                return;
            }

            value.anchorMin = Vector2.zero;
            value.anchorMax = Vector2.one;
            value.offsetMin = Vector2.zero;
            value.offsetMax = Vector2.zero;
            value.anchoredPosition = Vector2.zero;
            value.localScale = Vector3.one;
        }

        private static void DestroyElement(GameObject value)
        {
            if (value == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(value);
                return;
            }

            DestroyImmediate(value);
        }

        private void EnsureViewState()
        {
            if (_defaultStoreData == null || _defaultStoreData.Count == 0)
            {
                _defaultStoreData = new List<StoreData>(_fallbackStoreData);
            }
        }

        [ContextMenu("Rebuild View")]
        private void RebuildView()
        {
            EnsureViewState();
            BuildView();
        }
    }
}
