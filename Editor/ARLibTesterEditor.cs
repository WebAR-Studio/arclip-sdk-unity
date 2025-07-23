using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ARLibTester))]
public class ARLibTesterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var tester = (ARLibTester)target;

        EditorGUILayout.Space(10);
        if (GUILayout.Button("Send Initialized", GUILayout.Height(30)))
        {
            tester.TestInitialized();
        }

        EditorGUILayout.Space(20);
        if (GUILayout.Button("Send Camera Pose Data", GUILayout.Height(30)))
        {
            tester.TestCameraPoseUpdate();
        }

        EditorGUILayout.Space(20);
        if (GUILayout.Button("Send Surface Tracking Data", GUILayout.Height(30)))
        {
            tester.TestSurfaceTrackingUpdate();
        }

        EditorGUILayout.Space(20);
        if (GUILayout.Button("Send Image Tracking Data", GUILayout.Height(30)))
        {
            tester.TestImageTrackingUpdate();
        }

        EditorGUILayout.Space(20);
        if (GUILayout.Button("Send Tracked Images Array Data", GUILayout.Height(30)))
        {
            tester.TestTrackedImagesUpdate();
        }

        EditorGUILayout.Space(20);
        if (GUILayout.Button("Send VPS Ready", GUILayout.Height(30)))
        {
            tester.TestVPSReady();
        }

        EditorGUILayout.Space(20);
        if (GUILayout.Button("Send VPS Position Data", GUILayout.Height(30)))
        {
            tester.TestVPSPositionUpdate();
        }

        EditorGUILayout.Space(20);
        if (GUILayout.Button("Send VPS Localized Data", GUILayout.Height(30)))
        {
            tester.TestVPSLocalized();
        }

        EditorGUILayout.Space(20);
        if (GUILayout.Button("Send VPS Error", GUILayout.Height(30)))
        {
            tester.TestVPSError();
        }

        EditorGUILayout.Space(20);
        if (GUILayout.Button("Send VPS SessionId", GUILayout.Height(30)))
        {
            tester.TestVPSSessionIdUpdate();
        }

        EditorGUILayout.Space(20);
        if (GUILayout.Button("Send Location Data", GUILayout.Height(30)))
        {
            tester.TestLocationUpdate();
        }

        EditorGUILayout.Space(20);
        if (GUILayout.Button("Send Heading Data", GUILayout.Height(30)))
        {
            tester.TestHeadingUpdate();
        }

        EditorGUILayout.Space(20);
        if (GUILayout.Button("Send Detach Memory Request", GUILayout.Height(30)))
        {
            tester.TestDetachMemoryRequest();
        }

        EditorGUILayout.Space(20);
        if (GUILayout.Button("Send Surface Tracking Trigger (shared buffer)", GUILayout.Height(30)))
        {
            tester.TestSurfaceTrackingTrigger();
        }

        EditorGUILayout.Space(20);
        if (GUILayout.Button("Send Camera Render Request", GUILayout.Height(30)))
        {
            tester.TestCameraRenderRequest();
        }
        EditorGUILayout.Space(10);
    }
} 