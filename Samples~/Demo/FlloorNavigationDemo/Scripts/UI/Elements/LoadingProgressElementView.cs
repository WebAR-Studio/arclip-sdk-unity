using NavigationDemo.UI.Base;
using UnityEngine;
using UnityEngine.UI;

namespace NavigationDemo.UI.Elements
{
    public class LoadingProgressElementView : BaseViewElement
    {
        [SerializeField] private Text _labelText;
        [SerializeField] private Text _percentText;
        [SerializeField] private RingGraphic _trackRingGraphic;
        [SerializeField] private RingGraphic _progressRingGraphic;
        [SerializeField] private string _defaultLabelValue = "Загрузка локаций";
        [SerializeField] [Range(0f, 1f)] private float _defaultProgressValue = 0.33f;

        public float ProgressValue => GetProgressValue();

        public string Value => GetValue();

        protected override void Awake()
        {
            base.Awake();
            SetLabelValue(_defaultLabelValue);
            SetProgressValue(_defaultProgressValue);
        }

        public void SetLabelValue(string value)
        {
            if (_labelText == null)
            {
                return;
            }

            _labelText.text = string.IsNullOrWhiteSpace(value) ? _defaultLabelValue : value;
        }

        public string GetLabelValue()
        {
            if (_labelText == null)
            {
                return string.Empty;
            }

            return _labelText.text;
        }

        public void SetProgressValue(float value)
        {
            float valueProgress = Mathf.Clamp01(value);

            if (_progressRingGraphic != null)
            {
                _progressRingGraphic.FillAmount = valueProgress;
            }

            if (_percentText != null)
            {
                int valuePercent = Mathf.RoundToInt(valueProgress * 100f);
                _percentText.text = $"{valuePercent}%";
            }
        }

        public float GetProgressValue()
        {
            if (_progressRingGraphic == null)
            {
                return 0f;
            }

            return _progressRingGraphic.FillAmount;
        }

        public void SetTrackColor(Color value)
        {
            if (_trackRingGraphic == null)
            {
                return;
            }

            _trackRingGraphic.color = value;
            _trackRingGraphic.SetVerticesDirty();
        }

        public void SetProgressColor(Color value)
        {
            if (_progressRingGraphic == null)
            {
                return;
            }

            _progressRingGraphic.color = value;
            _progressRingGraphic.SetVerticesDirty();
        }

        public override string GetValue()
        {
            return GetLabelValue();
        }
    }
}