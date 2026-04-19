using System.Collections.Generic;
using NavigationDemo.UI.Base;
using NavigationDemo.UI.Views;
using UnityEngine;

namespace NavigationDemo.UI.Controllers
{
    public enum UIViewType
    {
        ArrivedLocation,
        CategoryStores,
        DestinationSelection,
        FillCirclesHint,
        NavigationDirection,
        RouteSummaryBar,
        StoreDetails,
        LoadingScreen,
        SettingsPrompt,
        FloorTransitionPrompt,
    }

    [DisallowMultipleComponent]
    public class UIViewsController : MonoBehaviour
    {
        [Header("Views")]
        [SerializeField] private ArrivedLocationView _arrivedLocationView;
        [SerializeField] private CategoryStoresView _categoryStoresView;
        [SerializeField] private DestinationSelectionView _destinationSelectionView;
        [SerializeField] private FillCirclesHintView _fillCirclesHintView;
        [SerializeField] private LoadingScreenView _loadingScreenView;
        [SerializeField] private NavigationDirectionView _navigationDirectionView;
        [SerializeField] private FloorTransitionPromptView _floorTransitionPromptView;
        [SerializeField] private RouteSummaryBarView _routeSummaryBarView;
        [SerializeField] private SettingsPromptView _settingsPromptView;
        [SerializeField] private StoreDetailsView _storeDetailsView;

        private readonly Dictionary<UIViewType, BaseView> _viewsByType = new Dictionary<UIViewType, BaseView>();
        private readonly List<BaseView> _allViews = new List<BaseView>(10);

        public ArrivedLocationView ArrivedLocationView => _arrivedLocationView;
        public CategoryStoresView CategoryStoresView => _categoryStoresView;
        public DestinationSelectionView DestinationSelectionView => _destinationSelectionView;
        public FillCirclesHintView FillCirclesHintView => _fillCirclesHintView;
        public FloorTransitionPromptView FloorTransitionPromptView => _floorTransitionPromptView;
        public LoadingScreenView LoadingScreenView => _loadingScreenView;
        public NavigationDirectionView NavigationDirectionView => _navigationDirectionView;
        public RouteSummaryBarView RouteSummaryBarView => _routeSummaryBarView;
        public SettingsPromptView SettingsPromptView => _settingsPromptView;
        public StoreDetailsView StoreDetailsView => _storeDetailsView;

        private void Awake()
        {
            CacheViews();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            CacheViews();
        }
#endif

        public bool TryGetView(UIViewType viewType, out BaseView view)
        {
            EnsureCache();
            return _viewsByType.TryGetValue(viewType, out view);
        }

        public bool IsVisible(UIViewType viewType)
        {
            return TryGetView(viewType, out BaseView view) && view.IsVisible;
        }

        public void Show(UIViewType viewType)
        {
            if (!TryGetView(viewType, out BaseView view))
            {
                return;
            }

            view.Show();
        }

        public void Hide(UIViewType viewType)
        {
            if (!TryGetView(viewType, out BaseView view))
            {
                return;
            }

            view.Hide();
        }

        public void SetVisible(UIViewType viewType, bool isVisible)
        {
            if (isVisible)
            {
                Show(viewType);
                return;
            }

            Hide(viewType);
        }

        public void ShowOnly(UIViewType viewType)
        {
            HideAll();
            Show(viewType);
        }

        public void ShowOnly(params UIViewType[] viewTypes)
        {
            HideAll();

            if (viewTypes == null)
            {
                return;
            }

            for (int index = 0; index < viewTypes.Length; index++)
            {
                Show(viewTypes[index]);
            }
        }

        public void ShowAllAssigned()
        {
            EnsureCache();
            for (int index = 0; index < _allViews.Count; index++)
            {
                _allViews[index].Show();
            }
        }

        public void HideAll()
        {
            EnsureCache();
            for (int index = 0; index < _allViews.Count; index++)
            {
                _allViews[index].Hide();
            }
        }

        public void ShowArrivedLocationView() => Show(UIViewType.ArrivedLocation);
        public void HideArrivedLocationView() => Hide(UIViewType.ArrivedLocation);

        public void ShowCategoryStoresView() => Show(UIViewType.CategoryStores);
        public void HideCategoryStoresView() => Hide(UIViewType.CategoryStores);

        public void ShowDestinationSelectionView() => Show(UIViewType.DestinationSelection);
        public void HideDestinationSelectionView() => Hide(UIViewType.DestinationSelection);

        public void ShowFillCirclesHintView() => Show(UIViewType.FillCirclesHint);
        public void HideFillCirclesHintView() => Hide(UIViewType.FillCirclesHint);

        public void ShowFloorTransitionPromptView() => Show(UIViewType.FloorTransitionPrompt);
        public void HideFloorTransitionPromptView() => Hide(UIViewType.FloorTransitionPrompt);

        public void ShowLoadingScreenView() => Show(UIViewType.LoadingScreen);
        public void HideLoadingScreenView() => Hide(UIViewType.LoadingScreen);

        public void ShowNavigationDirectionView() => Show(UIViewType.NavigationDirection);
        public void HideNavigationDirectionView() => Hide(UIViewType.NavigationDirection);

        public void ShowRouteSummaryBarView() => Show(UIViewType.RouteSummaryBar);
        public void HideRouteSummaryBarView() => Hide(UIViewType.RouteSummaryBar);

        public void ShowSettingsPromptView() => Show(UIViewType.SettingsPrompt);
        public void HideSettingsPromptView() => Hide(UIViewType.SettingsPrompt);

        public void ShowStoreDetailsView() => Show(UIViewType.StoreDetails);
        public void HideStoreDetailsView() => Hide(UIViewType.StoreDetails);

        public void ShowOnlyArrivedLocationView() => ShowOnly(UIViewType.ArrivedLocation);
        public void ShowOnlyCategoryStoresView() => ShowOnly(UIViewType.CategoryStores);
        public void ShowOnlyDestinationSelectionView() => ShowOnly(UIViewType.DestinationSelection);
        public void ShowOnlyFillCirclesHintView() => ShowOnly(UIViewType.FillCirclesHint);
        public void ShowOnlyFloorTransitionPromptView() => ShowOnly(UIViewType.FloorTransitionPrompt);
        public void ShowOnlyLoadingScreenView() => ShowOnly(UIViewType.LoadingScreen);
        public void ShowOnlyNavigationDirectionView() => ShowOnly(UIViewType.NavigationDirection);
        public void ShowOnlyRouteSummaryBarView() => ShowOnly(UIViewType.RouteSummaryBar);
        public void ShowOnlySettingsPromptView() => ShowOnly(UIViewType.SettingsPrompt);
        public void ShowOnlyStoreDetailsView() => ShowOnly(UIViewType.StoreDetails);

        private void EnsureCache()
        {
            if (_viewsByType.Count > 0)
            {
                return;
            }

            CacheViews();
        }

        private void CacheViews()
        {
            _viewsByType.Clear();
            _allViews.Clear();

            CacheView(UIViewType.ArrivedLocation, _arrivedLocationView);
            CacheView(UIViewType.CategoryStores, _categoryStoresView);
            CacheView(UIViewType.DestinationSelection, _destinationSelectionView);
            CacheView(UIViewType.FillCirclesHint, _fillCirclesHintView);
            CacheView(UIViewType.FloorTransitionPrompt, _floorTransitionPromptView);
            CacheView(UIViewType.LoadingScreen, _loadingScreenView);
            CacheView(UIViewType.NavigationDirection, _navigationDirectionView);
            CacheView(UIViewType.RouteSummaryBar, _routeSummaryBarView);
            CacheView(UIViewType.SettingsPrompt, _settingsPromptView);
            CacheView(UIViewType.StoreDetails, _storeDetailsView);
        }

        private void CacheView(UIViewType viewType, BaseView view)
        {
            if (view == null)
            {
                return;
            }

            _viewsByType[viewType] = view;
            if (!_allViews.Contains(view))
            {
                _allViews.Add(view);
            }
        }
    }
}
