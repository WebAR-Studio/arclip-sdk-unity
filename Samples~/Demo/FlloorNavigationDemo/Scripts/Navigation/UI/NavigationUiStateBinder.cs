using System.Collections.Generic;
using NavigationDemo.UI.Data;
using NavigationDemo.UI.Controllers;
using NavigationDemo.UI.Views;
using UnityEngine;

namespace NavigationDemo.Navigation.UI
{
    [DisallowMultipleComponent]
    public class NavigationUiStateBinder : MonoBehaviour
    {
        [Header("Navigation References")]
        [SerializeField] private TargetManager _targetManager;
        [SerializeField] private NavigationUiPresentationConfig _presentationConfig;
        [SerializeField] private DestinationSelectionFlowController _destinationSelectionFlowController;
        [SerializeField] private VPSManager _vpsManager;

        [Header("State References")]
        [SerializeField] private StateController _stateController;
        [SerializeField] private UIViewsController _uiViewsController;

        [Header("View Overrides (Optional)")]
        [SerializeField] private NavigationDirectionView _navigationDirectionView;
        [SerializeField] private FloorTransitionPromptView _floorTransitionPromptView;
        [SerializeField] private RouteSummaryBarView _routeSummaryBarView;
        [SerializeField] private ArrivedLocationView _arrivedLocationView;
        [SerializeField] private StoreDetailsView _storeDetailsView;

        [Header("State Flow")]
        [SerializeField] private NavigationState _stateWhenRouting = NavigationState.Navigation;
        [SerializeField] private NavigationState _stateWhenTransitionReached = NavigationState.FloorTransitionReached;
        [SerializeField] private NavigationState _stateWhenRouteCompleted = NavigationState.NavigationCompleted;
        [SerializeField] private NavigationState _stateWhenRouteFailed = NavigationState.NavigationInitialization;
        [SerializeField] private NavigationState _stateWhenRouteStopped = NavigationState.NavigationInitialization;
        [SerializeField] private NavigationState _stateWhenRouteCanceledBySummary = NavigationState.StorySelection;
        [SerializeField] private NavigationState _stateWhenRouteCompletedInfoOpened = NavigationState.LocationInfo;
        [SerializeField] private NavigationState _stateWhenRouteCompletedFinish = NavigationState.CategorySelection;
        [SerializeField] private NavigationState _stateWhenLocationInfoClosed = NavigationState.CategorySelection;

        [Header("Prompt Actions")]
        [SerializeField] private bool _doneButtonContinuesRoute = true;
        [SerializeField] private bool _continueUsingInstructionWhenFloorAutoDetectIsOff = true;
        [SerializeField] private bool _finishButtonStopsRoute = true;
        [SerializeField] private bool _finishButtonSetsCompletedState;

        [Header("Location Info Fallback")]
        [SerializeField] private string _fallbackLocationTitle = "\u041B\u043E\u043A\u0430\u0446\u0438\u044F";
        [SerializeField] private string _fallbackLocationFloor = "\u042D\u0442\u0430\u0436 \u043D\u0435 \u0443\u043A\u0430\u0437\u0430\u043D";
        [SerializeField] private string _fallbackLocationCategories = "\u0418\u043D\u0444\u043E \u043D\u0435 \u0443\u043A\u0430\u0437\u0430\u043D\u0430";
        [SerializeField] private string _fallbackLocationSchedule = "\u0413\u0440\u0430\u0444\u0438\u043A \u043D\u0435 \u0443\u043A\u0430\u0437\u0430\u043D";
        [SerializeField] [TextArea(2, 6)] private string _fallbackLocationDescription =
            "\u0418\u043D\u0444\u043E\u0440\u043C\u0430\u0446\u0438\u044F \u043E \u043B\u043E\u043A\u0430\u0446\u0438\u0438 \u043D\u0435 \u043D\u0430\u0439\u0434\u0435\u043D\u0430.";

        private RouteTurnDirection _currentDirection = RouteTurnDirection.Straight;
        private float _currentLegDistance;
        private float _currentLegTimeSeconds;
        private PathTarget _currentDestination;
        private bool _isSubscribed;

        private void Awake()
        {
            ResolveReferences();
            UpdateSummaryView();
            UpdateDirectionView();
        }

        private void OnEnable()
        {
            ResolveReferences();
            SubscribeEvents();

            if (_stateController != null)
            {
                HandleStateChanged(_stateController.CurrentState);
            }
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        public void StartRouteByKey(string targetKey, bool showArrows = true)
        {
            if (_targetManager == null)
            {
                return;
            }

            ResetRouteVisualState();
            _targetManager.ShowPath(targetKey, showArrows);
            _currentDestination = _targetManager.DestinationTarget;

            if (!_targetManager.IsRouting)
            {
                UpdateSummaryView();
                UpdateDirectionView();
                return;
            }

            SetState(_stateWhenRouting);
            UpdateSummaryView();
            UpdateDirectionView();
        }

        public void StartRoute(PathTarget target, bool showArrows = true)
        {
            if (_targetManager == null || target == null)
            {
                return;
            }

            ResetRouteVisualState();
            _targetManager.ShowPath(target, showArrows);
            _currentDestination = target;

            if (!_targetManager.IsRouting)
            {
                UpdateSummaryView();
                UpdateDirectionView();
                return;
            }

            SetState(_stateWhenRouting);
            UpdateSummaryView();
            UpdateDirectionView();
        }

        public void StopRoute()
        {
            _targetManager?.HidePath();
            SetState(_stateWhenRouteStopped);
        }

        private void HandleRouteMetricsChanged(float distance, float timeSeconds, PathTarget destination)
        {
            _currentLegDistance = distance;
            _currentLegTimeSeconds = timeSeconds;
            if (destination != null)
            {
                _currentDestination = destination;
            }

            UpdateSummaryView();
            UpdateDirectionView();
        }

        private void HandleDirectionChanged(RouteTurnDirection direction)
        {
            _currentDirection = direction;
            UpdateDirectionView();
        }

        private void HandleTransitionReached(FloorTransitionPoint transitionPoint, FloorTransitionInstruction instruction)
        {
            UpdateTransitionPromptView(instruction);
            SetState(_stateWhenTransitionReached);
            _floorTransitionPromptView?.RestartDoneButtonLock();
        }

        private void HandleRouteCompleted(PathTarget destination)
        {
            if (destination != null)
            {
                _currentDestination = destination;
            }

            UpdateSummaryView();
            UpdateCompletedLocationViews();
            SetState(_stateWhenRouteCompleted);
        }

        private void HandleRouteFailed()
        {
            SetState(_stateWhenRouteFailed);
        }

        private void HandleDoneClicked(FloorTransitionPromptView view)
        {
            if (_doneButtonContinuesRoute && _targetManager != null)
            {
                if (_continueUsingInstructionWhenFloorAutoDetectIsOff && !_targetManager.IsFloorAutoDetectionEnabled)
                {
                    _targetManager.ContinueAfterTransitionUsingInstruction();
                }
                else
                {
                    _targetManager.ContinueAfterTransition();
                }
            }

            SetState(_stateWhenRouteStopped);
        }

        private void HandleFinishClicked(FloorTransitionPromptView view)
        {
            if (!_finishButtonStopsRoute)
            {
                return;
            }

            _targetManager?.HidePath();
            SetState(_finishButtonSetsCompletedState ? _stateWhenRouteCompleted : _stateWhenRouteStopped);
        }

        private void HandleArrivedLeftButtonClicked(ArrivedLocationView view)
        {
            UpdateLocationInfoView();
            SetState(_stateWhenRouteCompletedInfoOpened);
        }

        private void HandleArrivedRightButtonClicked(ArrivedLocationView view)
        {
            CompleteRouteAndReturn(_stateWhenRouteCompletedFinish);
        }

        private void HandleStoreDetailsCloseClicked(StoreDetailsView view)
        {
            CompleteRouteAndReturn(_stateWhenLocationInfoClosed);
        }

        private void HandleRouteSummaryLeftButtonClicked(RouteSummaryBarView view)
        {
            _targetManager?.HidePath();
            _vpsManager?.ForceStopVps();
            _destinationSelectionFlowController?.ClearSelectedLocation();
            SetState(_stateWhenRouteCanceledBySummary);
        }

        private void CompleteRouteAndReturn(NavigationState returnState)
        {
            _targetManager?.HidePath();
            _destinationSelectionFlowController?.ClearSelectedLocation();
            SetState(returnState);
        }

        private void HandleStateChanged(NavigationState state)
        {
            if (state == _stateWhenRouteCompleted)
            {
                UpdateArrivedLocationView();
                _arrivedLocationView?.Show();
                return;
            }

            if (state == _stateWhenRouteCompletedInfoOpened)
            {
                UpdateLocationInfoView();
                _storeDetailsView?.Show();
            }
        }

        private void UpdateSummaryView()
        {
            if (_routeSummaryBarView == null)
            {
                return;
            }

            string targetName = _currentDestination == null ? string.Empty : _currentDestination.Title;
            _routeSummaryBarView.SetSummary(
                FormatTime(_currentLegTimeSeconds),
                FormatDistance(_currentLegDistance),
                targetName);
        }

        private void UpdateDirectionView()
        {
            if (_navigationDirectionView == null)
            {
                return;
            }

            Sprite iconSprite;
            string iconFallback;
            string title;

            if (_presentationConfig != null)
            {
                _presentationConfig.GetTurnPresentation(_currentDirection, out iconSprite, out iconFallback, out title);
            }
            else
            {
                iconSprite = null;
                iconFallback = GetDefaultTurnIcon(_currentDirection);
                title = GetDefaultTurnTitle(_currentDirection);
            }

            _navigationDirectionView.SetDirection(
                iconSprite,
                iconFallback,
                title,
                FormatDistance(_currentLegDistance));
        }

        private void UpdateTransitionPromptView(FloorTransitionInstruction instruction)
        {
            if (_floorTransitionPromptView == null)
            {
                return;
            }

            int destinationFloor = _currentDestination == null ? _targetManager.CurrentFloorId : _currentDestination.FloorId;

            Sprite iconSprite;
            string iconFallback;
            string subtitle;
            string title;

            if (_presentationConfig != null)
            {
                _presentationConfig.GetTransitionPresentation(
                    instruction,
                    destinationFloor,
                    out iconSprite,
                    out iconFallback,
                    out subtitle,
                    out title);
            }
            else
            {
                iconSprite = null;
                iconFallback = GetDefaultTransitionIcon(instruction);
                subtitle = "Чтобы продолжить,";
                title = GetDefaultTransitionTitle(instruction, destinationFloor);
            }

            _floorTransitionPromptView.SetContent(iconSprite, iconFallback, subtitle, title);
        }

        private string FormatDistance(float distanceMeters)
        {
            if (_presentationConfig != null)
            {
                return _presentationConfig.FormatDistance(distanceMeters);
            }

            if (distanceMeters < 1f)
            {
                return "< 1 м";
            }

            return $"{distanceMeters:F0} м";
        }

        private string FormatTime(float timeSeconds)
        {
            if (_presentationConfig != null)
            {
                return _presentationConfig.FormatTime(timeSeconds);
            }

            float minutes = timeSeconds / 60f;
            if (minutes < 1f)
            {
                return "< 1 мин";
            }

            return $"{minutes:F0} мин";
        }

        private void SetState(NavigationState state)
        {
            if (_stateController == null || _stateController.CurrentState == state)
            {
                return;
            }

            _stateController.SetState(state);
        }

        private void UpdateCompletedLocationViews()
        {
            UpdateArrivedLocationView();
            UpdateLocationInfoView();
        }

        private void UpdateArrivedLocationView()
        {
            if (_arrivedLocationView == null)
            {
                return;
            }

            _arrivedLocationView.SetSubtitleValue(ResolveLocationTitle());
        }

        private void UpdateLocationInfoView()
        {
            if (_storeDetailsView == null)
            {
                return;
            }

            _storeDetailsView.SetTitleValue(ResolveLocationTitle());
            _storeDetailsView.SetSubtitleValue(ResolveLocationSubtitle());
            _storeDetailsView.SetCategoriesValue(ResolveLocationCategories());
            _storeDetailsView.SetScheduleValue(_fallbackLocationSchedule);
            _storeDetailsView.SetDescriptionValue(ResolveLocationDescription());
        }

        private string ResolveLocationTitle()
        {
            if (TryGetSelectedLocation(out DestinationCategoryLocationsConfig.LocationDefinition location) &&
                !string.IsNullOrWhiteSpace(location.Title))
            {
                return location.Title;
            }

            if (_currentDestination != null && !string.IsNullOrWhiteSpace(_currentDestination.Title))
            {
                return _currentDestination.Title;
            }

            return _fallbackLocationTitle;
        }

        private string ResolveLocationSubtitle()
        {
            if (TryGetSelectedLocation(out DestinationCategoryLocationsConfig.LocationDefinition location))
            {
                return FormatFloorLabel(location.FloorId);
            }

            if (_currentDestination != null)
            {
                return FormatFloorLabel(_currentDestination.FloorId);
            }

            return _fallbackLocationFloor;
        }

        private string ResolveLocationCategories()
        {
            if (TryGetSelectedLocation(out DestinationCategoryLocationsConfig.LocationDefinition location))
            {
                string tags = JoinNonEmpty(location.Tags);
                if (!string.IsNullOrWhiteSpace(tags))
                {
                    return tags;
                }
            }

            if (_destinationSelectionFlowController != null)
            {
                DestinationCatalogConfig.CategoryDefinition category = _destinationSelectionFlowController.SelectedCategory;
                if (category != null && !string.IsNullOrWhiteSpace(category.Title))
                {
                    return category.Title;
                }
            }

            return _fallbackLocationCategories;
        }

        private string ResolveLocationDescription()
        {
            if (TryGetSelectedLocation(out DestinationCategoryLocationsConfig.LocationDefinition location) &&
                !string.IsNullOrWhiteSpace(location.Description))
            {
                return location.Description;
            }

            return _fallbackLocationDescription;
        }

        private bool TryGetSelectedLocation(out DestinationCategoryLocationsConfig.LocationDefinition location)
        {
            location = null;
            return _destinationSelectionFlowController != null &&
                   _destinationSelectionFlowController.TryGetSelectedLocation(out location) &&
                   location != null;
        }

        private static string JoinNonEmpty(IReadOnlyList<string> values)
        {
            if (values == null || values.Count == 0)
            {
                return string.Empty;
            }

            List<string> filteredValues = new List<string>();
            for (int index = 0; index < values.Count; index++)
            {
                string value = values[index];
                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                filteredValues.Add(value.Trim());
            }

            if (filteredValues.Count == 0)
            {
                return string.Empty;
            }

            return string.Join(", ", filteredValues);
        }

        private static string FormatFloorLabel(int floorId)
        {
            return $"\u042D\u0442\u0430\u0436 {floorId}";
        }

        private void ResetRouteVisualState()
        {
            _currentDirection = RouteTurnDirection.Straight;
            _currentLegDistance = 0f;
            _currentLegTimeSeconds = 0f;
        }

        private void ResolveReferences()
        {
            if (_targetManager == null)
            {
                _targetManager = GetComponent<TargetManager>();
            }

            if (_destinationSelectionFlowController == null)
            {
                _destinationSelectionFlowController = GetComponent<DestinationSelectionFlowController>();
            }

            if (_vpsManager == null)
            {
                _vpsManager = GetComponent<VPSManager>();
            }

            if (_stateController == null)
            {
                _stateController = GetComponent<StateController>();
            }

            if (_uiViewsController == null && _stateController != null)
            {
                _uiViewsController = _stateController.UIViewsController;
            }

            if (_uiViewsController != null)
            {
                if (_navigationDirectionView == null)
                {
                    _navigationDirectionView = _uiViewsController.NavigationDirectionView;
                }

                if (_floorTransitionPromptView == null)
                {
                    _floorTransitionPromptView = _uiViewsController.FloorTransitionPromptView;
                }

                if (_routeSummaryBarView == null)
                {
                    _routeSummaryBarView = _uiViewsController.RouteSummaryBarView;
                }

                if (_arrivedLocationView == null)
                {
                    _arrivedLocationView = _uiViewsController.ArrivedLocationView;
                }

                if (_storeDetailsView == null)
                {
                    _storeDetailsView = _uiViewsController.StoreDetailsView;
                }
            }
        }

        private void SubscribeEvents()
        {
            if (_isSubscribed)
            {
                return;
            }

            if (_targetManager != null)
            {
                _targetManager.RouteMetricsChanged += HandleRouteMetricsChanged;
                _targetManager.DirectionChanged += HandleDirectionChanged;
                _targetManager.TransitionReached += HandleTransitionReached;
                _targetManager.FullRouteCompleted += HandleRouteCompleted;
                _targetManager.RouteFailedEvent += HandleRouteFailed;
            }

            if (_stateController != null)
            {
                _stateController.StateChanged += HandleStateChanged;
            }

            if (_floorTransitionPromptView != null)
            {
                _floorTransitionPromptView.DoneClicked += HandleDoneClicked;
                _floorTransitionPromptView.FinishClicked += HandleFinishClicked;
            }

            if (_arrivedLocationView != null)
            {
                _arrivedLocationView.LeftButtonClicked += HandleArrivedLeftButtonClicked;
                _arrivedLocationView.RightButtonClicked += HandleArrivedRightButtonClicked;
            }

            if (_storeDetailsView != null)
            {
                _storeDetailsView.CloseClicked += HandleStoreDetailsCloseClicked;
            }

            if (_routeSummaryBarView != null)
            {
                _routeSummaryBarView.LeftButtonClicked += HandleRouteSummaryLeftButtonClicked;
            }

            _isSubscribed = true;
        }

        private void UnsubscribeEvents()
        {
            if (!_isSubscribed)
            {
                return;
            }

            if (_targetManager != null)
            {
                _targetManager.RouteMetricsChanged -= HandleRouteMetricsChanged;
                _targetManager.DirectionChanged -= HandleDirectionChanged;
                _targetManager.TransitionReached -= HandleTransitionReached;
                _targetManager.FullRouteCompleted -= HandleRouteCompleted;
                _targetManager.RouteFailedEvent -= HandleRouteFailed;
            }

            if (_stateController != null)
            {
                _stateController.StateChanged -= HandleStateChanged;
            }

            if (_floorTransitionPromptView != null)
            {
                _floorTransitionPromptView.DoneClicked -= HandleDoneClicked;
                _floorTransitionPromptView.FinishClicked -= HandleFinishClicked;
            }

            if (_arrivedLocationView != null)
            {
                _arrivedLocationView.LeftButtonClicked -= HandleArrivedLeftButtonClicked;
                _arrivedLocationView.RightButtonClicked -= HandleArrivedRightButtonClicked;
            }

            if (_storeDetailsView != null)
            {
                _storeDetailsView.CloseClicked -= HandleStoreDetailsCloseClicked;
            }

            if (_routeSummaryBarView != null)
            {
                _routeSummaryBarView.LeftButtonClicked -= HandleRouteSummaryLeftButtonClicked;
            }

            _isSubscribed = false;
        }

        private static string GetDefaultTurnIcon(RouteTurnDirection direction)
        {
            return direction switch
            {
                RouteTurnDirection.Right => "\u2192",
                RouteTurnDirection.Left => "\u2190",
                RouteTurnDirection.Back => "\u21B6",
                _ => "\u2191"
            };
        }

        private static string GetDefaultTurnTitle(RouteTurnDirection direction)
        {
            return direction switch
            {
                RouteTurnDirection.Right => "Поверните направо",
                RouteTurnDirection.Left => "Поверните налево",
                RouteTurnDirection.Back => "Развернитесь назад",
                _ => "Двигайтесь прямо"
            };
        }

        private static string GetDefaultTransitionIcon(FloorTransitionInstruction instruction)
        {
            return instruction switch
            {
                FloorTransitionInstruction.Up => "\u21E7",
                FloorTransitionInstruction.Down => "\u21E9",
                _ => "\u21C5"
            };
        }

        private static string GetDefaultTransitionTitle(FloorTransitionInstruction instruction, int destinationFloor)
        {
            return instruction switch
            {
                FloorTransitionInstruction.Up => $"Поднимитесь на этаж {destinationFloor}",
                FloorTransitionInstruction.Down => $"Спуститесь на этаж {destinationFloor}",
                _ => $"Доедьте на лифте до этажа {destinationFloor}"
            };
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ResolveReferences();
        }
#endif
    }
}
