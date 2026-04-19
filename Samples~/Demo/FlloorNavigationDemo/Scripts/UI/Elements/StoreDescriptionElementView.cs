using NavigationDemo.UI.Base;
using UnityEngine;
using UnityEngine.UI;

namespace NavigationDemo.UI.Elements
{
    public class StoreDescriptionElementView : BaseViewElement
    {
        [SerializeField] private Text _valueText;
        [SerializeField] [TextArea(6, 20)] private string _defaultValue =
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

        public string Value => GetValue();

        protected override void Awake()
        {
            base.Awake();
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

        public override string GetValue()
        {
            if (_valueText == null)
            {
                return string.Empty;
            }

            return _valueText.text;
        }
    }
}