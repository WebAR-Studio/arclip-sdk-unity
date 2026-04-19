using System;
using UnityEngine;

namespace NavigationDemo.Navigation
{
    public enum FloorTransitionInstruction
    {
        Up,
        Down,
        Lift
    }

    public enum TransitionPointKind
    {
        Stairs,
        Elevator
    }

    public enum RouteTurnDirection
    {
        Straight,
        Right,
        Left,
        Back
    }

    [Serializable]
    public struct FloorHeightRange
    {
        [SerializeField] private int _floorId;
        [SerializeField] private float _minY;
        [SerializeField] private float _maxY;

        public int FloorId => _floorId;

        public bool Contains(float y)
        {
            float min = Mathf.Min(_minY, _maxY);
            float max = Mathf.Max(_minY, _maxY);
            return y >= min && y <= max;
        }
    }
}
