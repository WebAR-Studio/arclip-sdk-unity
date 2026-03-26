using UnityEditor;
using UnityEngine;

namespace ARClip.Editor
{
    public sealed class ARClipBuildWindow : EditorWindow
    {
        private SerializedObject serializedSettings;
        private ARClipBuildSettings settings;
        private static readonly GUIContent[] RuntimeTargetOptions =
        {
            new GUIContent("ARClip App"),
            new GUIContent("8th Wall"),
            new GUIContent("WebXR"),
        };

        [MenuItem("ARClip/Build Window")]
        public static void Open()
        {
            var window = GetWindow<ARClipBuildWindow>("ARClip Build");
            window.minSize = new Vector2(420f, 420f);
            window.Show();
        }

        private void OnEnable()
        {
            settings = ARClipBuildPipeline.GetOrCreateSettings();
            serializedSettings = new SerializedObject(settings);
        }

        private void OnGUI()
        {
            if (settings == null)
            {
                settings = ARClipBuildPipeline.GetOrCreateSettings();
                serializedSettings = new SerializedObject(settings);
            }

            serializedSettings.Update();

            EditorGUILayout.LabelField("Build Target", EditorStyles.boldLabel);
            var currentTargetProperty = serializedSettings.FindProperty("currentTarget");
            var previousTarget = (ARClipRuntimeTarget)currentTargetProperty.enumValueIndex;
            currentTargetProperty.enumValueIndex = EditorGUILayout.Popup(
                new GUIContent("Runtime Target"),
                currentTargetProperty.enumValueIndex,
                RuntimeTargetOptions);

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("WebGL Templates", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(
                serializedSettings.FindProperty("arClipAppTemplate"),
                new GUIContent("ARClip App Template"));
            EditorGUILayout.PropertyField(
                serializedSettings.FindProperty("eighthWallTemplate"),
                new GUIContent("8th Wall Template"));
            EditorGUILayout.PropertyField(
                serializedSettings.FindProperty("webXrTemplate"),
                new GUIContent("WebXR Template"));
            EditorGUILayout.HelpBox(
                ARClipBuildPipeline.GetTemplateStatus(
                    settings,
                    (ARClipRuntimeTarget)currentTargetProperty.enumValueIndex),
                MessageType.None);

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Output", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedSettings.FindProperty("outputDirectory"));

            serializedSettings.ApplyModifiedProperties();

            var currentTarget = (ARClipRuntimeTarget)currentTargetProperty.enumValueIndex;
            if (currentTarget != previousTarget)
            {
                ARClipBuildPipeline.ApplyTargetSelection(settings, currentTarget);
                GUIUtility.ExitGUI();
            }

            EditorGUILayout.Space(12f);
            DrawScenesSection();
            EditorGUILayout.Space(12f);

            if (currentTarget == ARClipRuntimeTarget.WebXR)
            {
                DrawWebXRPrerequisites();
                EditorGUILayout.Space(12f);
            }

            if (GUILayout.Button("Build Selected Target", GUILayout.Height(32f)))
            {
                ARClipBuildPipeline.BuildCurrentTarget();
            }

            EditorGUILayout.Space(10f);
            DrawGeneratedConfigLink();
            EditorGUILayout.Space(10f);
            EditorGUILayout.HelpBox(
                "The selected runtime target is baked at build time. The build pipeline switches the WebGL template and writes a generated Resources build config used by camera bootstrap at runtime.",
                MessageType.Info);
        }

        private static void DrawWebXRPrerequisites()
        {
            var status = ARClipWebXRSetupUtility.GetStatus();

            EditorGUILayout.LabelField("WebXR Prerequisites", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                status.GetSummary() + " WebXR Export also needs to be enabled in Project Settings -> XR Plug-in Management -> WebGL.",
                status.IsReady ? MessageType.Info : MessageType.Warning);

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(status.IsReady))
                {
                    if (GUILayout.Button("Install WebXR Prerequisites", GUILayout.Height(28f)))
                    {
                        ARClipWebXRSetupUtility.EnsureInstalled();
                    }
                }

                if (GUILayout.Button("Recheck WebXR", GUILayout.Height(28f)))
                {
                    GUIUtility.ExitGUI();
                }
            }
        }

        private static void DrawGeneratedConfigLink()
        {
            var generatedConfig = ARClipBuildPipeline.GetGeneratedConfig();

            EditorGUILayout.LabelField("Generated Build Config", EditorStyles.boldLabel);

            if (generatedConfig == null)
            {
                EditorGUILayout.HelpBox(
                    "No generated build config asset exists yet. It will be created on the first build.",
                    MessageType.None);
                return;
            }

            EditorGUILayout.ObjectField(
                new GUIContent("Current Config"),
                generatedConfig,
                typeof(ARClipBuildConfig),
                false);
        }

        private static void DrawScenesSection()
        {
            var scenes = EditorBuildSettings.scenes;

            EditorGUILayout.LabelField("Build Scenes", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                var enabledCount = 0;
                for (var i = 0; i < scenes.Length; i++)
                {
                    if (scenes[i].enabled)
                    {
                        enabledCount++;
                    }
                }

                EditorGUILayout.LabelField(
                    $"{enabledCount} of {scenes.Length} scene(s) enabled in Build Settings",
                    EditorStyles.miniLabel);

                if (GUILayout.Button("Edit Build Settings", GUILayout.Width(140f)))
                {
                    BuildPlayerWindow.ShowBuildPlayerWindow();
                }
            }

            if (scenes.Length == 0)
            {
                EditorGUILayout.HelpBox(
                    "No scenes are configured in Build Settings. The ARClip build uses all enabled scenes from Unity Build Settings.",
                    MessageType.Warning);
                return;
            }

            for (var i = 0; i < scenes.Length; i++)
            {
                var buildScene = scenes[i];
                var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(buildScene.path);
                var status = buildScene.enabled ? "Enabled" : "Disabled";

                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.ObjectField(
                        new GUIContent($"{i + 1}. {status}"),
                        sceneAsset,
                        typeof(SceneAsset),
                        false);
                }
            }
        }
    }
}
