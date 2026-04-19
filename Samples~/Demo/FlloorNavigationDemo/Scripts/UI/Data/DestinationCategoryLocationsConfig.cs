using System;
using System.Collections.Generic;
using UnityEngine;

namespace NavigationDemo.UI.Data
{
    [CreateAssetMenu(
        fileName = "DestinationCategoryLocationsConfig",
        menuName = "Navigation/UI Category Locations Config")]
    public class DestinationCategoryLocationsConfig : ScriptableObject
    {
        [Serializable]
        public class LocationDefinition
        {
            [SerializeField] private string _id;
            [SerializeField] private string _title = "Location";
            [SerializeField] [TextArea(2, 4)] private string _description = "Location description";
            [SerializeField] private Sprite _icon;
            [SerializeField] private string _iconFallback = "\u2022";
            [SerializeField] private Vector3 _coordinates;
            [SerializeField] private int _floorId;
            [SerializeField] private string _navigationTargetKey;
            [SerializeField] private bool _showInfoIcon;
            [SerializeField] private List<string> _tags = new List<string>();

            public string Id => string.IsNullOrWhiteSpace(_id) ? _title : _id;
            public string Title => _title;
            public string Description => _description;
            public Sprite Icon => _icon;
            public string IconFallback => _iconFallback;
            public Vector3 Coordinates => _coordinates;
            public int FloorId => _floorId;
            public string NavigationTargetKey => _navigationTargetKey;
            public bool ShowInfoIcon => _showInfoIcon;
            public IReadOnlyList<string> Tags => _tags;
        }

        [SerializeField] private List<LocationDefinition> _locations = new List<LocationDefinition>();

        public IReadOnlyList<LocationDefinition> Locations => _locations;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_locations == null)
            {
                _locations = new List<LocationDefinition>();
            }
        }
#endif
    }
}
