using UnityEngine;

namespace ARClip
{
    public sealed class ARClipBuildConfig : ScriptableObject
    {
        public const string ResourcesLoadPath = "ARClipGenerated/ARClipBuildConfig";

        [SerializeField] private ARClipRuntimeTarget runtimeTarget = ARClipRuntimeTarget.ARClipApp;

        public ARClipRuntimeTarget RuntimeTarget => runtimeTarget;
    }
}
