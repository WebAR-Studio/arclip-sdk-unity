using System;
using System.Collections;
using NavigationDemo.UI.Base;
using NavigationDemo.UI.Elements;
using UnityEngine;

namespace NavigationDemo.UI.Views
{
    [ExecuteAlways]
    public class FloorTransitionPromptView : BaseView
    {
        [SerializeField] private RectTransform _contentContainer;
        [SerializeField] private string _contentElementResourcePath = "UI/Elements/FloorTransitionPromptElement";
        [SerializeField] private Vector2 _contentElementSize = new Vector2(328f, 240f);
        [SerializeField] [Min(0f)] private float _doneButtonLockDuration = 2f;
        [SerializeField] private bool _rebuildInEditMode = true;
        [SerializeField] private Sprite _defaultIconSprite;
        [SerializeField] private string _defaultIconValue = "\u21CA";
        [SerializeField] private string _defaultSubtitleValue = "\u0427\u0442\u043E\u0431\u044B \u043F\u0440\u043E\u0434\u043E\u043B\u0436\u0438\u0442\u044C,";
        [SerializeField] private string _defaultTitleValue = "\u0421\u043F\u0443\u0441\u0442\u0438\u0442\u0435\u0441\u044C \u043D\u0430 2 \u044D\u0442\u0430\u0436";
        [SerializeField] private string _defaultDoneButtonValue = "\u0413\u043E\u0442\u043E\u0432\u043E!";
        [SerializeField] private string _defaultFinishButtonValue = "\u0417\u0430\u0432\u0435\u0440\u0448\u0438\u0442\u044C";

        private FloorTransitionPromptElementView _floorTransitionPromptElementView;
        private Coroutine _doneButtonUnlockCoroutine;

        public event Action<FloorTransitionPromptView> DoneClicked;
        public event Action<FloorTransitionPromptView> FinishClicked;

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

            if (Application.isPlaying)
            {
                RestartDoneButtonLock();
            }
        }

        private void OnDisable()
        {
            if (_doneButtonUnlockCoroutine == null)
            {
                return;
            }

            StopCoroutine(_doneButtonUnlockCoroutine);
            _doneButtonUnlockCoroutine = null;
        }

        private void OnValidate()
        {
            if (!Application.isPlaying && !_rebuildInEditMode)
            {
                return;
            }

            BuildView();
        }

        public string GetSubtitleValue()
        {
            if (_floorTransitionPromptElementView == null)
            {
                return string.Empty;
            }

            return _floorTransitionPromptElementView.GetSubtitleValue();
        }

        public void SetSubtitleValue(string value)
        {
            if (_floorTransitionPromptElementView == null)
            {
                return;
            }

            _floorTransitionPromptElementView.SetSubtitleValue(value);
        }

        public string GetTitleValue()
        {
            if (_floorTransitionPromptElementView == null)
            {
                return string.Empty;
            }

            return _floorTransitionPromptElementView.GetTitleValue();
        }

        public void SetTitleValue(string value)
        {
            if (_floorTransitionPromptElementView == null)
            {
                return;
            }

            _floorTransitionPromptElementView.SetTitleValue(value);
        }

        public void SetIconSprite(Sprite value)
        {
            if (_floorTransitionPromptElementView == null)
            {
                return;
            }

            _floorTransitionPromptElementView.SetIconSprite(value);
        }

        public void SetIconFallbackValue(string value)
        {
            if (_floorTransitionPromptElementView == null)
            {
                return;
            }

            _floorTransitionPromptElementView.SetIconFallbackValue(value);
        }

        public void SetDoneButtonValue(string value)
        {
            if (_floorTransitionPromptElementView == null)
            {
                return;
            }

            _floorTransitionPromptElementView.SetDoneButtonValue(value);
        }

        public void SetFinishButtonValue(string value)
        {
            if (_floorTransitionPromptElementView == null)
            {
                return;
            }

            _floorTransitionPromptElementView.SetFinishButtonValue(value);
        }

        public void SetDoneButtonInteractable(bool value)
        {
            if (_floorTransitionPromptElementView == null)
            {
                return;
            }

            _floorTransitionPromptElementView.SetDoneButtonInteractable(value);
        }

        public void SetFinishButtonInteractable(bool value)
        {
            if (_floorTransitionPromptElementView == null)
            {
                return;
            }

            _floorTransitionPromptElementView.SetFinishButtonInteractable(value);
        }

        public void SetContent(
            Sprite valueIconSprite,
            string valueIconFallback,
            string valueSubtitle,
            string valueTitle)
        {
            if (_floorTransitionPromptElementView == null)
            {
                return;
            }

            _floorTransitionPromptElementView.SetContent(
                valueIconSprite,
                valueIconFallback,
                valueSubtitle,
                valueTitle);
        }

        public void RestartDoneButtonLock()
        {
            if (!Application.isPlaying || _floorTransitionPromptElementView == null)
            {
                return;
            }

            _floorTransitionPromptElementView.SetDoneButtonInteractable(false);

            if (_doneButtonUnlockCoroutine != null)
            {
                StopCoroutine(_doneButtonUnlockCoroutine);
            }

            _doneButtonUnlockCoroutine = StartCoroutine(UnlockDoneButtonRoutine());
        }

        private IEnumerator UnlockDoneButtonRoutine()
        {
            if (_doneButtonLockDuration > 0f)
            {
                yield return new WaitForSeconds(_doneButtonLockDuration);
            }

            if (_floorTransitionPromptElementView != null)
            {
                _floorTransitionPromptElementView.SetDoneButtonInteractable(true);
            }

            _doneButtonUnlockCoroutine = null;
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

            if (_floorTransitionPromptElementView == null)
            {
                _floorTransitionPromptElementView = _contentContainer.GetComponentInChildren<FloorTransitionPromptElementView>(true);
            }

            if (_floorTransitionPromptElementView == null)
            {
                FloorTransitionPromptElementView value = Resources.Load<FloorTransitionPromptElementView>(_contentElementResourcePath);
                if (value == null)
                {
                    return;
                }

                _floorTransitionPromptElementView = Instantiate(value, _contentContainer);
                _floorTransitionPromptElementView.name = value.name;
            }

            ApplyContentElementLayout(_floorTransitionPromptElementView.GetComponent<RectTransform>());
            _floorTransitionPromptElementView.DoneClicked -= HandleDoneClicked;
            _floorTransitionPromptElementView.DoneClicked += HandleDoneClicked;
            _floorTransitionPromptElementView.FinishClicked -= HandleFinishClicked;
            _floorTransitionPromptElementView.FinishClicked += HandleFinishClicked;
        }

        private void ApplyDefaultState()
        {
            if (_floorTransitionPromptElementView == null)
            {
                return;
            }

            _floorTransitionPromptElementView.SetContent(
                _defaultIconSprite,
                _defaultIconValue,
                _defaultSubtitleValue,
                _defaultTitleValue);

            _floorTransitionPromptElementView.SetDoneButtonValue(_defaultDoneButtonValue);
            _floorTransitionPromptElementView.SetFinishButtonValue(_defaultFinishButtonValue);
        }

        private void ApplyContentElementLayout(RectTransform value)
        {
            if (value == null)
            {
                return;
            }

            value.anchorMin = new Vector2(0.5f, 1f);
            value.anchorMax = new Vector2(0.5f, 1f);
            value.pivot = new Vector2(0.5f, 1f);
            value.anchoredPosition = new Vector2(0f, -16f);
            value.sizeDelta = _contentElementSize;
            value.localScale = Vector3.one;
        }

        private void HandleDoneClicked(FloorTransitionPromptElementView value)
        {
            DoneClicked?.Invoke(this);
        }

        private void HandleFinishClicked(FloorTransitionPromptElementView value)
        {
            FinishClicked?.Invoke(this);
        }

        [ContextMenu("Rebuild View")]
        private void RebuildView()
        {
            BuildView();
        }
    }
}
