using UnityEngine;

namespace NavigationDemo.UI.Base
{
    public abstract class BaseViewElement : MonoBehaviour
    {
        [SerializeField] protected bool _isVisible = true;

        public bool IsVisible => _isVisible;

        protected virtual void Awake()
        {
            _isVisible = gameObject.activeSelf;
        }

        public virtual void Show()
        {
            if (_isVisible)
            {
                return;
            }

            _isVisible = true;
            gameObject.SetActive(true);
            OnShown();
        }

        public virtual void Hide()
        {
            if (!_isVisible)
            {
                return;
            }

            _isVisible = false;
            gameObject.SetActive(false);
            OnHidden();
        }

        public virtual void Close()
        {
            Hide();
        }

        public virtual string GetValue()
        {
            return string.Empty;
        }

        protected virtual void OnShown()
        {
        }

        protected virtual void OnHidden()
        {
        }
    }
}
