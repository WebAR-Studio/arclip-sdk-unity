using UnityEngine;

namespace NavigationDemo.Navigation
{
    [DisallowMultipleComponent]
    public class PathTarget : MonoBehaviour
    {
        [SerializeField] private string _key;
        [SerializeField] private string _title;
        [SerializeField] private int _floorId;
        [SerializeField] [Min(0.1f)] private float _arrivalRadius = 2f;

        public string Key => _key;
        public string Title => string.IsNullOrWhiteSpace(_title) ? gameObject.name : _title;
        public int FloorId => _floorId;
        public float ArrivalRadius => _arrivalRadius;
        public Vector3 Position => transform.position;

        public void SetData(string key, string title, int floorId, float arrivalRadius)
        {
            _key = key;
            _title = title;
            _floorId = floorId;
            _arrivalRadius = Mathf.Max(0.1f, arrivalRadius);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(_key))
            {
                _key = gameObject.name;
            }

            if (_arrivalRadius < 0.1f)
            {
                _arrivalRadius = 0.1f;
            }
        }
#endif
    }
}
