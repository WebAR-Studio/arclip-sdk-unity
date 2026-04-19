using System;
using System.Collections.Generic;
using UnityEngine;

namespace NavigationDemo.UI.Data
{
    [CreateAssetMenu(
        fileName = "DestinationCatalogConfig",
        menuName = "Navigation/UI Destination Catalog")]
    public class DestinationCatalogConfig : ScriptableObject
    {
        [Serializable]
        public class CategoryDefinition
        {
            [SerializeField] private string _id;
            [SerializeField] private string _title = "Category";
            [SerializeField] private Sprite _icon;
            [SerializeField] private string _iconFallback = "\u2022";
            [SerializeField] private DestinationCategoryLocationsConfig _locationsConfig;

            public string Id => string.IsNullOrWhiteSpace(_id) ? _title : _id;
            public string Title => _title;
            public Sprite Icon => _icon;
            public string IconFallback => _iconFallback;
            public DestinationCategoryLocationsConfig LocationsConfig => _locationsConfig;
            public IReadOnlyList<DestinationCategoryLocationsConfig.LocationDefinition> Locations =>
                _locationsConfig == null ? null : _locationsConfig.Locations;
        }

        [SerializeField] private List<CategoryDefinition> _categories = new List<CategoryDefinition>();

        public IReadOnlyList<CategoryDefinition> Categories => _categories;

        public bool TryGetCategory(int index, out CategoryDefinition category)
        {
            category = null;
            if (_categories == null || index < 0 || index >= _categories.Count)
            {
                return false;
            }

            category = _categories[index];
            return category != null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_categories == null)
            {
                _categories = new List<CategoryDefinition>();
            }
        }
#endif
    }
}
