using ARLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ARController : MonoBehaviour
{

    public static ARController Instance { get; private set; }

    public Camera renderCamera;
    private List<GameObject> currentPlanes = new List<GameObject>();
    private List<GameObject> currentImages = new List<GameObject>();

    private float fpsUpdateInterval = 0.5f;
    private float fpsAccumulator = 0;
    private int fpsFrames = 0;
    private float fpsTimeLeft;
    private float currentFPS = 0;

    public ARLibController arlib;

    public event Action<ImagesArrayData> OnTrackedImagesUpdated;
    public event Action<LocationData> OnLocationUpdated;
    public event Action<HeadingData> OnHeadingUpdated;
    public event Action<VPSPoseData> OnVPSPositionUpdated;
    public event Action<String> OnVPSError;
    public event Action<String> OnVPSSessionIdUpdated;

    private UIState currentState = UIState.Loading;

    private Dictionary<string, GameObject> trackedImages = new Dictionary<string, GameObject>();

    void Awake()
    {   
        
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        ARLibController.Initialized += OnInitialized;
        ARLibController.CameraPoseUpdated += OnCameraPoseUpdate;
        ARLibController.SurfaceTrackingUpdated += OnSurfaceTrackingUpdate;
        ARLibController.ImageTrackingUpdated += OnImageTrackingUpdate;
        ARLibController.TrackedImagesArrayUpdate += OnTrackedImagesArrayUpdate;
        ARLibController.VPSInitialized += OnVPSInitialize;
        ARLibController.LocationUpdated += OnLocationUpdate;
        ARLibController.HeadingUpdated += OnHeadingUpdate;
        ARLibController.VPSPositionUpdated += OnVPSPositionUpdate;
        ARLibController.OnVPSErrorHappened += OnVPSErrorHappened;
        ARLibController.OnVPSSessionIdUpdated += OnVPSSessionIdUpdate;
        ARLibController.Initialize();
    }

    void Start() {
        Application.targetFrameRate = 60;
        fpsTimeLeft = fpsUpdateInterval;
        ARLibController.EnableCamera();
    }

    void Update() {
        // fpsTimeLeft -= Time.deltaTime;
        // fpsAccumulator += Time.timeScale / Time.deltaTime;
        // fpsFrames++;

        // if (fpsTimeLeft <= 0) {
        //     currentFPS = fpsAccumulator / fpsFrames;
        //     fpsTimeLeft = fpsUpdateInterval;
        //     fpsAccumulator = 0;
        //     fpsFrames = 0;
        //     Debug.Log("Current FPS: " + currentFPS);
        // }
    }

    public void SetState(UIState state) {
        currentState = state;
    }

    public void preloadImages() {
        TrackingImageData data = new TrackingImageData
        {
            name = "MyImage",
            url = "https://cdn.web-ar.studio/12/242507/media/image/2025-04-0812.14.44_1744218808876.jpg",
            physicalWidth = 0.1f
        };
        ARLibController.AddTrackingImage(data);
    }

    public void OnInitialized() {
        //
    }

    public void OnEnableSurfaceTrackingButtonTap()
    {
        Debug.Log("OnEnableSurfaceTrackingButtonTap");
        ARLibController.EnableSurfaceTracking("both");
    }

    public void OnEnableImageTrackingButtonTap()
    {
        Debug.Log("OnEnableImageTrackingButtonTap");
        ARLibController.EnableImageTracking();
    }

    public void OnEnableVPSButtonTap()
    {
        var settings = new VPSSettings {
            serverUrl = "https://vps-cu.naviar.io/vps/api/v3",
            locationsIds = new[] {"2_floor_668696175ec4c318084343f1"},
            type = "mobile",
            gps = false,
            maxFailsCount = 5
        };
        ARLibController.SetupVPS(settings);
    }

    public void OnDisableTrackingButtonTap()
    {
        ClearOldPlanes();
        ClearImages();
        ARLibController.DisableTracking();
    }

    public void OnEnableLocationButtonTap()
    {
        Debug.Log("OnEnableLocationButtonTap");
        ARLibController.WatchPosition();
        ARLibController.StartHeadingUpdates();
    }

    public void OnDisableLocationButtonTap()
    {
        Debug.Log("OnDisableLocationButtonTap");
        ARLibController.ClearWatch();
        ARLibController.StopHeadingUpdates();
    }

    // Callbacks

    void OnCameraPoseUpdate(CameraPoseData poseData)
    {
        //
    }

    void OnSurfaceTrackingUpdate(PlaneInfo[] planeInfos)
    {
        if (currentState != UIState.SurfaceTracking) {
            ClearOldPlanes();
            return;
        }

        UpdatePlanes(planeInfos);
    }

    void OnImageTrackingUpdate(TrackedImageInfo[] imagesInfo)
    {
        UpdateImages(imagesInfo);
    }

    void OnTrackedImagesArrayUpdate(ImagesArrayData names)
    {
        OnTrackedImagesUpdated?.Invoke(names);
    }

    void OnVPSInitialize()
    {
        ARLibController.StartVPS();
    }

    void OnLocationUpdate(LocationData data)
    {
        if (currentState != UIState.Location) return;
        OnLocationUpdated?.Invoke(data);
    }

    void OnHeadingUpdate(HeadingData data)
    {
        if (currentState != UIState.Location) return;
        OnHeadingUpdated?.Invoke(data);
    }

    void OnVPSErrorHappened(string errorMessage)
    {
         OnVPSError?.Invoke(errorMessage);
    }

    void OnVPSSessionIdUpdate(string sessionId)
    {
        OnVPSSessionIdUpdated?.Invoke(sessionId);
    }

    void OnVPSPositionUpdate(VPSPoseData poseData)
    {
        if (currentState != UIState.VPS) return;
        
        OnVPSPositionUpdated?.Invoke(poseData);
    }

    // Private

    void UpdatePlanes(PlaneInfo[] planeInfos)
    {
        ClearOldPlanes();

        foreach (var planeInfo in planeInfos)
        {
            GameObject planeObj = CreatePlaneObject(planeInfo);
            currentPlanes.Add(planeObj);
        }
    }

    void ClearOldPlanes()
    {
        foreach (var plane in currentPlanes)
        {
            Destroy(plane);
        }
        currentPlanes.Clear();
    }

    GameObject CreatePlaneObject(PlaneInfo planeInfo)
    {
        GameObject planeObject = new GameObject("Plane");

        var poseData = planeInfo.centerPose;

        planeObject.transform.position = new Vector3(poseData.xPos, poseData.yPos, poseData.zPos);
        planeObject.transform.localEulerAngles = new Vector3(poseData.xAngle, poseData.yAngle, poseData.zAngle);

        LineRenderer lr = planeObject.AddComponent<LineRenderer>();
        int vertexCount = planeInfo.vertices.Length / 2;
        lr.positionCount = vertexCount + 1;

        Vector3[] positions = new Vector3[vertexCount + 1];

        for (int i = 0; i < vertexCount; i++)
        {
            float x = planeInfo.vertices[i * 2];
            float z = planeInfo.vertices[i * 2 + 1];
            positions[i] = new Vector3(x, 0, z);
        }
        positions[vertexCount] = positions[0];

        lr.SetPositions(positions);
        lr.startWidth = 0.02f;
        lr.endWidth = 0.02f;
        lr.loop = true;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.useWorldSpace = false;
        lr.transform.parent = planeObject.transform;

        return planeObject;
    }

    GameObject CreateOriginObject(PlaneInfo planeInfo)
    {
        GameObject originObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        var poseData = planeInfo.centerPose;
        originObject.transform.position = new Vector3(poseData.xPos, poseData.yPos, poseData.zPos);
        originObject.transform.localScale = Vector3.one * 0.1f;

        var greenMat = new Material(Shader.Find("Standard"));
        greenMat.color = Color.green;
        originObject.GetComponent<MeshRenderer>().material = greenMat;

        return originObject;
    }


    void UpdateImages(TrackedImageInfo[] imagesInfo)
    {
        foreach (var imageInfo in imagesInfo)
        {
            switch (imageInfo.trackingState)
            {
                case "Update":
                    if (!trackedImages.ContainsKey(imageInfo.name))
                    {
                        GameObject planeObj = CreateImageObject(imageInfo);
                        trackedImages[imageInfo.name] = planeObj;
                        currentImages.Add(planeObj);
                    } else if (trackedImages.TryGetValue(imageInfo.name, out GameObject existingObj)) {
                        UpdateImageObject(existingObj, imageInfo);
                    }
                    break;
                case "Remove":
                    if (trackedImages.TryGetValue(imageInfo.name, out GameObject objToRemove))
                    {
                        trackedImages.Remove(imageInfo.name);
                        currentImages.Remove(objToRemove);
                        Destroy(objToRemove);
                    }
                    break;
            }
        }
    }

    void UpdateImageObject(GameObject obj, TrackedImageInfo info)
    {
        obj.transform.position = new Vector3(
            info.centerPose.xPos,
            info.centerPose.yPos,
            info.centerPose.zPos
        );
        obj.transform.eulerAngles = new Vector3(
            info.centerPose.xAngle,
            info.centerPose.yAngle,
            info.centerPose.zAngle
        );
        obj.transform.localScale = new Vector3(
            info.sizeXmeters,
            0.005f,
            info.sizeZmeters
        );
    }

    void ClearImages()
    {
        foreach (var image in currentImages)
        {
            Destroy(image);
        }
        currentImages.Clear();
    }

    GameObject CreateImageObject(TrackedImageInfo info) {

        Vector3 scale = new Vector3(
            info.sizeXmeters,
            0.005f,
            info.sizeZmeters
        );

        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = new Vector3(
            info.centerPose.xPos,
            info.centerPose.yPos,
            info.centerPose.zPos
        );
        cube.transform.eulerAngles = new Vector3(
            info.centerPose.xAngle,
            info.centerPose.yAngle,
            info.centerPose.zAngle
        );
        cube.transform.localScale = scale;
        cube.name = info.name;

        return cube;
    }

    void OnDestroy()
    {
        ARLibController.Initialized -= OnInitialized;
        ARLibController.CameraPoseUpdated -= OnCameraPoseUpdate;
        ARLibController.SurfaceTrackingUpdated -= OnSurfaceTrackingUpdate;
        ARLibController.ImageTrackingUpdated -= OnImageTrackingUpdate;
        ARLibController.TrackedImagesArrayUpdate -= OnTrackedImagesArrayUpdate;
        ARLibController.VPSInitialized -= OnVPSInitialize;
        ARLibController.VPSPositionUpdated -= OnVPSPositionUpdate;
        ARLibController.OnVPSErrorHappened -= OnVPSErrorHappened;
        ARLibController.OnVPSSessionIdUpdated -= OnVPSSessionIdUpdate;
        ARLibController.LocationUpdated -= OnLocationUpdate;
        ARLibController.HeadingUpdated -= OnHeadingUpdate;
    }
}