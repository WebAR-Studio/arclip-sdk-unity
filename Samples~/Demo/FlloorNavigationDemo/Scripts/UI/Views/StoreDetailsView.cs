using System;
using NavigationDemo.UI.Base;
using NavigationDemo.UI.Elements;
using UnityEngine;

namespace NavigationDemo.UI.Views
{
    [ExecuteAlways]
    public class StoreDetailsView : BaseView
    {
        [SerializeField] private RectTransform _headerContainer;
        [SerializeField] private RectTransform _scheduleStickerContainer;
        [SerializeField] private RectTransform _descriptionContainer;
        [SerializeField] private RectTransform _closeButtonContainer;
        [SerializeField] private string _headerElementResourcePath = "UI/Elements/StoreDetailsHeaderElement";
        [SerializeField] private string _scheduleStickerElementResourcePath = "UI/Elements/StoreScheduleStickerElement";
        [SerializeField] private string _descriptionElementResourcePath = "UI/Elements/StoreDescriptionElement";
        [SerializeField] private string _closeButtonElementResourcePath = "UI/Elements/SquareIconButtonElement";
        [SerializeField] private bool _rebuildInEditMode = true;
        [SerializeField] private string _defaultTitleValue = "12 STOREEZ";
        [SerializeField] private string _defaultSubtitleValue = "Этаж 3, сектор 23";
        [SerializeField] private string _defaultCategoriesValue = "Одежда, Женская одежда, Мужская одежда, Аксессуары, Обувь, Локальные бренды";
        [SerializeField] private string _defaultScheduleValue = "10:00-22:00";
        [SerializeField] [TextArea(6, 20)] private string _defaultDescriptionValue =
            "Лаконичный гардероб для мужчин и женщин.\n\n" +
            "12 STOREEZ — российский бренд одежды,\n" +
            "созданный сестрами-близнецами Ириной и\n" +
            "Мариной Голомаздиными и Иваном\n" +
            "Хохловым. Каждый год марка выпускает 12\n" +
            "историй — 12 мини-коллекций, все модели в\n" +
            "которых сочетаются друг с другом.\n" +
            "Наша идея – создавать гармоничный и\n" +
            "продуманный гардероб надолго. И обновлять\n" +
            "его актуальными вещами 1-2 раза в сезон. Мы\n" +
            "уделяем много внимания выбору\n" +
            "качественных тканей и материалов и\n" +
            "тщательно прорабатываем каждую модель 12\n" +
            "STOREEZ: чем лаконичнее крой, тем более мы\n" +
            "требовательны к деталям.";
        [SerializeField] private string _defaultCloseIconValue = "?";
        [SerializeField] private Color _defaultCloseIconColor = Color.white;

        private StoreDetailsHeaderElementView _storeDetailsHeaderElementView;
        private StoreScheduleStickerElementView _storeScheduleStickerElementView;
        private StoreDescriptionElementView _storeDescriptionElementView;
        private SquareIconButtonElementView _closeButtonElementView;

        public event Action<StoreDetailsView> CloseClicked;

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
            if (_storeDetailsHeaderElementView == null)
            {
                return string.Empty;
            }

            return _storeDetailsHeaderElementView.GetTitleValue();
        }

        public void SetTitleValue(string value)
        {
            if (_storeDetailsHeaderElementView == null)
            {
                return;
            }

            _storeDetailsHeaderElementView.SetTitleValue(value);
        }

        public string GetSubtitleValue()
        {
            if (_storeDetailsHeaderElementView == null)
            {
                return string.Empty;
            }

            return _storeDetailsHeaderElementView.GetSubtitleValue();
        }

        public void SetSubtitleValue(string value)
        {
            if (_storeDetailsHeaderElementView == null)
            {
                return;
            }

            _storeDetailsHeaderElementView.SetSubtitleValue(value);
        }

        public string GetCategoriesValue()
        {
            if (_storeDetailsHeaderElementView == null)
            {
                return string.Empty;
            }

            return _storeDetailsHeaderElementView.GetCategoriesValue();
        }

        public void SetCategoriesValue(string value)
        {
            if (_storeDetailsHeaderElementView == null)
            {
                return;
            }

            _storeDetailsHeaderElementView.SetCategoriesValue(value);
        }

        public string GetScheduleValue()
        {
            if (_storeScheduleStickerElementView == null)
            {
                return string.Empty;
            }

            return _storeScheduleStickerElementView.GetValue();
        }

        public void SetScheduleValue(string value)
        {
            if (_storeScheduleStickerElementView == null)
            {
                return;
            }

            _storeScheduleStickerElementView.SetValue(value);
        }

        public string GetDescriptionValue()
        {
            if (_storeDescriptionElementView == null)
            {
                return string.Empty;
            }

            return _storeDescriptionElementView.GetValue();
        }

        public void SetDescriptionValue(string value)
        {
            if (_storeDescriptionElementView == null)
            {
                return;
            }

            _storeDescriptionElementView.SetValue(value);
        }

        public void SetCloseButtonInteractable(bool value)
        {
            if (_closeButtonElementView == null)
            {
                return;
            }

            _closeButtonElementView.SetInteractable(value);
        }

        private void BuildView()
        {
            BuildHeaderElement();
            BuildScheduleStickerElement();
            BuildDescriptionElement();
            BuildCloseButtonElement();
            ApplyDefaultState();
        }

        private void BuildHeaderElement()
        {
            if (_headerContainer == null)
            {
                return;
            }

            if (_storeDetailsHeaderElementView == null)
            {
                _storeDetailsHeaderElementView = _headerContainer.GetComponentInChildren<StoreDetailsHeaderElementView>(true);
            }

            if (_storeDetailsHeaderElementView == null)
            {
                StoreDetailsHeaderElementView value = Resources.Load<StoreDetailsHeaderElementView>(_headerElementResourcePath);
                if (value == null)
                {
                    return;
                }

                _storeDetailsHeaderElementView = Instantiate(value, _headerContainer);
                _storeDetailsHeaderElementView.name = value.name;
            }

            StretchToParent(_storeDetailsHeaderElementView.GetComponent<RectTransform>());
        }

        private void BuildScheduleStickerElement()
        {
            if (_scheduleStickerContainer == null)
            {
                return;
            }

            if (_storeScheduleStickerElementView == null)
            {
                _storeScheduleStickerElementView = _scheduleStickerContainer.GetComponentInChildren<StoreScheduleStickerElementView>(true);
            }

            if (_storeScheduleStickerElementView == null)
            {
                StoreScheduleStickerElementView value = Resources.Load<StoreScheduleStickerElementView>(_scheduleStickerElementResourcePath);
                if (value == null)
                {
                    return;
                }

                _storeScheduleStickerElementView = Instantiate(value, _scheduleStickerContainer);
                _storeScheduleStickerElementView.name = value.name;
            }

            StretchToParent(_storeScheduleStickerElementView.GetComponent<RectTransform>());
        }

        private void BuildDescriptionElement()
        {
            if (_descriptionContainer == null)
            {
                return;
            }

            if (_storeDescriptionElementView == null)
            {
                _storeDescriptionElementView = _descriptionContainer.GetComponentInChildren<StoreDescriptionElementView>(true);
            }

            if (_storeDescriptionElementView == null)
            {
                StoreDescriptionElementView value = Resources.Load<StoreDescriptionElementView>(_descriptionElementResourcePath);
                if (value == null)
                {
                    return;
                }

                _storeDescriptionElementView = Instantiate(value, _descriptionContainer);
                _storeDescriptionElementView.name = value.name;
            }

            StretchToParent(_storeDescriptionElementView.GetComponent<RectTransform>());
        }

        private void BuildCloseButtonElement()
        {
            if (_closeButtonContainer == null)
            {
                return;
            }

            if (_closeButtonElementView == null)
            {
                _closeButtonElementView = _closeButtonContainer.GetComponentInChildren<SquareIconButtonElementView>(true);
            }

            if (_closeButtonElementView == null)
            {
                SquareIconButtonElementView value = Resources.Load<SquareIconButtonElementView>(_closeButtonElementResourcePath);
                if (value == null)
                {
                    return;
                }

                _closeButtonElementView = Instantiate(value, _closeButtonContainer);
                _closeButtonElementView.name = value.name;
            }

            StretchToParent(_closeButtonElementView.GetComponent<RectTransform>());
            _closeButtonElementView.Clicked -= HandleCloseClicked;
            _closeButtonElementView.Clicked += HandleCloseClicked;
        }

        private void ApplyDefaultState()
        {
            if (_storeDetailsHeaderElementView != null)
            {
                _storeDetailsHeaderElementView.SetValue(_defaultTitleValue, _defaultSubtitleValue, _defaultCategoriesValue);
            }

            if (_storeScheduleStickerElementView != null)
            {
                _storeScheduleStickerElementView.SetValue(_defaultScheduleValue);
            }

            if (_storeDescriptionElementView != null)
            {
                _storeDescriptionElementView.SetValue(_defaultDescriptionValue);
            }

            if (_closeButtonElementView != null)
            {
                _closeButtonElementView.SetValue(_defaultCloseIconValue);
                _closeButtonElementView.SetIconColor(_defaultCloseIconColor);
                _closeButtonElementView.SetBackgroundColor(Color.clear);
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

        private void HandleCloseClicked(SquareIconButtonElementView value)
        {
            CloseClicked?.Invoke(this);
        }

        [ContextMenu("Rebuild View")]
        private void RebuildView()
        {
            BuildView();
        }
    }
}