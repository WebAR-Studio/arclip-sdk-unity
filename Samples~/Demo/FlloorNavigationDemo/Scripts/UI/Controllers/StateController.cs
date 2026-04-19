using System;
using System.Collections.Generic;
using NavigationDemo.UI.Views;
using UnityEngine;

namespace NavigationDemo.UI.Controllers
{
    public enum NavigationState
    {
        Loading = 0,
        CategorySelection = 1,
        StorySelection = 2,
        StoreSelection = StorySelection,
        NavigationInitialization = 3,
        Navigation = 4,
        NavigationCompleted = 5,
        LocationInfo = 6,
        FloorTransitionReached = 7,
        NoPermission = 8,
    }

    [Serializable]
    public class StateViewConfiguration
    {
        [SerializeField] private NavigationState _state;
        [SerializeField] private List<UIViewType> _activeViews = new List<UIViewType>();

        public NavigationState State => _state;
        public List<UIViewType> ActiveViews => _activeViews;

        public StateViewConfiguration()
        {
        }

        public StateViewConfiguration(NavigationState state)
        {
            _state = state;
        }
    }

    [DisallowMultipleComponent]
    public class StateController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private UIViewsController _uiViewsController;

        [Header("State")]
        [SerializeField] private NavigationState _currentState = NavigationState.Loading;
        [SerializeField] private bool _applyStateOnAwake = true;

        [Header("State View Configuration")]
        [SerializeField] private List<StateViewConfiguration> _stateViewConfigurations = new List<StateViewConfiguration>();

        private readonly Dictionary<NavigationState, List<UIViewType>> _viewsByState =
            new Dictionary<NavigationState, List<UIViewType>>();

        public event Action<NavigationState> StateChanged;

        public NavigationState CurrentState => _currentState;

        public UIViewsController UIViewsController => _uiViewsController;

        private void Reset()
        {
            EnsureAllStatesAreConfigured();
            RebuildStateCache();
        }

        private void Awake()
        {
            RebuildStateCache();

            if (_applyStateOnAwake)
            {
                ApplyCurrentStateViews();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            EnsureAllStatesAreConfigured();
            RebuildStateCache();
        }
#endif

        public void SetState(NavigationState state)
        {
            _currentState = state;
            ApplyCurrentStateViews();
            StateChanged?.Invoke(_currentState);
        }

        public void ApplyCurrentStateViews()
        {
            if (_uiViewsController == null)
            {
                return;
            }

            EnsureCache();
            _uiViewsController.HideAll();

            if (!_viewsByState.TryGetValue(_currentState, out List<UIViewType> activeViews))
            {
                return;
            }

            for (int index = 0; index < activeViews.Count; index++)
            {
                _uiViewsController.Show(activeViews[index]);
            }
        }

        public void SetLoadingState() => SetState(NavigationState.Loading);
        public void SetCategorySelectionState() => SetState(NavigationState.CategorySelection);
        public void SetStorySelectionState() => SetState(NavigationState.StorySelection);
        public void SetStoreSelectionState() => SetStorySelectionState();
        public void SetNavigationInitializationState() => SetState(NavigationState.NavigationInitialization);
        public void SetNavigationState() => SetState(NavigationState.Navigation);
        public void SetFloorTransitionReachedState() => SetState(NavigationState.FloorTransitionReached);
        public void SetNavigationCompletedState() => SetState(NavigationState.NavigationCompleted);
        public void SetLocationInfoState() => SetState(NavigationState.LocationInfo);
        public void SetNoPermissionState() => SetState(NavigationState.NoPermission);

        public void SetNavgationCompletedState() => SetNavigationCompletedState();

        private void EnsureCache()
        {
            if (_viewsByState.Count > 0)
            {
                return;
            }

            RebuildStateCache();
        }

        private void RebuildStateCache()
        {
            _viewsByState.Clear();
            for (int configIndex = 0; configIndex < _stateViewConfigurations.Count; configIndex++)
            {
                StateViewConfiguration configuration = _stateViewConfigurations[configIndex];
                if (configuration == null)
                {
                    continue;
                }

                if (!_viewsByState.TryGetValue(configuration.State, out List<UIViewType> mappedViews))
                {
                    mappedViews = new List<UIViewType>();
                    _viewsByState.Add(configuration.State, mappedViews);
                }

                List<UIViewType> configuredViews = configuration.ActiveViews;
                if (configuredViews == null)
                {
                    continue;
                }

                for (int viewIndex = 0; viewIndex < configuredViews.Count; viewIndex++)
                {
                    UIViewType viewType = configuredViews[viewIndex];
                    if (mappedViews.Contains(viewType))
                    {
                        continue;
                    }

                    mappedViews.Add(viewType);
                }
            }
        }

        private void EnsureAllStatesAreConfigured()
        {
            if (_stateViewConfigurations == null)
            {
                _stateViewConfigurations = new List<StateViewConfiguration>();
            }

            NavigationState[] states = (NavigationState[])Enum.GetValues(typeof(NavigationState));
            for (int stateIndex = 0; stateIndex < states.Length; stateIndex++)
            {
                NavigationState state = states[stateIndex];
                if (HasConfigurationForState(state))
                {
                    continue;
                }

                _stateViewConfigurations.Add(new StateViewConfiguration(state));
            }
        }

        private bool HasConfigurationForState(NavigationState state)
        {
            for (int configIndex = 0; configIndex < _stateViewConfigurations.Count; configIndex++)
            {
                StateViewConfiguration configuration = _stateViewConfigurations[configIndex];
                if (configuration == null)
                {
                    continue;
                }

                if (configuration.State == state)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
