using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using ARLib;

public enum UIState
{
    VPS,
    Loading
}

public class UIController : MonoBehaviour
{
    [Header("Start")]
    [SerializeField] private Button startButton;

    [Header("Common Screen")]
    [SerializeField] private GameObject commonScreenPanel;
    [SerializeField] private TextMeshProUGUI screenTitle;
    [SerializeField] private TextMeshProUGUI locationText;
    [SerializeField] private TextMeshProUGUI headingText;

    [Header("Loader")]
    [SerializeField] private GameObject loaderPanel;

    [Header("Route")]
    [SerializeField] private SampleVpsRouteSequencer routeSequencer;

    private UIState currentState = UIState.Loading;
    private bool localized = false;

    private void Start()
    {
        startButton.onClick.AddListener(StartVPS);
        SubscribeToAREvents();
        commonScreenPanel.SetActive(false);
        loaderPanel.SetActive(false);
        ARController.Instance.preloadImages();

    }

    private void StartVPS()
    {
        startButton.gameObject.SetActive(false);
        loaderPanel.SetActive(false);
        commonScreenPanel.SetActive(true);
        screenTitle.text = "VPS";
        currentState = UIState.VPS;
        ARController.Instance.SetState(currentState);
        ARController.Instance.OnEnableVPSButtonTap();
    }

    private void ShowLoader()
    {
        commonScreenPanel.SetActive(false);
        loaderPanel.SetActive(true);
    }

    private void SubscribeToAREvents()
    {
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
            ARController.Instance.OnLocationUpdated -= OnLocationUpdate;
            ARController.Instance.OnHeadingUpdated -= OnHeadingUpdate;
            ARController.Instance.OnVPSPositionUpdated -= OnVPSPositionUpdate;
            ARController.Instance.OnVPSError -= OnVPSError;
            ARController.Instance.OnVPSSessionIdUpdated -= OnVPSSessionIdUpdate;
        }
    }

    private void OnLocationUpdate(LocationData location)
    {
        if (currentState == UIState.VPS)
        {
            locationText.text = $"Latitude: {location.latitude:F6}\nLongitude: {location.longitude:F6}\nAltitude: {location.altitude:F2}m";
        }
    }

    private void OnHeadingUpdate(HeadingData heading)
    {
        if (currentState == UIState.VPS)
        {
            headingText.text = $"Heading: {heading.magneticHeading:F1}\nAccuracy: {heading.headingAccuracy:F1}";
        }
    }

    private void OnVPSPositionUpdate(VPSPoseData poseData)
    {
        if (currentState == UIState.VPS)
        {
            if (!localized)
            {
                localized = true;
                if (routeSequencer != null)
                    routeSequencer.StartRoute();
            }
            headingText.text = "";
            locationText.text = $"VPS Position:\nX: {poseData.localisation.vpsPosition.x:F2}\nY: {poseData.localisation.vpsPosition.y:F2}\nZ: {poseData.localisation.vpsPosition.z:F2}";
        }
    }

    private void OnVPSError(string errorMessage)
    {
        if (currentState == UIState.VPS)
        {
            locationText.text = "";
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
