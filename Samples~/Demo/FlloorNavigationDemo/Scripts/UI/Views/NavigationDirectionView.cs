using NavigationDemo.UI.Base;
using NavigationDemo.UI.Elements;
using UnityEngine;

namespace NavigationDemo.UI.Views
{
    [ExecuteAlways]
    public class NavigationDirectionView : BaseView
    {
        [SerializeField] private RectTransform _directionContainer;
        [SerializeField] private string _directionElementResourcePath = "UI/Elements/NavigationDirectionElement";
        [SerializeField] private Vector2 _directionElementSize = new Vector2(305f, 78f);
        [SerializeField] private bool _rebuildInEditMode = true;
        [SerializeField] private string _defaultIconValue = "\u2192";
        [SerializeField] private string _defaultTitleValue = "\u041f\u043e\u0432\u0435\u0440\u043d\u0438\u0442\u0435 \u043d\u0430\u043f\u0440\u0430\u0432\u043e";
        [SerializeField] private string _defaultDistanceValue = "5\u043c";

        private NavigationDirectionElementView _navigationDirectionElementView;

        public string Value => GetDirectionValue();

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

        public void SetDirection(string valueIcon, string valueTitle, string valueDistance)
        {
            if (_navigationDirectionElementView == null)
            {
                return;
            }

            _navigationDirectionElementView.SetDirection(valueIcon, valueTitle, valueDistance);
        }

        public void SetDirection(Sprite valueIconSprite, string valueIconFallback, string valueTitle, string valueDistance)
        {
            if (_navigationDirectionElementView == null)
            {
                return;
            }

            _navigationDirectionElementView.SetDirection(valueIconSprite, valueIconFallback, valueTitle, valueDistance);
        }

        public string GetDirectionValue()
        {
            if (_navigationDirectionElementView == null)
            {
                return string.Empty;
            }

            return _navigationDirectionElementView.GetDirectionValue();
        }

        public string GetIconValue()
        {
            if (_navigationDirectionElementView == null)
            {
                return string.Empty;
            }

            return _navigationDirectionElementView.GetIconValue();
        }

        public void SetIconValue(string value)
        {
            if (_navigationDirectionElementView == null)
            {
                return;
            }

            _navigationDirectionElementView.SetIconValue(value);
        }

        public void SetIconSprite(Sprite value)
        {
            if (_navigationDirectionElementView == null)
            {
                return;
            }

            _navigationDirectionElementView.SetIconSprite(value);
        }

        public string GetTitleValue()
        {
            if (_navigationDirectionElementView == null)
            {
                return string.Empty;
            }

            return _navigationDirectionElementView.GetTitleValue();
        }

        public void SetTitleValue(string value)
        {
            if (_navigationDirectionElementView == null)
            {
                return;
            }

            _navigationDirectionElementView.SetTitleValue(value);
        }

        public string GetDistanceValue()
        {
            if (_navigationDirectionElementView == null)
            {
                return string.Empty;
            }

            return _navigationDirectionElementView.GetDistanceValue();
        }

        public void SetDistanceValue(string value)
        {
            if (_navigationDirectionElementView == null)
            {
                return;
            }

            _navigationDirectionElementView.SetDistanceValue(value);
        }

        private void BuildView()
        {
            BuildDirectionElement();
            SetDirection(_defaultIconValue, _defaultTitleValue, _defaultDistanceValue);
        }

        private void BuildDirectionElement()
        {
            if (_directionContainer == null)
            {
                return;
            }

            if (_navigationDirectionElementView == null)
            {
                _navigationDirectionElementView = _directionContainer.GetComponentInChildren<NavigationDirectionElementView>(true);
            }

            if (_navigationDirectionElementView == null)
            {
                NavigationDirectionElementView value = Resources.Load<NavigationDirectionElementView>(_directionElementResourcePath);
                if (value == null)
                {
                    return;
                }

                _navigationDirectionElementView = Instantiate(value, _directionContainer);
                _navigationDirectionElementView.name = value.name;
            }

            ApplyDirectionElementLayout(_navigationDirectionElementView.GetComponent<RectTransform>());
        }

        private void ApplyDirectionElementLayout(RectTransform value)
        {
            if (value == null)
            {
                return;
            }

            value.anchorMin = new Vector2(0.5f, 0.5f);
            value.anchorMax = new Vector2(0.5f, 0.5f);
            value.pivot = new Vector2(0.5f, 0.5f);
            value.anchoredPosition = Vector2.zero;
            value.sizeDelta = _directionElementSize;
            value.localScale = Vector3.one;
        }

        [ContextMenu("Rebuild View")]
        private void RebuildView()
        {
            BuildView();
        }
    }
}
