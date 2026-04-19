using NavigationDemo.UI.Base;
using NavigationDemo.UI.Elements;
using UnityEngine;

namespace NavigationDemo.UI.Views
{
    [ExecuteAlways]
    public class FillCirclesHintView : BaseView
    {
        [SerializeField] private RectTransform _messageContainer;
        [SerializeField] private string _messageElementResourcePath = "UI/Elements/ModalMessageElement";
        [SerializeField] private Vector2 _messageElementSize = new Vector2(244f, 58f);
        [SerializeField] private bool _rebuildInEditMode = true;
        [SerializeField] private string _defaultMessageValue = "\u0417\u0430\u043f\u043e\u043b\u043d\u0438 \u043a\u0440\u0443\u0433\u0438 \u0432\u043e\u043a\u0440\u0443\u0433 \u0441\u0435\u0431\u044f";

        private ModalMessageElementView _modalMessageElementView;

        public string Value => GetMessageValue();

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

        public string GetMessageValue()
        {
            if (_modalMessageElementView == null)
            {
                return string.Empty;
            }

            return _modalMessageElementView.GetValue();
        }

        public void SetMessageValue(string value)
        {
            if (_modalMessageElementView == null)
            {
                return;
            }

            _modalMessageElementView.SetValue(value);
        }

        public void SetBackgroundColor(Color value)
        {
            if (_modalMessageElementView == null)
            {
                return;
            }

            _modalMessageElementView.SetBackgroundColor(value);
        }

        public Color GetBackgroundColor()
        {
            if (_modalMessageElementView == null)
            {
                return Color.clear;
            }

            return _modalMessageElementView.GetBackgroundColor();
        }

        private void BuildView()
        {
            BuildModalMessageElement();
            SetMessageValue(_defaultMessageValue);
        }

        private void BuildModalMessageElement()
        {
            if (_messageContainer == null)
            {
                return;
            }

            if (_modalMessageElementView == null)
            {
                _modalMessageElementView = _messageContainer.GetComponentInChildren<ModalMessageElementView>(true);
            }

            if (_modalMessageElementView == null)
            {
                ModalMessageElementView value = Resources.Load<ModalMessageElementView>(_messageElementResourcePath);
                if (value == null)
                {
                    return;
                }

                _modalMessageElementView = Instantiate(value, _messageContainer);
                _modalMessageElementView.name = value.name;
            }

            ApplyMessageElementLayout(_modalMessageElementView.GetComponent<RectTransform>());
        }

        private void ApplyMessageElementLayout(RectTransform value)
        {
            if (value == null)
            {
                return;
            }

            value.anchorMin = new Vector2(0.5f, 0.5f);
            value.anchorMax = new Vector2(0.5f, 0.5f);
            value.pivot = new Vector2(0.5f, 0.5f);
            value.anchoredPosition = Vector2.zero;
            value.sizeDelta = _messageElementSize;
            value.localScale = Vector3.one;
        }

        [ContextMenu("Rebuild View")]
        private void RebuildView()
        {
            BuildView();
        }
    }
}
