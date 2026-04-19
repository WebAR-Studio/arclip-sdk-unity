using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace NavigationDemo.Navigation
{
    [DisallowMultipleComponent]
    public class PathShowerArrows : MonoBehaviour, IPathShower
    {
        [SerializeField] private Transform _startPoint;
        [SerializeField] private GameObject _arrowPrefab;
        [SerializeField] [Min(0.1f)] private float _arrowSpacing = 0.6f;
        [SerializeField] [Min(0.01f)] private float _averageSpeedMetersPerSecond = 1.4f;
        [SerializeField] [Min(0.02f)] private float _updateInterval = 0.15f;
        [SerializeField] [Min(0.05f)] private float _sampleDistance = 2f;
        [SerializeField] private float _arrowHeightOffset = 0.05f;

        [Header("Route Rebuild Filtering")]
        [SerializeField] private bool _enableRebuildFiltering = true;
        [SerializeField] [Min(0f)] private float _minDistanceForRebuildMeters = 5f;
        [SerializeField] [Min(1)] private int _requiredConsecutiveVpsLocalizations = 2;

        private readonly List<GameObject> _arrowPool = new List<GameObject>();
        private NavMeshPath _path;
        private Coroutine _updateCoroutine;
        private Vector3 _targetPosition;
        private bool _hasTarget;
        private bool _isVisible;
        private bool _isActive;
        private bool _wasOnNavMesh;
        private bool _warnedAboutArrowPrefab;
        private int _activeArrowCount;
        private int _consecutiveVpsLocalizations;
        private bool _hasLastRebuildNavPoint;
        private Vector3 _lastRebuildNavPoint;
        private bool _forceRebuildOnNextUpdate = true;

        public event Action LeftNavMesh;

        public bool IsVisible => _isVisible;

        private void Awake()
        {
            _path = new NavMeshPath();

            if (_startPoint == null)
            {
                _startPoint = transform;
            }
        }

        public void SetTarget(Vector3 targetPosition)
        {
            _targetPosition = targetPosition;
            _hasTarget = true;
            _hasLastRebuildNavPoint = false;
            _forceRebuildOnNextUpdate = true;
        }

        public void ShowPath(bool visible)
        {
            _isVisible = visible;
            _isActive = true;
            _forceRebuildOnNextUpdate = true;

            if (!_isVisible)
            {
                SetActiveArrowCount(0);
            }

            if (_updateCoroutine == null)
            {
                _updateCoroutine = StartCoroutine(UpdatePathRoutine());
            }
        }

        public void HidePath()
        {
            _isVisible = false;
            _isActive = false;
            _hasTarget = false;
            _wasOnNavMesh = false;
            _hasLastRebuildNavPoint = false;
            _consecutiveVpsLocalizations = 0;
            _forceRebuildOnNextUpdate = true;
            SetActiveArrowCount(0);

            if (_updateCoroutine != null)
            {
                StopCoroutine(_updateCoroutine);
                _updateCoroutine = null;
            }
        }

        public float GetDistanceToTarget()
        {
            if (_path == null || _path.corners == null || _path.corners.Length < 2)
            {
                return 0f;
            }

            float distance = 0f;
            for (int i = 0; i < _path.corners.Length - 1; i++)
            {
                distance += Vector3.Distance(_path.corners[i], _path.corners[i + 1]);
            }

            return distance;
        }

        public float GetTimeToTarget()
        {
            if (_averageSpeedMetersPerSecond <= 0.01f)
            {
                return float.PositiveInfinity;
            }

            return GetDistanceToTarget() / _averageSpeedMetersPerSecond;
        }

        public bool TryGetPathCorners(List<Vector3> cornersBuffer)
        {
            if (cornersBuffer == null)
            {
                return false;
            }

            cornersBuffer.Clear();

            if (_path == null || _path.corners == null || _path.corners.Length < 2)
            {
                return false;
            }

            cornersBuffer.AddRange(_path.corners);
            return true;
        }

        public void NotifyVpsLocalizationSuccess()
        {
            _consecutiveVpsLocalizations++;
        }

        public void NotifyVpsLocalizationFailure()
        {
            _consecutiveVpsLocalizations = 0;
        }

        private IEnumerator UpdatePathRoutine()
        {
            var wait = new WaitForSeconds(_updateInterval);

            while (_isActive)
            {
                UpdatePath();
                yield return wait;
            }

            _updateCoroutine = null;
        }

        private void UpdatePath()
        {
            if (!_hasTarget || _startPoint == null || _path == null)
            {
                if (_isVisible)
                {
                    SetActiveArrowCount(0);
                }

                return;
            }

            if (!TryGetNavMeshPoint(_startPoint.position, out Vector3 navStart))
            {
                if (_wasOnNavMesh)
                {
                    LeftNavMesh?.Invoke();
                }

                _wasOnNavMesh = false;

                if (_isVisible)
                {
                    SetActiveArrowCount(0);
                }

                return;
            }

            _wasOnNavMesh = true;

            if (!ShouldRebuild(navStart))
            {
                return;
            }

            Vector3 navTarget = _targetPosition;
            if (TryGetNavMeshPoint(_targetPosition, out Vector3 sampledTarget))
            {
                navTarget = sampledTarget;
            }

            bool calculated = NavMesh.CalculatePath(navStart, navTarget, NavMesh.AllAreas, _path);
            bool hasValidCorners = calculated &&
                                   _path.status != NavMeshPathStatus.PathInvalid &&
                                   _path.corners != null &&
                                   _path.corners.Length > 1;

            if (!hasValidCorners)
            {
                if (_isVisible)
                {
                    SetActiveArrowCount(0);
                }

                return;
            }

            CommitRebuild(navStart);

            if (_isVisible)
            {
                RenderArrows(_path);
            }
            else if (_activeArrowCount > 0)
            {
                SetActiveArrowCount(0);
            }
        }

        private bool TryGetNavMeshPoint(Vector3 worldPoint, out Vector3 navPoint)
        {
            if (NavMesh.SamplePosition(worldPoint, out NavMeshHit hit, _sampleDistance, NavMesh.AllAreas))
            {
                navPoint = hit.position;
                return true;
            }

            navPoint = worldPoint;
            return false;
        }

        private bool ShouldRebuild(Vector3 navStart)
        {
            if (_forceRebuildOnNextUpdate)
            {
                return true;
            }

            if (!_enableRebuildFiltering)
            {
                return true;
            }

            if (!_hasLastRebuildNavPoint)
            {
                return true;
            }

            if (Vector3.Distance(_lastRebuildNavPoint, navStart) < _minDistanceForRebuildMeters)
            {
                return false;
            }

            int requiredSuccesses = Mathf.Max(1, _requiredConsecutiveVpsLocalizations);
            return _consecutiveVpsLocalizations >= requiredSuccesses;
        }

        private void CommitRebuild(Vector3 navStart)
        {
            _lastRebuildNavPoint = navStart;
            _hasLastRebuildNavPoint = true;
            _forceRebuildOnNextUpdate = false;
            _consecutiveVpsLocalizations = 0;
        }

        private void RenderArrows(NavMeshPath navPath)
        {
            int arrowIndex = 0;

            for (int i = 0; i < navPath.corners.Length - 1; i++)
            {
                Vector3 start = navPath.corners[i];
                Vector3 end = navPath.corners[i + 1];

                float segmentDistance = Vector3.Distance(start, end);
                if (segmentDistance <= 0.01f)
                {
                    continue;
                }

                Vector3 direction = (end - start).normalized;
                int segmentArrowCount = Mathf.Max(1, Mathf.FloorToInt(segmentDistance / _arrowSpacing));

                for (int j = 0; j < segmentArrowCount; j++)
                {
                    float distanceOnSegment = Mathf.Min(j * _arrowSpacing, segmentDistance - 0.02f);
                    Vector3 position = start + direction * distanceOnSegment + Vector3.up * _arrowHeightOffset;
                    Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);

                    GameObject arrow = GetOrCreateArrow(arrowIndex);
                    if (arrow == null)
                    {
                        SetActiveArrowCount(0);
                        return;
                    }

                    arrow.transform.SetPositionAndRotation(position, rotation);
                    arrowIndex++;
                }
            }

            SetActiveArrowCount(arrowIndex);
        }

        private GameObject GetOrCreateArrow(int index)
        {
            while (_arrowPool.Count <= index)
            {
                if (_arrowPrefab == null)
                {
                    if (!_warnedAboutArrowPrefab)
                    {
                        Debug.LogWarning(
                            $"[{nameof(PathShowerArrows)}] Arrow prefab is not set. Path will not be displayed.",
                            this);
                        _warnedAboutArrowPrefab = true;
                    }

                    return null;
                }

                GameObject arrow = Instantiate(_arrowPrefab, transform);
                arrow.SetActive(false);
                _arrowPool.Add(arrow);
            }

            GameObject value = _arrowPool[index];
            if (!value.activeSelf)
            {
                value.SetActive(true);
            }

            return value;
        }

        private void SetActiveArrowCount(int count)
        {
            int clampedCount = Mathf.Max(0, count);
            int maxToDisable = Mathf.Min(_activeArrowCount, _arrowPool.Count);

            for (int i = clampedCount; i < maxToDisable; i++)
            {
                GameObject arrow = _arrowPool[i];
                if (arrow != null && arrow.activeSelf)
                {
                    arrow.SetActive(false);
                }
            }

            _activeArrowCount = clampedCount;
        }
    }
}
