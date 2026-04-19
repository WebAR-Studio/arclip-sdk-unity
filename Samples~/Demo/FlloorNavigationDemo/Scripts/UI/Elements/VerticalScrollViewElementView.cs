using NavigationDemo.UI.Base;
using UnityEngine;
using UnityEngine.UI;

namespace NavigationDemo.UI.Elements
{
    [ExecuteAlways]
    public class VerticalScrollViewElementView : BaseViewElement
    {
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private RectTransform _viewport;
        [SerializeField] private RectTransform _content;
        [SerializeField] private bool _vertical = true;
        [SerializeField] private bool _horizontal;

        public RectTransform Content => _content;

        protected override void Awake()
        {
            base.Awake();
            EnsureScrollView();
            ApplyAxisState();
        }

        public RectTransform GetContent()
        {
            EnsureScrollView();
            return _content;
        }

        public void SetContent(RectTransform value)
        {
            _content = value;
            EnsureScrollView();
            if (_scrollRect == null)
            {
                return;
            }

            _scrollRect.content = value;
        }

        public void SetVerticalNormalizedPosition(float value)
        {
            EnsureScrollView();
            if (_scrollRect == null)
            {
                return;
            }

            _scrollRect.verticalNormalizedPosition = Mathf.Clamp01(value);
        }

        public float GetVerticalNormalizedPosition()
        {
            EnsureScrollView();
            if (_scrollRect == null)
            {
                return 1f;
            }

            return _scrollRect.verticalNormalizedPosition;
        }

        public void ScrollToTop()
        {
            SetVerticalNormalizedPosition(1f);
        }

        public void ScrollToBottom()
        {
            SetVerticalNormalizedPosition(0f);
        }

        public void SetScrollEnabled(bool value)
        {
            EnsureScrollView();
            if (_scrollRect == null)
            {
                return;
            }

            _scrollRect.enabled = value;
        }

        public override string GetValue()
        {
            return _content != null ? _content.childCount.ToString() : "0";
        }

        private void OnValidate()
        {
            EnsureScrollView();
            ApplyAxisState();
        }

        private void EnsureScrollView()
        {
            if (_scrollRect == null)
            {
                _scrollRect = GetComponent<ScrollRect>();
            }

            if (_scrollRect == null)
            {
                if (!CanModifyHierarchy())
                {
                    return;
                }

                _scrollRect = gameObject.AddComponent<ScrollRect>();
            }

            if (_viewport == null)
            {
                Transform value = transform.Find("Viewport");
                _viewport = value as RectTransform;
            }

            if (_viewport == null)
            {
                if (!CanModifyHierarchy())
                {
                    return;
                }

                GameObject value = new GameObject("Viewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Mask));
                _viewport = value.GetComponent<RectTransform>();
                _viewport.SetParent(transform, false);
                StretchToParent(_viewport);

                Image valueImage = value.GetComponent<Image>();
                if (valueImage != null)
                {
                    valueImage.color = new Color(1f, 1f, 1f, 0.01f);
                    valueImage.raycastTarget = true;
                }

                Mask valueMask = value.GetComponent<Mask>();
                if (valueMask != null)
                {
                    valueMask.showMaskGraphic = false;
                }
            }

            if (_content == null)
            {
                Transform value = _viewport.Find("Content");
                _content = value as RectTransform;
            }

            if (_content == null)
            {
                if (!CanModifyHierarchy())
                {
                    return;
                }

                GameObject value = new GameObject("Content", typeof(RectTransform));
                _content = value.GetComponent<RectTransform>();
                _content.SetParent(_viewport, false);
            }

            _content.anchorMin = new Vector2(0f, 1f);
            _content.anchorMax = new Vector2(1f, 1f);
            _content.pivot = new Vector2(0.5f, 1f);
            _content.anchoredPosition = Vector2.zero;
            _content.sizeDelta = new Vector2(0f, Mathf.Max(0f, _content.sizeDelta.y));

            _scrollRect.viewport = _viewport;
            _scrollRect.content = _content;
            _scrollRect.movementType = ScrollRect.MovementType.Clamped;
        }

        private void ApplyAxisState()
        {
            EnsureScrollView();

            if (_scrollRect == null)
            {
                return;
            }

            _scrollRect.vertical = _vertical;
            _scrollRect.horizontal = _horizontal;
            _scrollRect.viewport = _viewport;
            _scrollRect.content = _content;
        }

        private bool CanModifyHierarchy()
        {
            if (Application.isPlaying)
            {
                return true;
            }

            return gameObject.scene.IsValid();
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
    }
}
