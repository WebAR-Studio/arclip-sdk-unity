using System;
using System.Collections.Generic;
using NavigationDemo.UI.Base;
using NavigationDemo.UI.Elements;
using UnityEngine;
using UnityEngine.UI;

namespace NavigationDemo.UI.Views
{
    [ExecuteAlways]
    public class DestinationSelectionView : BaseView
    {
        [Serializable]
        public class CategoryViewData
        {
            [SerializeField] private string _value = "Category";
            [SerializeField] private Sprite _iconSprite;
            [SerializeField] private string _iconFallback = "\u2022";

            public string Value => _value;
            public Sprite IconSprite => _iconSprite;
            public string IconFallback => _iconFallback;

            public CategoryViewData(string value, Sprite iconSprite = null, string iconFallback = null)
            {
                _value = value;
                _iconSprite = iconSprite;
                _iconFallback = iconFallback;
            }
        }

        private static readonly List<string> _fallbackCategoryValues = new List<string>
        {
            "\u041C\u0430\u0433\u0430\u0437\u0438\u043D\u044B",
            "\u0415\u0434\u0430",
            "\u0420\u0430\u0437\u0432\u043B\u0435\u0447\u0435\u043D\u0438\u044F",
            "\u0414\u0435\u0442\u044F\u043C",
            "\u0423\u0441\u043B\u0443\u0433\u0438",
        };

        [SerializeField] private Text _titleText;
        [SerializeField] private RectTransform _searchContainer;
        [SerializeField] private RectTransform _categoriesContainer;
        [SerializeField] private string _searchElementResourcePath = "UI/Elements/SearchElement";
        [SerializeField] private string _categoryElementResourcePath = "UI/Elements/CategoryItemElement";
        [SerializeField] private float _categoryItemHeight = 52f;
        [SerializeField] private bool _rebuildInEditMode = true;
        [SerializeField] private Sprite _defaultSearchIconSprite;
        [SerializeField] private List<Sprite> _defaultCategoryIconSprites = new List<Sprite>();
        [SerializeField] private Sprite _defaultCategoryArrowSprite;
        [SerializeField] private List<string> _defaultCategoryValues = new List<string>(_fallbackCategoryValues);

        private readonly List<CategoryItemElementView> _categoryElementViews = new List<CategoryItemElementView>();
        private readonly List<CategoryViewData> _currentCategoryData = new List<CategoryViewData>();
        private SearchElementView _searchElementView;

        public event Action<int> CategorySelected;

        public string Value => GetSearchValue();

        public IReadOnlyList<CategoryItemElementView> CategoryElementViews => _categoryElementViews;

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
            UnbindCategoryEvents();
        }

        private void OnValidate()
        {
            if (Application.isPlaying || !_rebuildInEditMode)
            {
                return;
            }

            EnsureViewState();
            BuildView();
        }

        public void SetTitle(string value)
        {
            if (_titleText == null)
            {
                return;
            }

            _titleText.text = value;
        }

        public string GetTitle()
        {
            if (_titleText == null)
            {
                return string.Empty;
            }

            return _titleText.text;
        }

        public void SetSearchValue(string value)
        {
            if (_searchElementView == null)
            {
                return;
            }

            _searchElementView.SetValue(value);
        }

        public string GetSearchValue()
        {
            if (_searchElementView == null)
            {
                return string.Empty;
            }

            return _searchElementView.GetValue();
        }

        public IReadOnlyList<string> GetCategoryValues()
        {
            List<string> value = new List<string>(_categoryElementViews.Count);
            foreach (CategoryItemElementView categoryElementView in _categoryElementViews)
            {
                value.Add(categoryElementView.GetValue());
            }

            return value;
        }

        public void SetCategories(IReadOnlyList<string> value)
        {
            List<CategoryViewData> valueCategories = new List<CategoryViewData>();
            if (value != null)
            {
                for (int index = 0; index < value.Count; index++)
                {
                    valueCategories.Add(new CategoryViewData(value[index], null, GetCategoryIconFallback(index)));
                }
            }

            SetCategories(valueCategories);
        }

        public void SetCategories(IReadOnlyList<CategoryViewData> value)
        {
            SetCurrentCategoryData(value);
            SyncCategories(_currentCategoryData);
        }

        private void BuildView()
        {
            BuildSearchElement();
            EnsureCurrentCategoryData();
            SyncCategories(_currentCategoryData);
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

            RectTransform valueRectTransform = _searchElementView.GetComponent<RectTransform>();
            if (valueRectTransform != null)
            {
                StretchToParent(valueRectTransform);
            }

            _searchElementView.SetIconSprite(_defaultSearchIconSprite);
        }

        private void SyncCategories(IReadOnlyList<CategoryViewData> value)
        {
            if (_categoriesContainer == null)
            {
                return;
            }

            CollectCategoryElements();
            if (_categoryElementViews.Count != value.Count || HasLegacyCategoryElements())
            {
                RebuildCategories(value);
                return;
            }

            for (int index = 0; index < value.Count; index++)
            {
                CategoryItemElementView valueCategoryItemElementView = _categoryElementViews[index];
                CategoryViewData valueCategoryData = value[index];
                valueCategoryItemElementView.SetValue(valueCategoryData.Value);
                valueCategoryItemElementView.SetIcon(valueCategoryData.IconFallback);
                valueCategoryItemElementView.SetIconSprite(valueCategoryData.IconSprite);
                valueCategoryItemElementView.SetArrowSprite(_defaultCategoryArrowSprite);
                valueCategoryItemElementView.SetDividerVisible(index < value.Count - 1);

                RectTransform valueRectTransform = valueCategoryItemElementView.GetComponent<RectTransform>();
                if (valueRectTransform == null)
                {
                    continue;
                }

                valueRectTransform.anchorMin = new Vector2(0f, 1f);
                valueRectTransform.anchorMax = new Vector2(1f, 1f);
                valueRectTransform.pivot = new Vector2(0.5f, 1f);
                valueRectTransform.anchoredPosition = new Vector2(0f, -index * _categoryItemHeight);
                valueRectTransform.sizeDelta = new Vector2(0f, _categoryItemHeight);
            }

            _categoriesContainer.sizeDelta = new Vector2(_categoriesContainer.sizeDelta.x, value.Count * _categoryItemHeight);
            BindCategoryEvents();
        }

        private void RebuildCategories(IReadOnlyList<CategoryViewData> value)
        {
            if (_categoriesContainer == null)
            {
                return;
            }

            ClearCategoryElements();

            CategoryItemElementView categoryItemElementView = Resources.Load<CategoryItemElementView>(_categoryElementResourcePath);
            if (categoryItemElementView == null)
            {
                return;
            }

            for (int index = 0; index < value.Count; index++)
            {
                CategoryItemElementView valueCategoryItemElementView = Instantiate(categoryItemElementView, _categoriesContainer);
                CategoryViewData valueCategoryData = value[index];
                valueCategoryItemElementView.SetValue(valueCategoryData.Value);
                valueCategoryItemElementView.SetIcon(valueCategoryData.IconFallback);
                valueCategoryItemElementView.SetIconSprite(valueCategoryData.IconSprite);
                valueCategoryItemElementView.SetArrowSprite(_defaultCategoryArrowSprite);
                valueCategoryItemElementView.SetDividerVisible(index < value.Count - 1);
                _categoryElementViews.Add(valueCategoryItemElementView);

                RectTransform valueRectTransform = valueCategoryItemElementView.GetComponent<RectTransform>();
                if (valueRectTransform == null)
                {
                    continue;
                }

                valueRectTransform.anchorMin = new Vector2(0f, 1f);
                valueRectTransform.anchorMax = new Vector2(1f, 1f);
                valueRectTransform.pivot = new Vector2(0.5f, 1f);
                valueRectTransform.anchoredPosition = new Vector2(0f, -index * _categoryItemHeight);
                valueRectTransform.sizeDelta = new Vector2(0f, _categoryItemHeight);
            }

            _categoriesContainer.sizeDelta = new Vector2(_categoriesContainer.sizeDelta.x, value.Count * _categoryItemHeight);
            BindCategoryEvents();
        }

        private void CollectCategoryElements()
        {
            _categoryElementViews.Clear();
            if (_categoriesContainer == null)
            {
                return;
            }

            int value = _categoriesContainer.childCount;
            for (int index = 0; index < value; index++)
            {
                Transform valueTransform = _categoriesContainer.GetChild(index);
                CategoryItemElementView valueCategoryItemElementView = valueTransform.GetComponent<CategoryItemElementView>();
                if (valueCategoryItemElementView == null)
                {
                    continue;
                }

                _categoryElementViews.Add(valueCategoryItemElementView);
            }
        }

        private void ClearCategoryElements()
        {
            if (_categoriesContainer == null)
            {
                UnbindCategoryEvents();
                _categoryElementViews.Clear();
                return;
            }

            UnbindCategoryEvents();
            for (int index = _categoriesContainer.childCount - 1; index >= 0; index--)
            {
                Transform valueTransform = _categoriesContainer.GetChild(index);
                CategoryItemElementView valueCategoryItemElementView = valueTransform.GetComponent<CategoryItemElementView>();
                if (valueCategoryItemElementView == null)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    Destroy(valueCategoryItemElementView.gameObject);
                    continue;
                }

                DestroyImmediate(valueCategoryItemElementView.gameObject);
            }

            _categoryElementViews.Clear();
        }

        private void BindCategoryEvents()
        {
            for (int index = 0; index < _categoryElementViews.Count; index++)
            {
                CategoryItemElementView valueCategoryElementView = _categoryElementViews[index];
                if (valueCategoryElementView == null)
                {
                    continue;
                }

                valueCategoryElementView.Clicked -= HandleCategoryClicked;
                valueCategoryElementView.Clicked += HandleCategoryClicked;
            }
        }

        private void UnbindCategoryEvents()
        {
            for (int index = 0; index < _categoryElementViews.Count; index++)
            {
                CategoryItemElementView valueCategoryElementView = _categoryElementViews[index];
                if (valueCategoryElementView == null)
                {
                    continue;
                }

                valueCategoryElementView.Clicked -= HandleCategoryClicked;
            }
        }

        private void HandleCategoryClicked(CategoryItemElementView value)
        {
            int valueIndex = _categoryElementViews.IndexOf(value);
            if (valueIndex < 0)
            {
                return;
            }

            CategorySelected?.Invoke(valueIndex);
        }

        private void EnsureCurrentCategoryData()
        {
            if (_currentCategoryData.Count > 0)
            {
                return;
            }

            _currentCategoryData.Clear();
            AddDefaultCategories(_currentCategoryData);
        }

        private void SetCurrentCategoryData(IReadOnlyList<CategoryViewData> value)
        {
            _currentCategoryData.Clear();
            if (value != null)
            {
                for (int index = 0; index < value.Count; index++)
                {
                    CategoryViewData valueCategoryData = value[index];
                    if (valueCategoryData == null)
                    {
                        continue;
                    }

                    string valueTitle = string.IsNullOrWhiteSpace(valueCategoryData.Value)
                        ? GetDefaultCategoryValue(index)
                        : valueCategoryData.Value;
                    string valueIconFallback = string.IsNullOrWhiteSpace(valueCategoryData.IconFallback)
                        ? GetCategoryIconFallback(index)
                        : valueCategoryData.IconFallback;
                    Sprite valueIcon = valueCategoryData.IconSprite != null
                        ? valueCategoryData.IconSprite
                        : GetCategoryIconSprite(index);

                    _currentCategoryData.Add(new CategoryViewData(valueTitle, valueIcon, valueIconFallback));
                }
            }

            if (_currentCategoryData.Count == 0)
            {
                AddDefaultCategories(_currentCategoryData);
            }
        }

        private void AddDefaultCategories(List<CategoryViewData> value)
        {
            List<string> valuesSource = _defaultCategoryValues != null && _defaultCategoryValues.Count > 0
                ? _defaultCategoryValues
                : _fallbackCategoryValues;
            for (int index = 0; index < valuesSource.Count; index++)
            {
                value.Add(new CategoryViewData(
                    valuesSource[index],
                    GetCategoryIconSprite(index),
                    GetCategoryIconFallback(index)));
            }
        }

        private string GetDefaultCategoryValue(int value)
        {
            if (_defaultCategoryValues != null && value >= 0 && value < _defaultCategoryValues.Count)
            {
                return _defaultCategoryValues[value];
            }

            if (_fallbackCategoryValues.Count > 0 && value >= 0 && value < _fallbackCategoryValues.Count)
            {
                return _fallbackCategoryValues[value];
            }

            return "Category";
        }

        private Sprite GetCategoryIconSprite(int value)
        {
            if (_defaultCategoryIconSprites == null || _defaultCategoryIconSprites.Count == 0)
            {
                return null;
            }

            if (value >= 0 && value < _defaultCategoryIconSprites.Count)
            {
                return _defaultCategoryIconSprites[value];
            }

            return _defaultCategoryIconSprites[_defaultCategoryIconSprites.Count - 1];
        }

        private static string GetCategoryIconFallback(int value)
        {
            switch (value)
            {
                case 0:
                    return "\u2302";
                case 1:
                    return "\u2668";
                case 2:
                    return "\u25C9";
                case 3:
                    return "\u25CC";
                case 4:
                    return "\u2713";
                default:
                    return "\u2022";
            }
        }

        private bool HasLegacyCategoryElements()
        {
            foreach (CategoryItemElementView value in _categoryElementViews)
            {
                if (value == null || !value.HasImageIconReferences)
                {
                    return true;
                }
            }

            return false;
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

        private static void StretchToParent(RectTransform value)
        {
            value.anchorMin = Vector2.zero;
            value.anchorMax = Vector2.one;
            value.offsetMin = Vector2.zero;
            value.offsetMax = Vector2.zero;
            value.anchoredPosition = Vector2.zero;
            value.localScale = Vector3.one;
        }

        private void EnsureViewState()
        {
            if (_defaultCategoryValues == null || _defaultCategoryValues.Count == 0)
            {
                _defaultCategoryValues = new List<string>(_fallbackCategoryValues);
            }

            if (_titleText != null && string.IsNullOrWhiteSpace(_titleText.text))
            {
                _titleText.text = "\u041A\u0443\u0434\u0430 \u043F\u043E\u0439\u0434\u0451\u043C?";
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
