using System;
using System.Collections.Generic;
using UnityEngine;

namespace NavigationDemo.Navigation
{
    public interface IPathShower
    {
        event Action LeftNavMesh;
        bool IsVisible { get; }
        void SetTarget(Vector3 targetPosition);
        void ShowPath(bool visible);
        void HidePath();
        float GetTimeToTarget();
        float GetDistanceToTarget();
        bool TryGetPathCorners(List<Vector3> cornersBuffer);
    }
}
