using System;
using System.Collections.Generic;
using NavigationDemo.UI.Data;
using NavigationDemo.UI.Views;
using UnityEngine;

namespace NavigationDemo.UI.Controllers
{
    [DisallowMultipleComponent]
    public class DestinationSelectionFlowController : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private DestinationCatalogConfig _catalogConfig;

        [Header("Dependencies")]
        [SerializeField] private StateController _stateController;
        [SerializeField] private UIViewsController _uiViewsController;

        [Header("State Flow")]
        [SerializeField] private NavigationState _stateWhenCategoriesVisible = NavigationState.CategorySelection;
        [SerializeField] private NavigationState _stateWhenLocationsVisible = NavigationState.StorySelection;
        [SerializeField] private NavigationState _stateWhenLocationSelected = NavigationState.NavigationInitialization;

        [Header("Fallback Icons")]
        [SerializeField] private string _defaultCategoryIconFallback = "\u2022";
        [SerializeField] private string _defaultLocationIconFallback = "\u2022";

        [Header("Behaviour")]
        [SerializeField] private bool _populateOnEnable = true;

        private DestinationSelectionView _destinationSelectionView;
        private CategoryStoresView _categoryStoresView;
        private readonly List<DestinationCatalogConfig.CategoryDefinition> _presentedCategories =
            new List<DestinationCatalogConfig.CategoryDefinition>();
        private readonly List<DestinationCategoryLocationsConfig.LocationDefinition> _presentedLocations =
            new List<DestinationCategoryLocationsConfig.LocationDefinition>();
        private DestinationCatalogConfig.CategoryDefinition _selectedCategory;
        private DestinationCategoryLocationsConfig.LocationDefinition _selectedLocation;
        private bool _isSubscribed;

        public event Action<DestinationCatalogConfig.CategoryDefinition> CategorySelected;
        public event Action<DestinationCategoryLocationsConfig.LocationDefinition> LocationSelected;

        public DestinationCatalogConfig.CategoryDefinition SelectedCategory => _selectedCategory;
        public DestinationCategoryLocationsConfig.LocationDefinition SelectedLocation => _selectedLocation;
        public string SelectedNavigationTargetKey => _selectedLocation == null ? string.Empty : _selectedLocation.NavigationTargetKey;
        public Vector3 SelectedLocationCoordinates => _selectedLocation == null ? Vector3.zero : _selectedLocation.Coordinates;

        private void Awake()
        {
            ResolveReferences();
            PopulateCategoriesFromConfig();
        }

        private void OnEnable()
        {
            ResolveReferences();
            SubscribeEvents();

            if (_populateOnEnable)
            {
                PopulateCategoriesFromConfig();
            }
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        public void SetCatalogConfig(DestinationCatalogConfig config)
        {
            _catalogConfig = config;
            PopulateCategoriesFromConfig();
        }

        public void ClearSelectedLocation()
        {
            _selectedLocation = null;
        }

        public bool TryGetSelectedLocation(out DestinationCategoryLocationsConfig.LocationDefinition location)
        {
            location = _selectedLocation;
            return location != null;
        }

        private void HandleCategoryPressed(int index)
        {
            if (!TryGetPresentedCategory(index, out DestinationCatalogConfig.CategoryDefinition category))
            {
                return;
            }

            _selectedCategory = category;
            _selectedLocation = null;

            PopulateLocationsForCategory(category);
            SetState(_stateWhenLocationsVisible);
            CategorySelected?.Invoke(category);
        }

        private void HandleLocationPressed(int index)
        {
            if (!TryGetLocation(index, out DestinationCategoryLocationsConfig.LocationDefinition location))
            {
                return;
            }

            _selectedLocation = location;
            SetState(_stateWhenLocationSelected);
            LocationSelected?.Invoke(location);
        }

        private void HandleBackRequested()
        {
            _selectedLocation = null;
            SetState(_stateWhenCategoriesVisible);
        }

        private void HandleStateChanged(NavigationState state)
        {
            if (state == _stateWhenCategoriesVisible)
            {
                PopulateCategoriesFromConfig();
                return;
            }

            if (state == _stateWhenLocationsVisible && _selectedCategory != null)
            {
                PopulateLocationsForCategory(_selectedCategory);
            }
        }

        private void PopulateCategoriesFromConfig()
        {
            if (_destinationSelectionView == null)
            {
                return;
            }

            if (_catalogConfig == null || _catalogConfig.Categories == null)
            {
                _presentedCategories.Clear();
                _presentedLocations.Clear();
                _destinationSelectionView.SetCategories((IReadOnlyList<DestinationSelectionView.CategoryViewData>)null);
                return;
            }

            List<DestinationSelectionView.CategoryViewData> valueCategories = new List<DestinationSelectionView.CategoryViewData>();
            _presentedCategories.Clear();
            IReadOnlyList<DestinationCatalogConfig.CategoryDefinition> values = _catalogConfig.Categories;
            for (int index = 0; index < values.Count; index++)
            {
                DestinationCatalogConfig.CategoryDefinition category = values[index];
                if (category == null)
                {
                    continue;
                }

                _presentedCategories.Add(category);
                valueCategories.Add(new DestinationSelectionView.CategoryViewData(
                    category.Title,
                    category.Icon,
                    ResolveIconFallback(category.IconFallback, _defaultCategoryIconFallback)));
            }

            _destinationSelectionView.SetCategories(valueCategories);
        }

        private void PopulateLocationsForCategory(DestinationCatalogConfig.CategoryDefinition category)
        {
            if (_categoryStoresView == null || category == null)
            {
                return;
            }

            _presentedLocations.Clear();
            _categoryStoresView.SetBackButtonValue("\u0412\u0441\u0435 \u043A\u0430\u0442\u0435\u0433\u043E\u0440\u0438\u0438");
            _categoryStoresView.SetHeaderValue(category.Title);
            _categoryStoresView.SetHeaderIcon(category.Icon, ResolveIconFallback(category.IconFallback, _defaultCategoryIconFallback));

            List<CategoryStoresView.StoreCardViewData> valueStores = new List<CategoryStoresView.StoreCardViewData>();
            IReadOnlyList<DestinationCategoryLocationsConfig.LocationDefinition> locations = category.Locations;
            if (locations != null)
            {
                for (int index = 0; index < locations.Count; index++)
                {
                    DestinationCategoryLocationsConfig.LocationDefinition location = locations[index];
                    if (location == null)
                    {
                        continue;
                    }

                    _presentedLocations.Add(location);
                    valueStores.Add(new CategoryStoresView.StoreCardViewData(
                        location.Title,
                        location.Description,
                        location.Icon,
                        ResolveLocationIconFallback(location, index),
                        location.ShowInfoIcon));
                }
            }

            _categoryStoresView.SetStores(valueStores);
        }

        private bool TryGetLocation(int index, out DestinationCategoryLocationsConfig.LocationDefinition location)
        {
            location = null;
            if (index < 0 || index >= _presentedLocations.Count)
            {
                return false;
            }

            location = _presentedLocations[index];
            return location != null;
        }

        private bool TryGetPresentedCategory(int index, out DestinationCatalogConfig.CategoryDefinition category)
        {
            category = null;
            if (index < 0 || index >= _presentedCategories.Count)
            {
                return false;
            }

            category = _presentedCategories[index];
            return category != null;
        }

        private static string ResolveIconFallback(string value, string defaultValue)
        {
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
        }

        private string ResolveLocationIconFallback(DestinationCategoryLocationsConfig.LocationDefinition location, int index)
        {
            if (location == null)
            {
                return _defaultLocationIconFallback;
            }

            if (!string.IsNullOrWhiteSpace(location.IconFallback))
            {
                return location.IconFallback;
            }

            string title = location.Title;
            if (string.IsNullOrWhiteSpace(title))
            {
                return _defaultLocationIconFallback;
            }

            if (title.Length == 1)
            {
                return title.ToUpperInvariant();
            }

            if (title.Length <= 3)
            {
                return title.Substring(0, title.Length).ToUpperInvariant();
            }

            int length = index % 2 == 0 ? 2 : 3;
            return title.Substring(0, Mathf.Min(length, title.Length)).ToUpperInvariant();
        }

        private void SetState(NavigationState state)
        {
            _stateController?.SetState(state);
        }

        private void ResolveReferences()
        {
            if (_stateController == null)
            {
                _stateController = GetComponent<StateController>();
            }

            if (_uiViewsController == null && _stateController != null)
            {
                _uiViewsController = _stateController.UIViewsController;
            }

            if (_uiViewsController == null)
            {
                _uiViewsController = GetComponent<UIViewsController>();
            }

            if (_uiViewsController != null)
            {
                _destinationSelectionView = _uiViewsController.DestinationSelectionView;
                _categoryStoresView = _uiViewsController.CategoryStoresView;
            }
        }

        private void SubscribeEvents()
        {
            if (_isSubscribed)
            {
                return;
            }

            if (_destinationSelectionView != null)
            {
                _destinationSelectionView.CategorySelected += HandleCategoryPressed;
            }

            if (_categoryStoresView != null)
            {
                _categoryStoresView.StoreSelected += HandleLocationPressed;
                _categoryStoresView.BackRequested += HandleBackRequested;
            }

            if (_stateController != null)
            {
                _stateController.StateChanged += HandleStateChanged;
            }

            _isSubscribed = true;
        }

        private void UnsubscribeEvents()
        {
            if (!_isSubscribed)
            {
                return;
            }

            if (_destinationSelectionView != null)
            {
                _destinationSelectionView.CategorySelected -= HandleCategoryPressed;
            }

            if (_categoryStoresView != null)
            {
                _categoryStoresView.StoreSelected -= HandleLocationPressed;
                _categoryStoresView.BackRequested -= HandleBackRequested;
            }

            if (_stateController != null)
            {
                _stateController.StateChanged -= HandleStateChanged;
            }

            _isSubscribed = false;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ResolveReferences();
        }
#endif
    }
}
