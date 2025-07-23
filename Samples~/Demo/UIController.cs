using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using ARLib;

public enum UIState
{
    MainMenu,
    ImageTracking,
    SurfaceTracking,
    VPS,
    Location,
    Loading
}

public class UIController : MonoBehaviour
{
    [Header("Main Menu")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private Button imageTrackingButton;
    [SerializeField] private Button surfaceTrackingButton;
    [SerializeField] private Button vpsButton;
    [SerializeField] private Button locationButton;

    [Header("Common Screen")]
    [SerializeField] private GameObject commonScreenPanel;
    [SerializeField] private TextMeshProUGUI screenTitle;
    [SerializeField] private Button backButton;
    [SerializeField] private TextMeshProUGUI locationText;
    [SerializeField] private TextMeshProUGUI headingText;

    [Header("Loader")]
    [SerializeField] private GameObject loaderPanel;

    private Dictionary<UIState, Action> stateEnterActions = new Dictionary<UIState, Action>();
    private Dictionary<UIState, Action> stateExitActions = new Dictionary<UIState, Action>();
    private UIState currentState = UIState.Loading;

    private void Start()
    {
        InitializeStateActions();
        SetupButtons();
        SubscribeToAREvents();
        SwitchState(currentState);
        ARController.Instance.preloadImages();

#if UNITY_EDITOR
        SwitchState(UIState.MainMenu);
#endif
    }

    private void SetupButtons()
    {
        imageTrackingButton.onClick.AddListener(() => SwitchState(UIState.ImageTracking));
        surfaceTrackingButton.onClick.AddListener(() => SwitchState(UIState.SurfaceTracking));
        vpsButton.onClick.AddListener(() => SwitchState(UIState.VPS));
        locationButton.onClick.AddListener(() => SwitchState(UIState.Location));
        backButton.onClick.AddListener(() => SwitchState(UIState.MainMenu));
    }

    private void InitializeStateActions()
    {
        // Main Menu
        stateEnterActions[UIState.MainMenu] = ShowMainMenu;
        stateExitActions[UIState.MainMenu] = () => {
            Debug.Log("Exit main menu");
        };
        
        // Image Tracking
        stateEnterActions[UIState.ImageTracking] = () => {
            ARController.Instance.OnEnableImageTrackingButtonTap();
            ShowScreen("Image Tracking", () => SwitchState(UIState.MainMenu));
        };
        stateExitActions[UIState.ImageTracking] = () => {
            ARController.Instance.OnDisableTrackingButtonTap();
        };
        
        // Surface Tracking
        stateEnterActions[UIState.SurfaceTracking] = () => {
            ARController.Instance.OnEnableSurfaceTrackingButtonTap();
            ShowScreen("Surface Tracking", () => SwitchState(UIState.MainMenu));
        };
        stateExitActions[UIState.SurfaceTracking] = () => {
            ARController.Instance.OnDisableTrackingButtonTap();
        };
        
        // VPS
        stateEnterActions[UIState.VPS] = () => {
            ARController.Instance.OnEnableVPSButtonTap();
            ShowScreen("VPS", () => SwitchState(UIState.MainMenu));
        };
        stateExitActions[UIState.VPS] = () => {
            ARController.Instance.OnDisableTrackingButtonTap();
        };

        // Location
        stateEnterActions[UIState.Location] = () => {
            ARController.Instance.OnEnableLocationButtonTap();
            ShowScreen("Location", () => SwitchState(UIState.MainMenu));
        };
        stateExitActions[UIState.Location] = () => {
            ARController.Instance.OnDisableLocationButtonTap();
        };

        // Loading
        stateEnterActions[UIState.Loading] = ShowLoader;
        stateExitActions[UIState.Loading] = HideLoader;
    }

    public void SwitchState(UIState newState)
    {
        if (stateExitActions.ContainsKey(currentState))
        {
            stateExitActions[currentState]?.Invoke();
        }

        currentState = newState;
        ARController.Instance.SetState(currentState);

        if (stateEnterActions.ContainsKey(newState))
        {
            stateEnterActions[newState]?.Invoke();
        }
    }

    private void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        commonScreenPanel.SetActive(false);
    }

    private void ShowScreen(string title, Action backAction)
    {
        mainMenuPanel.SetActive(false);
        commonScreenPanel.SetActive(true);
        screenTitle.text = title;
        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(() => backAction?.Invoke());
    }

    private void ShowLoader()
    {
        mainMenuPanel.SetActive(false);
        commonScreenPanel.SetActive(false);
        loaderPanel.SetActive(true);
    }

    private void HideLoader()
    {
        loaderPanel.SetActive(false);
    }

    private void SubscribeToAREvents()
    {
        ARController.Instance.OnTrackedImagesUpdated += OnTrackedImagesUpdated;
        ARController.Instance.OnLocationUpdated += OnLocationUpdate;
        ARController.Instance.OnHeadingUpdated += OnHeadingUpdate;
        ARController.Instance.OnVPSPositionUpdated += OnVPSPositionUpdate;
        ARController.Instance.OnVPSError += OnVPSError;
        ARController.Instance.OnVPSSessionIdUpdated += OnVPSSessionIdUpdate;
    }

    private void OnDestroy()
    {
        if (ARController.Instance != null)
        {
            ARController.Instance.OnTrackedImagesUpdated -= OnTrackedImagesUpdated;
            ARController.Instance.OnLocationUpdated -= OnLocationUpdate;
            ARController.Instance.OnHeadingUpdated -= OnHeadingUpdate;
            ARController.Instance.OnVPSPositionUpdated -= OnVPSPositionUpdate;
            ARController.Instance.OnVPSError -= OnVPSError;
            ARController.Instance.OnVPSSessionIdUpdated -= OnVPSSessionIdUpdate;
        }
    }

    private void OnTrackedImagesUpdated(ImagesArrayData images)
    {
        SwitchState(UIState.MainMenu);
    }

    private void OnLocationUpdate(LocationData location)
    {
        if (currentState == UIState.Location)
        {
            locationText.text = $"Latitude: {location.latitude:F6}\nLongitude: {location.longitude:F6}\nAltitude: {location.altitude:F2}m";
        }
    }

    private void OnHeadingUpdate(HeadingData heading)
    {
        if (currentState == UIState.Location)
        {
            headingText.text = $"Heading: {heading.magneticHeading:F1}°\nAccuracy: {heading.headingAccuracy:F1}°";
        }
    }

    private void OnVPSPositionUpdate(VPSPoseData poseData)
    {
        if (currentState == UIState.VPS)
        {
            locationText.text = $"VPS Position:\nX: {poseData.localisation.vpsPosition.x:F2}\nY: {poseData.localisation.vpsPosition.y:F2}\nZ: {poseData.localisation.vpsPosition.z:F2}";
        }
    }

    private void OnVPSError(string errorMessage)
    {
        if (currentState == UIState.VPS)
        {
            headingText.text = $"VPS Error:\n{errorMessage}";
        }
    }

    private void OnVPSSessionIdUpdate(string sessionId)
    {
        if (currentState == UIState.VPS)
        {
            headingText.text = $"VPS Session ID:\n{sessionId}";
        }
    }
} 