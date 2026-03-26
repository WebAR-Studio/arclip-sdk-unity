using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ARClip.Editor
{
    public static class ARClipBuildPipeline
    {
        private const string SettingsDirectory = "Assets/ARClipSettings";
        private const string SettingsAssetPath = SettingsDirectory + "/ARClipBuildSettings.asset";
        private const string GeneratedRoot = "Assets/ARClipGenerated";
        private const string GeneratedResourcesDirectory = GeneratedRoot + "/Resources";
        private const string GeneratedConfigDirectory = GeneratedResourcesDirectory + "/ARClipGenerated";
        private const string GeneratedConfigAssetPath = GeneratedConfigDirectory + "/ARClipBuildConfig.asset";

        public static ARClipBuildSettings GetOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<ARClipBuildSettings>(SettingsAssetPath);
            if (settings != null)
            {
                return settings;
            }

            EnsureFolder("Assets", "ARClipSettings");

            settings = ScriptableObject.CreateInstance<ARClipBuildSettings>();
            AssetDatabase.CreateAsset(settings, SettingsAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return settings;
        }

        public static BuildReport BuildCurrentTarget(BuildOptions buildOptions = BuildOptions.None)
        {
            var settings = GetOrCreateSettings();
            return Build(settings, settings.currentTarget, buildOptions);
        }

        public static string ApplyTargetSelection(ARClipBuildSettings settings, ARClipRuntimeTarget runtimeTarget)
        {
            settings.currentTarget = runtimeTarget;
            EditorUtility.SetDirty(settings);

            var templateId = settings.GetTemplateFor(runtimeTarget);
            var templateMessage = string.Empty;
            if (!string.IsNullOrWhiteSpace(templateId))
            {
                templateMessage = TryApplyTemplate(runtimeTarget, templateId, false);
            }

            AssetDatabase.SaveAssets();

            if (runtimeTarget == ARClipRuntimeTarget.WebXR)
            {
                return CombineMessages(templateMessage, ARClipWebXRSetupUtility.AutoConfigureForSelection());
            }

            return CombineMessages(templateMessage, ARClipWebXRSetupUtility.AutoCleanupForNonWebXRSelection());
        }

        public static BuildReport Build(ARClipBuildSettings settings, ARClipRuntimeTarget runtimeTarget, BuildOptions buildOptions = BuildOptions.None)
        {
            ApplyTargetSelection(settings, runtimeTarget);

            if (runtimeTarget == ARClipRuntimeTarget.WebXR)
            {
                ARClipWebXRSetupUtility.ThrowIfInvalidForBuild();
            }

            var templateId = settings.GetTemplateFor(runtimeTarget);
            if (string.IsNullOrWhiteSpace(templateId))
            {
                throw new BuildFailedException($"ARClip build template is not configured for {runtimeTarget}.");
            }

            TryApplyTemplate(runtimeTarget, templateId, true);
            WriteBuildConfig(runtimeTarget);

            var outputRoot = GetOutputRoot(settings.outputDirectory);
            var outputPath = Path.Combine(outputRoot, runtimeTarget.ToString());

            Directory.CreateDirectory(outputPath);

            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = GetEnabledScenes(),
                locationPathName = outputPath,
                target = BuildTarget.WebGL,
                options = buildOptions,
            };

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return BuildPipeline.BuildPlayer(buildPlayerOptions);
        }

        public static void RevealGeneratedConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<ARClipBuildConfig>(GeneratedConfigAssetPath);
            if (config != null)
            {
                EditorGUIUtility.PingObject(config);
                Selection.activeObject = config;
            }
        }

        public static ARClipBuildConfig GetGeneratedConfig()
        {
            return AssetDatabase.LoadAssetAtPath<ARClipBuildConfig>(GeneratedConfigAssetPath);
        }

        public static string GetTemplateStatus(ARClipBuildSettings settings, ARClipRuntimeTarget runtimeTarget)
        {
            var templateId = settings.GetTemplateFor(runtimeTarget);
            if (string.IsNullOrWhiteSpace(templateId))
            {
                return "No WebGL template configured.";
            }

            var activeTemplate = PlayerSettings.WebGL.template;
            if (TryGetProjectTemplateIndexPath(runtimeTarget, out var indexPath))
            {
                if (!File.Exists(indexPath))
                {
                    return $"Configured: {templateId}. Active: {activeTemplate}. Expected package template: {GetControlledTemplateFolderName(runtimeTarget)}. Missing: {indexPath}";
                }

                return $"Configured: {templateId}. Active: {activeTemplate}. Package template found: {indexPath}";
            }

            return $"Configured: {templateId}. Active: {activeTemplate}.";
        }

        private static void WriteBuildConfig(ARClipRuntimeTarget runtimeTarget)
        {
            EnsureFolder("Assets", "ARClipGenerated");
            EnsureFolder(GeneratedRoot, "Resources");
            EnsureFolder(GeneratedResourcesDirectory, "ARClipGenerated");

            var config = AssetDatabase.LoadAssetAtPath<ARClipBuildConfig>(GeneratedConfigAssetPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<ARClipBuildConfig>();
                AssetDatabase.CreateAsset(config, GeneratedConfigAssetPath);
            }

            var serializedObject = new SerializedObject(config);
            serializedObject.FindProperty("runtimeTarget").enumValueIndex = (int)runtimeTarget;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(config);
        }

        private static string[] GetEnabledScenes()
        {
            var enabledScenes = EditorBuildSettings.scenes;
            var scenePaths = new System.Collections.Generic.List<string>(enabledScenes.Length);

            foreach (var scene in enabledScenes)
            {
                if (scene.enabled)
                {
                    scenePaths.Add(scene.path);
                }
            }

            if (scenePaths.Count == 0)
            {
                throw new BuildFailedException("ARClip build failed because no enabled scenes were found in EditorBuildSettings.");
            }

            return scenePaths.ToArray();
        }

        private static string GetOutputRoot(string configuredPath)
        {
            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? Directory.GetCurrentDirectory();
            var relativePath = string.IsNullOrWhiteSpace(configuredPath) ? "Builds/WebGL" : configuredPath;
            return Path.GetFullPath(Path.Combine(projectRoot, relativePath));
        }

        private static void EnsureFolder(string parentFolder, string folderName)
        {
            var folderPath = $"{parentFolder}/{folderName}";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder(parentFolder, folderName);
            }
        }

        private static string TryApplyTemplate(ARClipRuntimeTarget runtimeTarget, string templateId, bool failIfMissing)
        {
            if (TryGetProjectTemplateIndexPath(runtimeTarget, out var indexPath) && !File.Exists(indexPath))
            {
                var message = $"ARClip build template is missing. Expected project template at '{indexPath}'. Copy the template folder into Assets/WebGLTemplates before building.";
                if (failIfMissing)
                {
                    throw new BuildFailedException(message);
                }

                return message;
            }

            var appliedTemplate = ApplyTemplateWithCompatibilityFallback(runtimeTarget, templateId, failIfMissing);
            return $"Template set to {appliedTemplate}.";
        }

        private static string ApplyTemplateWithCompatibilityFallback(ARClipRuntimeTarget runtimeTarget, string templateId, bool failIfNotApplied)
        {
            AssetDatabase.Refresh();
            var candidates = GetTemplateCandidates(runtimeTarget, templateId);
            for (var i = 0; i < candidates.Length; i++)
            {
                var candidate = candidates[i];
                PlayerSettings.WebGL.template = candidate;
                var activeTemplate = PlayerSettings.WebGL.template;
                if (MatchesAnyCandidate(activeTemplate, candidates))
                {
                    return activeTemplate;
                }
            }

            var message = $"Failed to apply WebGL template '{templateId}'. Active Unity template is still '{PlayerSettings.WebGL.template}'.";
            if (failIfNotApplied)
            {
                throw new BuildFailedException(message);
            }

            return message;
        }

        private static string[] GetTemplateCandidates(ARClipRuntimeTarget runtimeTarget, string templateId)
        {
            var candidates = new System.Collections.Generic.List<string>();
            AddTemplateCandidates(candidates, GetControlledTemplateFolderName(runtimeTarget));
            AddTemplateCandidates(candidates, templateId);
            return candidates.ToArray();
        }

        private static bool TryGetProjectTemplateIndexPath(ARClipRuntimeTarget runtimeTarget, out string indexPath)
        {
            indexPath = null;
            var templateName = GetControlledTemplateFolderName(runtimeTarget);
            if (string.IsNullOrWhiteSpace(templateName))
            {
                return false;
            }

            indexPath = Path.Combine("Assets", "WebGLTemplates", templateName, "index.html");
            return true;
        }

        private static void AddTemplateCandidates(System.Collections.Generic.List<string> candidates, string templateId)
        {
            if (string.IsNullOrWhiteSpace(templateId))
            {
                return;
            }

            var normalized = templateId.Trim();
            if (!TryExtractProjectTemplateName(normalized, out var templateName))
            {
                if (!candidates.Contains(normalized))
                {
                    candidates.Add(normalized);
                }

                return;
            }

            var projectCandidate = $"PROJECT:{templateName}";
            if (!candidates.Contains(projectCandidate))
            {
                candidates.Add(projectCandidate);
            }

            if (!candidates.Contains(templateName))
            {
                candidates.Add(templateName);
            }
        }

        private static string GetControlledTemplateFolderName(ARClipRuntimeTarget runtimeTarget)
        {
            switch (runtimeTarget)
            {
                case ARClipRuntimeTarget.EighthWall:
                    return "8thWallVPS";
                case ARClipRuntimeTarget.WebXR:
                    return "WebXRVPS";
                default:
                    return "ARLib";
            }
        }

        private static bool TryExtractProjectTemplateName(string templateId, out string templateName)
        {
            templateName = null;
            if (string.IsNullOrWhiteSpace(templateId))
            {
                return false;
            }

            var normalized = templateId.Trim();
            if (normalized.StartsWith("PROJECT:"))
            {
                templateName = normalized.Substring("PROJECT:".Length);
                return !string.IsNullOrWhiteSpace(templateName);
            }

            if (normalized.Contains(":"))
            {
                return false;
            }

            templateName = normalized;
            return true;
        }

        private static string CombineMessages(string first, string second)
        {
            if (string.IsNullOrWhiteSpace(first)) return second;
            if (string.IsNullOrWhiteSpace(second)) return first;
            return $"{first} {second}";
        }

        private static bool MatchesAnyCandidate(string activeTemplate, string[] candidates)
        {
            if (string.IsNullOrWhiteSpace(activeTemplate))
            {
                return false;
            }

            var normalizedActive = activeTemplate.Trim();
            for (var i = 0; i < candidates.Length; i++)
            {
                if (normalizedActive == candidates[i])
                {
                    return true;
                }
            }

            if (TryExtractProjectTemplateName(normalizedActive, out var activeName))
            {
                for (var i = 0; i < candidates.Length; i++)
                {
                    if (TryExtractProjectTemplateName(candidates[i], out var candidateName) && activeName == candidateName)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
