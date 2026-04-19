using System.Collections.Generic;
using UnityEngine;

namespace NavigationDemo.Navigation
{
    [DisallowMultipleComponent]
    public class FloorTransitionPoint : MonoBehaviour
    {
        [SerializeField] private string _id;
        [SerializeField] private TransitionPointKind _kind = TransitionPointKind.Stairs;
        [SerializeField] private int _floorId;
        [SerializeField] private List<int> _reachableFloors = new List<int>();
        [SerializeField] [Min(0.1f)] private float _arrivalRadius = 2f;

        public string Id => _id;
        public TransitionPointKind Kind => _kind;
        public int FloorId => _floorId;
        public float ArrivalRadius => _arrivalRadius;
        public Vector3 Position => transform.position;

        public bool CanMoveTowards(int destinationFloorId)
        {
            if (destinationFloorId == _floorId)
            {
                return false;
            }

            bool goingUp = destinationFloorId > _floorId;
            if (_reachableFloors == null || _reachableFloors.Count == 0)
            {
                return true;
            }

            for (int i = 0; i < _reachableFloors.Count; i++)
            {
                int reachableFloor = _reachableFloors[i];
                if (goingUp && reachableFloor > _floorId)
                {
                    return true;
                }

                if (!goingUp && reachableFloor < _floorId)
                {
                    return true;
                }
            }

            return false;
        }

        public FloorTransitionInstruction GetInstruction(int destinationFloorId)
        {
            if (_kind == TransitionPointKind.Elevator)
            {
                return FloorTransitionInstruction.Lift;
            }

            return destinationFloorId > _floorId
                ? FloorTransitionInstruction.Up
                : FloorTransitionInstruction.Down;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(_id))
            {
                _id = gameObject.name;
            }

            if (_arrivalRadius < 0.1f)
            {
                _arrivalRadius = 0.1f;
            }
        }
#endif
    }
}
