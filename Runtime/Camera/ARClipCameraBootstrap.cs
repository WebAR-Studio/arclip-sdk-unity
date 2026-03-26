using ARLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ARClip
{
    [DefaultExecutionOrder(-10000)]
    public sealed class ARClipCameraBootstrap : MonoBehaviour
    {
        [SerializeField] private ARLibController arLibController;
        [SerializeField] private ARClipCameraPlaceholder cameraPlaceholder;

        private Camera selectedCamera;

        private void Awake()
        {
            InstallCameraIfNeeded();
        }

        private void InstallCameraIfNeeded()
        {
            if (selectedCamera != null)
            {
                return;
            }

            var config = Resources.Load<ARClipBuildConfig>(ARClipBuildConfig.ResourcesLoadPath);
            var runtimeTarget = config != null ? config.RuntimeTarget : ARClipRuntimeTarget.ARClipApp;

            if (cameraPlaceholder == null)
            {
                cameraPlaceholder = FindObjectOfType<ARClipCameraPlaceholder>();
            }

            if (cameraPlaceholder == null)
            {
                Debug.LogWarning("ARClipCameraBootstrap: camera placeholder was not found.");
                return;
            }

            var selection = cameraPlaceholder.GetSelection(runtimeTarget);
            var configuredRoot = selection.Root != null ? selection.Root : (selection.Camera != null ? selection.Camera.gameObject : null);
            if (configuredRoot == null)
            {
                Debug.LogError($"ARClipCameraBootstrap: no camera root configured for runtime target {runtimeTarget}.");
                return;
            }

            var runtimeRoot = ResolveRootInstance(configuredRoot);
            selectedCamera = ResolveSelectionCamera(selection, configuredRoot, runtimeRoot);
            if (selectedCamera == null)
            {
                Debug.LogError($"ARClipCameraBootstrap: failed to resolve camera for runtime target {runtimeTarget}.");
                return;
            }

            SetRootActivationState(runtimeRoot);

            if (arLibController == null)
            {
                arLibController = FindObjectOfType<ARLibController>();
            }

            if (arLibController == null)
            {
                Debug.LogWarning("ARClipCameraBootstrap: ARLibController was not found.");
                return;
            }

            if (runtimeTarget == ARClipRuntimeTarget.WebXR)
            {
                return;
            }

            arLibController.renderCamera = selectedCamera;
            SynchronizeRenderCameraConsumers(selectedCamera);
        }

        private GameObject ResolveRootInstance(GameObject configuredRoot)
        {
            if (configuredRoot.scene.IsValid())
            {
                return configuredRoot;
            }

            return Instantiate(
                configuredRoot,
                cameraPlaceholder.transform);
        }

        private static Camera ResolveSelectionCamera(
            ARClipCameraSelection selection,
            GameObject configuredRoot,
            GameObject runtimeRoot)
        {
            if (runtimeRoot == null)
            {
                return null;
            }

            var configuredCamera = selection.Camera;
            if (configuredCamera == null)
            {
                return runtimeRoot.GetComponentInChildren<Camera>(true);
            }

            if (configuredRoot.scene.IsValid() && configuredCamera.gameObject.scene.IsValid())
            {
                return configuredCamera;
            }

            if (configuredCamera.gameObject == configuredRoot)
            {
                return runtimeRoot.GetComponent<Camera>();
            }

            var relativePath = BuildRelativePath(configuredRoot.transform, configuredCamera.transform);
            if (!string.IsNullOrEmpty(relativePath))
            {
                var mappedTransform = runtimeRoot.transform.Find(relativePath);
                if (mappedTransform != null)
                {
                    var mappedCamera = mappedTransform.GetComponent<Camera>();
                    if (mappedCamera != null)
                    {
                        return mappedCamera;
                    }
                }
            }

            return runtimeRoot.GetComponentInChildren<Camera>(true);
        }

        private void SetRootActivationState(GameObject activeRoot)
        {
            var configuredRoots = cameraPlaceholder.GetAllManagedRoots();
            var handledRoots = new HashSet<GameObject>();
            for (var i = 0; i < configuredRoots.Length; i++)
            {
                var rootCandidate = configuredRoots[i];
                if (rootCandidate == null || !handledRoots.Add(rootCandidate))
                {
                    continue;
                }

                if (rootCandidate.scene.IsValid())
                {
                    rootCandidate.SetActive(rootCandidate == activeRoot);
                }
            }
        }

        private static string BuildRelativePath(Transform root, Transform target)
        {
            if (root == null || target == null)
            {
                return null;
            }

            if (root == target)
            {
                return string.Empty;
            }

            var segments = new List<string>();
            var current = target;
            while (current != null && current != root)
            {
                segments.Add(current.name);
                current = current.parent;
            }

            if (current != root)
            {
                return null;
            }

            segments.Reverse();
            return string.Join("/", segments);
        }

        private static void SynchronizeRenderCameraConsumers(Camera renderCamera)
        {
            var behaviours = FindObjectsOfType<MonoBehaviour>();
            for (var i = 0; i < behaviours.Length; i++)
            {
                var behaviour = behaviours[i];
                if (behaviour == null)
                {
                    continue;
                }

                var field = behaviour.GetType().GetField(
                    "renderCamera",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (field == null || field.FieldType != typeof(Camera))
                {
                    continue;
                }

                field.SetValue(behaviour, renderCamera);
            }
        }
    }
}
