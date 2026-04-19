using NavigationDemo.UI.Base;
using UnityEngine;
using UnityEngine.UI;

namespace NavigationDemo.UI.Elements
{
    public class LoadingStatusBarElementView : BaseViewElement
    {
        [SerializeField] private Text _timeText;
        [SerializeField] private Text _signalText;
        [SerializeField] private Text _wifiText;
        [SerializeField] private Text _batteryText;
        [SerializeField] private string _defaultTimeValue = "9:15";
        [SerializeField] private string _defaultSignalValue = "???";
        [SerializeField] private string _defaultWifiValue = "?";
        [SerializeField] private string _defaultBatteryValue = "?";

        public string Value => GetTimeValue();

        protected override void Awake()
        {
            base.Awake();
            SetTimeValue(_defaultTimeValue);
            SetSignalValue(_defaultSignalValue);
            SetWifiValue(_defaultWifiValue);
            SetBatteryValue(_defaultBatteryValue);
        }

        public void SetTimeValue(string value)
        {
            if (_timeText == null)
            {
                return;
            }

            _timeText.text = string.IsNullOrWhiteSpace(value) ? _defaultTimeValue : value;
        }

        public string GetTimeValue()
        {
            if (_timeText == null)
            {
                return string.Empty;
            }

            return _timeText.text;
        }

        public void SetSignalValue(string value)
        {
            if (_signalText == null)
            {
                return;
            }

            _signalText.text = string.IsNullOrWhiteSpace(value) ? _defaultSignalValue : value;
        }

        public void SetWifiValue(string value)
        {
            if (_wifiText == null)
            {
                return;
            }

            _wifiText.text = string.IsNullOrWhiteSpace(value) ? _defaultWifiValue : value;
        }

        public void SetBatteryValue(string value)
        {
            if (_batteryText == null)
            {
                return;
            }

            _batteryText.text = string.IsNullOrWhiteSpace(value) ? _defaultBatteryValue : value;
        }

        public override string GetValue()
        {
            return Value;
        }
    }
}