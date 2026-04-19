using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace NavigationDemo.Navigation
{
    [DisallowMultipleComponent]
    public class TargetManager : MonoBehaviour
    {
        private enum RouteStage
        {
            None,
            ToTransitionPoint,
            WaitForFloorTransition,
            ToDestination
        }

        [Header("References")]
        [SerializeField] private Transform _startPoint;
        [SerializeField] private PathShowerArrows _pathShower;
        [SerializeField] private List<PathTarget> _targets = new List<PathTarget>();
        [SerializeField] private List<FloorTransitionPoint> _transitionPoints = new List<FloorTransitionPoint>();

        [Header("Transition Routing")]
        [SerializeField] private TransitionPointKind _preferredTransitionKind = TransitionPointKind.Stairs;
        [SerializeField] private bool _fallbackToOtherTransitionKind = true;
        [SerializeField] private bool _autoContinueAfterTransition;

        [Header("Route Update")]
        [SerializeField] [Min(0.05f)] private float _updateInterval = 0.2f;
        [SerializeField] [Min(0.1f)] private float _navMeshSampleDistance = 2f;

        [Header("Direction Detection")]
        [SerializeField] [Min(0.1f)] private float _directionLookAheadDistance = 1.2f;
        [SerializeField] [Range(0f, 89f)] private float _straightAngleThreshold = 30f;
        [SerializeField] [Range(90f, 179f)] private float _backAngleThreshold = 140f;

        [Header("Floor Detection")]
        [SerializeField] private bool _detectFloorByHeight = true;
        [SerializeField] private int _currentFloorId;
        [SerializeField] private List<FloorHeightRange> _floorRanges = new List<FloorHeightRange>();

        [Header("Runtime State")]
        public FloorTransitionInstruction CurrentTransitionInstruction = FloorTransitionInstruction.Up;
        public RouteTurnDirection CurrentTurnDirection = RouteTurnDirection.Straight;

        [Header("Unity Events")]
        public UnityEvent PathComplete;
        public UnityEvent PathFailed;
        public UnityEvent TransitionPointArrived;

        public event Action<float, float, PathTarget> RouteMetricsChanged;
        public event Action<RouteTurnDirection> DirectionChanged;
        public event Action<RouteTurnDirection> DirectionChangedToTransitionPoint;
        public event Action<FloorTransitionPoint, FloorTransitionInstruction> TransitionReached;
        public event Action<PathTarget> FullRouteCompleted;
        public event Action RouteFailedEvent;

        private NavMeshPath _tmpPath;
        private readonly List<Vector3> _cornersBuffer = new List<Vector3>();

        private PathTarget _destinationTarget;
        private FloorTransitionPoint _activeTransitionPoint;
        private RouteStage _routeStage = RouteStage.None;
        private Coroutine _routeCoroutine;
        private bool _showArrows = true;
        private float _activeArrivalDistance;

        public int CurrentFloorId => _currentFloorId;
        public PathTarget DestinationTarget => _destinationTarget;
        public bool IsRouting => _routeStage != RouteStage.None;
        public bool IsFloorAutoDetectionEnabled => _detectFloorByHeight;

        public float GetDistanceToTarget()
        {
            return _pathShower == null ? 0f : _pathShower.GetDistanceToTarget();
        }

        public float GetTimeToTarget()
        {
            return _pathShower == null ? 0f : _pathShower.GetTimeToTarget();
        }

        private void Awake()
        {
            _tmpPath ??= new NavMeshPath();

            if (_startPoint == null)
            {
                _startPoint = transform;
            }

            if (_pathShower == null)
            {
                _pathShower = GetComponent<PathShowerArrows>();
            }

            if (_pathShower != null)
            {
                _pathShower.LeftNavMesh += HandleLeftNavMesh;
            }

            UpdateCurrentFloorFromHeight();
        }

        private void OnDestroy()
        {
            if (_pathShower != null)
            {
                _pathShower.LeftNavMesh -= HandleLeftNavMesh;
            }
        }

        public void AddTarget(PathTarget target)
        {
            if (target == null || _targets.Contains(target))
            {
                return;
            }

            _targets.Add(target);
        }

        public void SetCurrentFloor(int floorId)
        {
            _currentFloorId = floorId;
        }

        public void ShowPath(string targetKey, bool showArrows = true)
        {
            PathTarget target = FindTarget(targetKey);
            if (target == null)
            {
                Debug.LogWarning($"[{nameof(TargetManager)}] Target with key '{targetKey}' was not found.", this);
                RaisePathFailed();
                return;
            }

            ShowPath(target, showArrows);
        }

        public void ShowPath(PathTarget target, bool showArrows = true)
        {
            if (target == null || _pathShower == null || _startPoint == null)
            {
                RaisePathFailed();
                return;
            }

            _destinationTarget = target;
            _showArrows = showArrows;
            CurrentTurnDirection = RouteTurnDirection.Straight;

            if (!BuildNextLeg())
            {
                RaisePathFailed();
                return;
            }

            _pathShower.ShowPath(_showArrows);
            EnsureRouteCoroutineRunning();
        }

        public void HidePath()
        {
            StopRoute(clearDestination: true);
        }

        public void ContinueAfterTransition()
        {
            if (_routeStage != RouteStage.WaitForFloorTransition || _destinationTarget == null)
            {
                return;
            }

            if (!BuildNextLeg())
            {
                RaisePathFailed();
                return;
            }

            _pathShower.ShowPath(_showArrows);
            EnsureRouteCoroutineRunning();
        }

        public void ContinueAfterTransition(int newFloorId)
        {
            _currentFloorId = newFloorId;
            ContinueAfterTransition();
        }

        public void ContinueAfterTransitionUsingInstruction()
        {
            int nextFloor = _currentFloorId;
            if (CurrentTransitionInstruction == FloorTransitionInstruction.Up)
            {
                nextFloor++;
            }
            else if (CurrentTransitionInstruction == FloorTransitionInstruction.Down)
            {
                nextFloor--;
            }

            ContinueAfterTransition(nextFloor);
        }

        private PathTarget FindTarget(string targetKey)
        {
            if (string.IsNullOrEmpty(targetKey))
            {
                return null;
            }

            for (int i = 0; i < _targets.Count; i++)
            {
                PathTarget candidate = _targets[i];
                if (candidate == null)
                {
                    continue;
                }

                if (candidate.Key == targetKey)
                {
                    return candidate;
                }
            }

            return null;
        }

        private bool BuildNextLeg()
        {
            UpdateCurrentFloorFromHeight();

            if (_destinationTarget == null)
            {
                return false;
            }

            if (_currentFloorId == _destinationTarget.FloorId)
            {
                _routeStage = RouteStage.ToDestination;
                _activeArrivalDistance = _destinationTarget.ArrivalRadius;
                _activeTransitionPoint = null;
                _pathShower.SetTarget(_destinationTarget.Position);
                return true;
            }

            if (!TryFindBestTransitionPoint(_currentFloorId, _destinationTarget.FloorId, out FloorTransitionPoint transitionPoint))
            {
                return false;
            }

            _activeTransitionPoint = transitionPoint;
            _activeArrivalDistance = transitionPoint.ArrivalRadius;
            CurrentTransitionInstruction = transitionPoint.GetInstruction(_destinationTarget.FloorId);
            _routeStage = RouteStage.ToTransitionPoint;
            _pathShower.SetTarget(transitionPoint.Position);
            return true;
        }

        private bool TryFindBestTransitionPoint(
            int currentFloorId,
            int destinationFloorId,
            out FloorTransitionPoint bestPoint)
        {
            bestPoint = null;
            float bestDistance = float.PositiveInfinity;

            bool foundPreferred = TryFindBestTransitionPointInternal(
                currentFloorId,
                destinationFloorId,
                _preferredTransitionKind,
                out bestPoint,
                out bestDistance);

            if (foundPreferred || !_fallbackToOtherTransitionKind)
            {
                return foundPreferred;
            }

            return TryFindBestTransitionPointInternal(
                currentFloorId,
                destinationFloorId,
                null,
                out bestPoint,
                out bestDistance);
        }

        private bool TryFindBestTransitionPointInternal(
            int currentFloorId,
            int destinationFloorId,
            TransitionPointKind? requiredKind,
            out FloorTransitionPoint bestPoint,
            out float bestDistance)
        {
            bestPoint = null;
            bestDistance = float.PositiveInfinity;

            for (int i = 0; i < _transitionPoints.Count; i++)
            {
                FloorTransitionPoint candidate = _transitionPoints[i];
                if (candidate == null || candidate.FloorId != currentFloorId)
                {
                    continue;
                }

                if (requiredKind.HasValue && candidate.Kind != requiredKind.Value)
                {
                    continue;
                }

                if (!candidate.CanMoveTowards(destinationFloorId))
                {
                    continue;
                }

                float pathDistance = CalculatePathDistance(candidate.Position);
                if (float.IsInfinity(pathDistance))
                {
                    continue;
                }

                if (pathDistance < bestDistance)
                {
                    bestDistance = pathDistance;
                    bestPoint = candidate;
                }
            }

            return bestPoint != null;
        }

        private float CalculatePathDistance(Vector3 targetPoint)
        {
            if (_startPoint == null)
            {
                return float.PositiveInfinity;
            }

            _tmpPath ??= new NavMeshPath();

            if (!TryGetNavMeshPoint(_startPoint.position, out Vector3 navStart))
            {
                return float.PositiveInfinity;
            }

            Vector3 navTarget = targetPoint;
            if (TryGetNavMeshPoint(targetPoint, out Vector3 sampledTarget))
            {
                navTarget = sampledTarget;
            }

            bool hasPath = NavMesh.CalculatePath(navStart, navTarget, NavMesh.AllAreas, _tmpPath);
            bool validPath = hasPath &&
                             _tmpPath.status != NavMeshPathStatus.PathInvalid &&
                             _tmpPath.corners != null &&
                             _tmpPath.corners.Length > 1;
            if (!validPath)
            {
                return float.PositiveInfinity;
            }

            float distance = 0f;
            for (int i = 0; i < _tmpPath.corners.Length - 1; i++)
            {
                distance += Vector3.Distance(_tmpPath.corners[i], _tmpPath.corners[i + 1]);
            }

            return distance;
        }

        private bool TryGetNavMeshPoint(Vector3 worldPoint, out Vector3 navPoint)
        {
            if (NavMesh.SamplePosition(worldPoint, out NavMeshHit hit, _navMeshSampleDistance, NavMesh.AllAreas))
            {
                navPoint = hit.position;
                return true;
            }

            navPoint = worldPoint;
            return false;
        }

        private void EnsureRouteCoroutineRunning()
        {
            if (_routeCoroutine != null)
            {
                return;
            }

            _routeCoroutine = StartCoroutine(RouteRoutine());
        }

        private IEnumerator RouteRoutine()
        {
            var wait = new WaitForSeconds(_updateInterval);

            while (_routeStage == RouteStage.ToTransitionPoint || _routeStage == RouteStage.ToDestination)
            {
                float distance = _pathShower.GetDistanceToTarget();
                float time = _pathShower.GetTimeToTarget();
                RouteMetricsChanged?.Invoke(distance, time, _destinationTarget);

                if (_routeStage == RouteStage.ToTransitionPoint || _routeStage == RouteStage.ToDestination)
                {
                    UpdateDirectionForCurrentLeg();
                }

                if (distance > 0f && distance <= _activeArrivalDistance)
                {
                    HandleLegReached();
                }

                yield return wait;
            }

            _routeCoroutine = null;
        }

        private void HandleLegReached()
        {
            if (_routeStage == RouteStage.ToTransitionPoint)
            {
                _routeStage = RouteStage.WaitForFloorTransition;
                _pathShower.HidePath();
                TransitionPointArrived?.Invoke();
                TransitionReached?.Invoke(_activeTransitionPoint, CurrentTransitionInstruction);

                if (_autoContinueAfterTransition)
                {
                    ContinueAfterTransition();
                }

                return;
            }

            if (_routeStage != RouteStage.ToDestination)
            {
                return;
            }

            PathComplete?.Invoke();
            FullRouteCompleted?.Invoke(_destinationTarget);
            StopRoute(clearDestination: true);
        }

        private void UpdateDirectionForCurrentLeg()
        {
            if (_startPoint == null || !_pathShower.TryGetPathCorners(_cornersBuffer))
            {
                return;
            }

            Vector3 lookDirection = GetLookDirection(_cornersBuffer, _startPoint.position, _directionLookAheadDistance);
            if (lookDirection.sqrMagnitude < 0.0001f)
            {
                return;
            }

            Vector3 forward = _startPoint.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f)
            {
                return;
            }

            forward.Normalize();
            lookDirection.Normalize();

            float signedAngle = Vector3.SignedAngle(forward, lookDirection, Vector3.up);
            float absoluteAngle = Mathf.Abs(signedAngle);

            RouteTurnDirection nextDirection;
            if (absoluteAngle <= _straightAngleThreshold)
            {
                nextDirection = RouteTurnDirection.Straight;
            }
            else if (absoluteAngle >= _backAngleThreshold)
            {
                nextDirection = RouteTurnDirection.Back;
            }
            else
            {
                nextDirection = signedAngle > 0f ? RouteTurnDirection.Right : RouteTurnDirection.Left;
            }

            if (nextDirection == CurrentTurnDirection)
            {
                return;
            }

            CurrentTurnDirection = nextDirection;
            DirectionChanged?.Invoke(nextDirection);
            if (_routeStage == RouteStage.ToTransitionPoint)
            {
                DirectionChangedToTransitionPoint?.Invoke(nextDirection);
            }
        }

        private static Vector3 GetLookDirection(IReadOnlyList<Vector3> corners, Vector3 origin, float lookAheadDistance)
        {
            float minDistanceSquared = lookAheadDistance * lookAheadDistance;

            for (int i = 1; i < corners.Count; i++)
            {
                Vector3 delta = corners[i] - origin;
                delta.y = 0f;

                if (delta.sqrMagnitude >= minDistanceSquared)
                {
                    return delta;
                }
            }

            Vector3 fallback = corners[corners.Count - 1] - origin;
            fallback.y = 0f;
            return fallback;
        }

        private void HandleLeftNavMesh()
        {
            RaisePathFailed();
        }

        private void RaisePathFailed()
        {
            RouteFailedEvent?.Invoke();
            PathFailed?.Invoke();
            StopRoute(clearDestination: false);
        }

        private void StopRoute(bool clearDestination)
        {
            _routeStage = RouteStage.None;

            if (_routeCoroutine != null)
            {
                StopCoroutine(_routeCoroutine);
                _routeCoroutine = null;
            }

            _pathShower?.HidePath();
            _activeTransitionPoint = null;
            CurrentTurnDirection = RouteTurnDirection.Straight;

            if (clearDestination)
            {
                _destinationTarget = null;
            }
        }

        private void UpdateCurrentFloorFromHeight()
        {
            if (!_detectFloorByHeight || _startPoint == null || _floorRanges == null || _floorRanges.Count == 0)
            {
                return;
            }

            float y = _startPoint.position.y;
            for (int i = 0; i < _floorRanges.Count; i++)
            {
                FloorHeightRange floor = _floorRanges[i];
                if (!floor.Contains(y))
                {
                    continue;
                }

                _currentFloorId = floor.FloorId;
                return;
            }
        }
    }
}
