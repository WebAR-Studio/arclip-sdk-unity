using UnityEngine;

namespace ARClip.Editor
{
    [CreateAssetMenu(fileName = "ARClipBuildSettings", menuName = "ARClip/Build Settings")]
    public sealed class ARClipBuildSettings : ScriptableObject
    {
        public ARClipRuntimeTarget currentTarget = ARClipRuntimeTarget.ARClipApp;

        [Header("WebGL Templates")]
        public string arClipAppTemplate = "PROJECT:ARLib";
        public string eighthWallTemplate = "PROJECT:8thWallVPS";
        public string webXrTemplate = "PROJECT:WebXRVPS";

        [Header("Output")]
        public string outputDirectory = "Builds/WebGL";

        public string GetTemplateFor(ARClipRuntimeTarget target)
        {
            switch (target)
            {
                case ARClipRuntimeTarget.EighthWall:
                    return eighthWallTemplate;
                case ARClipRuntimeTarget.WebXR:
                    return webXrTemplate;
                default:
                    return arClipAppTemplate;
            }
        }
    }
}
