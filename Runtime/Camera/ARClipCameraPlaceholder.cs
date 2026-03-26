using UnityEngine;

namespace ARClip
{
    public readonly struct ARClipCameraSelection
    {
        public ARClipCameraSelection(Camera camera, GameObject root)
        {
            Camera = camera;
            Root = root;
        }

        public Camera Camera { get; }

        public GameObject Root { get; }
    }

    public sealed class ARClipCameraPlaceholder : MonoBehaviour
    {
        [SerializeField] private Camera arClipAppCamera;
        [SerializeField] private Camera eighthWallCamera;
        [SerializeField] private Camera webXrCamera;
        [SerializeField] private GameObject webXrRigRoot;

        public ARClipCameraSelection GetSelection(ARClipRuntimeTarget runtimeTarget)
        {
            switch (runtimeTarget)
            {
                case ARClipRuntimeTarget.EighthWall:
                {
                    var camera = eighthWallCamera != null ? eighthWallCamera : arClipAppCamera;
                    return new ARClipCameraSelection(camera, camera != null ? camera.gameObject : null);
                }
                case ARClipRuntimeTarget.WebXR:
                {
                    if (webXrRigRoot != null)
                    {
                        return new ARClipCameraSelection(webXrCamera, webXrRigRoot);
                    }

                    return new ARClipCameraSelection(webXrCamera, webXrCamera != null ? webXrCamera.gameObject : null);
                }
                default:
                    return new ARClipCameraSelection(arClipAppCamera, arClipAppCamera != null ? arClipAppCamera.gameObject : null);
            }
        }

        public GameObject[] GetAllManagedRoots()
        {
            return new[]
            {
                arClipAppCamera != null ? arClipAppCamera.gameObject : null,
                eighthWallCamera != null ? eighthWallCamera.gameObject : null,
                webXrRigRoot != null ? webXrRigRoot : (webXrCamera != null ? webXrCamera.gameObject : null),
            };
        }
    }
}
