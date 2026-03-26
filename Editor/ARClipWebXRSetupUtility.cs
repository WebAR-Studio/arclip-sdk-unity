using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.PackageManager;
using UnityEngine;

namespace ARClip.Editor
{
    internal readonly struct ARClipWebXRSetupStatus
    {
        public ARClipWebXRSetupStatus(bool manifestExists, bool hasScopedRegistry, bool hasManifestDependency, bool hasInstalledPackage)
        {
            ManifestExists = manifestExists;
            HasScopedRegistry = hasScopedRegistry;
            HasManifestDependency = hasManifestDependency;
            HasInstalledPackage = hasInstalledPackage;
        }

        public bool ManifestExists { get; }
        public bool HasScopedRegistry { get; }
        public bool HasManifestDependency { get; }
        public bool HasInstalledPackage { get; }
        public bool IsReady => ManifestExists && HasInstalledPackage;

        public string GetSummary()
        {
            if (!ManifestExists)
            {
                return "Packages/manifest.json was not found. WebXR prerequisites cannot be validated.";
            }

            if (IsReady)
            {
                if (HasScopedRegistry && HasManifestDependency)
                {
                    return "WebXR prerequisites are installed: OpenUPM scoped registry for com.de-panther and package com.de-panther.webxr.";
                }

                if (HasInstalledPackage)
                {
                    return "Package com.de-panther.webxr is already installed. OpenUPM registry or direct manifest dependency is missing, but WebXR package resolution is already satisfied for this project.";
                }
            }

            var missingRegistry = HasScopedRegistry ? string.Empty : "OpenUPM scoped registry (https://package.openupm.com, scope com.de-panther)";
            var missingPackage = HasInstalledPackage ? string.Empty : "installed package com.de-panther.webxr";

            if (!string.IsNullOrEmpty(missingRegistry) && !string.IsNullOrEmpty(missingPackage))
            {
                return $"Missing WebXR prerequisites: {missingRegistry}; {missingPackage}.";
            }

            return $"Missing WebXR prerequisite: {missingRegistry}{missingPackage}.";
        }

        public string GetBuildFailureMessage()
        {
            return $"{GetSummary()} Use ARClip/Build Window -> Install WebXR Prerequisites, then wait for Unity package resolution. Also enable WebXR Export in Project Settings -> XR Plug-in Management -> WebGL.";
        }
    }

    internal static class ARClipWebXRSetupUtility
    {
        private const string OpenUpmRegistryName = "package.openupm.com";
        private const string OpenUpmRegistryUrl = "https://package.openupm.com";
        private const string OpenUpmScope = "com.de-panther";
        private const string WebXRPackageName = "com.de-panther.webxr";
        private const string WebXRPackageVersion = "0.22.1";

        private static string ManifestPath =>
            Path.Combine(Directory.GetParent(Application.dataPath)?.FullName ?? Directory.GetCurrentDirectory(), "Packages/manifest.json");

        private static string PackagesLockPath =>
            Path.Combine(Directory.GetParent(Application.dataPath)?.FullName ?? Directory.GetCurrentDirectory(), "Packages/packages-lock.json");

        public static ARClipWebXRSetupStatus GetStatus()
        {
            if (!File.Exists(ManifestPath))
            {
                return new ARClipWebXRSetupStatus(false, false, false, false);
            }

            var manifestJson = File.ReadAllText(ManifestPath);
            var hasScopedRegistry = manifestJson.IndexOf($"\"{OpenUpmRegistryUrl}\"", System.StringComparison.Ordinal) >= 0
                                    && manifestJson.IndexOf($"\"{OpenUpmScope}\"", System.StringComparison.Ordinal) >= 0;
            var hasManifestDependency = manifestJson.IndexOf($"\"{WebXRPackageName}\"", System.StringComparison.Ordinal) >= 0;
            var hasInstalledPackage = hasManifestDependency || HasInstalledPackageInLock();

            return new ARClipWebXRSetupStatus(true, hasScopedRegistry, hasManifestDependency, hasInstalledPackage);
        }

        public static void EnsureInstalled()
        {
            if (!File.Exists(ManifestPath))
            {
                throw new BuildFailedException("Packages/manifest.json was not found.");
            }

            var manifestJson = File.ReadAllText(ManifestPath);
            manifestJson = EnsureOpenUpmRegistry(manifestJson);
            manifestJson = EnsureWebXRDependency(manifestJson);
            File.WriteAllText(ManifestPath, manifestJson);

            AssetDatabase.Refresh();
            Client.Resolve();
        }

        public static void ThrowIfInvalidForBuild()
        {
            var status = GetStatus();
            if (!status.IsReady)
            {
                throw new BuildFailedException(status.GetBuildFailureMessage());
            }
        }

        public static string AutoConfigureForSelection()
        {
            var status = GetStatus();
            if (!status.HasInstalledPackage)
            {
                EnsureInstalled();
                return "Installed WebXR package prerequisites. Wait for Unity package resolution, then select WebXR again to finish XR Plug-in Management setup.";
            }

            if (TryEnableWebXRLoaderForWebGL(out var configurationMessage))
            {
                return configurationMessage;
            }

            return configurationMessage;
        }

        public static string AutoCleanupForNonWebXRSelection()
        {
            if (TryDisableWebXRLoaderForWebGL(out var message))
            {
                return message;
            }

            return message;
        }

        private static bool HasInstalledPackageInLock()
        {
            if (!File.Exists(PackagesLockPath))
            {
                return false;
            }

            var lockJson = File.ReadAllText(PackagesLockPath);
            return lockJson.IndexOf($"\"{WebXRPackageName}\"", System.StringComparison.Ordinal) >= 0;
        }

        private static bool TryEnableWebXRLoaderForWebGL(out string message)
        {
            const string buildTargetSettingsTypeName = "UnityEditor.XR.Management.XRGeneralSettingsPerBuildTarget";
            const string metadataStoreTypeName = "UnityEditor.XR.Management.Metadata.XRPackageMetadataStore";
            const string webXrLoaderTypeName = "WebXR.WebXRLoader";

            var buildTargetSettingsType = FindType(buildTargetSettingsTypeName);
            var metadataStoreType = FindType(metadataStoreTypeName);
            var webXrLoaderType = FindType(webXrLoaderTypeName);

            if (buildTargetSettingsType == null || metadataStoreType == null || webXrLoaderType == null)
            {
                message = "WebXR package is installed, but Unity XR Management or WebXR loader types are not loaded yet. Wait for script reload, then select WebXR again.";
                return false;
            }

            var getOrCreateMethod = buildTargetSettingsType.GetMethod("GetOrCreate", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            var managerForBuildTargetMethod = buildTargetSettingsType.GetMethod("ManagerSettingsForBuildTarget", BindingFlags.Instance | BindingFlags.Public);
            var createDefaultManagerMethod = buildTargetSettingsType.GetMethod("CreateDefaultManagerSettingsForBuildTarget", BindingFlags.Instance | BindingFlags.Public);

            if (getOrCreateMethod == null || managerForBuildTargetMethod == null || createDefaultManagerMethod == null)
            {
                message = "Failed to access Unity XR Management editor APIs for WebXR auto-configuration.";
                return false;
            }

            var settingsPerBuildTarget = getOrCreateMethod.Invoke(null, null);
            if (settingsPerBuildTarget == null)
            {
                message = "Failed to create or load XR General Settings for build targets.";
                return false;
            }

            var managerSettings = managerForBuildTargetMethod.Invoke(settingsPerBuildTarget, new object[] { BuildTargetGroup.WebGL });
            if (managerSettings == null)
            {
                createDefaultManagerMethod.Invoke(settingsPerBuildTarget, new object[] { BuildTargetGroup.WebGL });
                managerSettings = managerForBuildTargetMethod.Invoke(settingsPerBuildTarget, new object[] { BuildTargetGroup.WebGL });
            }

            if (managerSettings == null)
            {
                message = "Failed to create XR Manager Settings for WebGL.";
                return false;
            }

            if (HasLoaderAssigned(managerSettings, webXrLoaderType.FullName))
            {
                message = "Applied WebXR target selection and confirmed WebXR Export loader is enabled for WebGL.";
                return true;
            }

            var assignLoaderMethod = metadataStoreType.GetMethod("AssignLoader", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (assignLoaderMethod == null)
            {
                message = "Failed to access XRPackageMetadataStore.AssignLoader for WebXR auto-configuration.";
                return false;
            }

            var assigned = assignLoaderMethod.Invoke(null, new[] { managerSettings, webXrLoaderType.FullName, (object)BuildTargetGroup.WebGL });
            var didAssign = assigned is bool boolResult && boolResult;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (didAssign || HasLoaderAssigned(managerSettings, webXrLoaderType.FullName))
            {
                message = "Applied WebXR target selection and enabled WebXR Export in XR Plug-in Management for WebGL.";
                return true;
            }

            message = "WebXR package is installed, but enabling the WebXR Export loader for WebGL failed. Check XR Plug-in Management -> WebGL manually.";
            return false;
        }

        private static bool TryDisableWebXRLoaderForWebGL(out string message)
        {
            const string buildTargetSettingsTypeName = "UnityEditor.XR.Management.XRGeneralSettingsPerBuildTarget";
            const string metadataStoreTypeName = "UnityEditor.XR.Management.Metadata.XRPackageMetadataStore";
            const string webXrLoaderTypeName = "WebXR.WebXRLoader";

            var buildTargetSettingsType = FindType(buildTargetSettingsTypeName);
            var metadataStoreType = FindType(metadataStoreTypeName);
            var webXrLoaderType = FindType(webXrLoaderTypeName);

            if (buildTargetSettingsType == null || metadataStoreType == null || webXrLoaderType == null)
            {
                message = "Applied non-WebXR target selection. WebXR loader cleanup was skipped because XR Management or WebXR types are not loaded.";
                return false;
            }

            var getOrCreateMethod = buildTargetSettingsType.GetMethod("GetOrCreate", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            var managerForBuildTargetMethod = buildTargetSettingsType.GetMethod("ManagerSettingsForBuildTarget", BindingFlags.Instance | BindingFlags.Public);
            if (getOrCreateMethod == null || managerForBuildTargetMethod == null)
            {
                message = "Applied non-WebXR target selection. Failed to access XR Management APIs for WebXR loader cleanup.";
                return false;
            }

            var settingsPerBuildTarget = getOrCreateMethod.Invoke(null, null);
            if (settingsPerBuildTarget == null)
            {
                message = "Applied non-WebXR target selection. XR General Settings were not available for WebXR loader cleanup.";
                return false;
            }

            var managerSettings = managerForBuildTargetMethod.Invoke(settingsPerBuildTarget, new object[] { BuildTargetGroup.WebGL });
            if (managerSettings == null)
            {
                message = "Applied non-WebXR target selection. No WebGL XR Manager Settings were present, so no cleanup was needed.";
                return true;
            }

            if (!HasLoaderAssigned(managerSettings, webXrLoaderType.FullName))
            {
                message = "Applied non-WebXR target selection and confirmed WebXR Export loader is already disabled for WebGL.";
                return true;
            }

            var removeLoaderMethod = metadataStoreType.GetMethod("RemoveLoader", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (removeLoaderMethod == null)
            {
                message = "Applied non-WebXR target selection, but failed to access XRPackageMetadataStore.RemoveLoader for WebXR cleanup.";
                return false;
            }

            var removed = removeLoaderMethod.Invoke(null, new[] { managerSettings, webXrLoaderType.FullName, (object)BuildTargetGroup.WebGL });
            var didRemove = removed is bool boolResult && boolResult;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (didRemove || !HasLoaderAssigned(managerSettings, webXrLoaderType.FullName))
            {
                message = "Applied non-WebXR target selection and disabled WebXR Export loader for WebGL.";
                return true;
            }

            message = "Applied non-WebXR target selection, but disabling the WebXR Export loader for WebGL failed. Check XR Plug-in Management -> WebGL manually.";
            return false;
        }

        private static bool HasLoaderAssigned(object managerSettings, string loaderTypeFullName)
        {
            if (managerSettings == null)
            {
                return false;
            }

            var managerSettingsType = managerSettings.GetType();
            var activeLoadersProperty = managerSettingsType.GetProperty("activeLoaders", BindingFlags.Instance | BindingFlags.Public);
            var activeLoaders = activeLoadersProperty?.GetValue(managerSettings, null) as System.Collections.IEnumerable;
            if (activeLoaders == null)
            {
                return false;
            }

            foreach (var loader in activeLoaders)
            {
                if (loader == null)
                {
                    continue;
                }

                var loaderType = loader.GetType();
                if (loaderType.FullName == loaderTypeFullName)
                {
                    return true;
                }
            }

            return false;
        }

        private static Type FindType(string fullName)
        {
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(fullName, false);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }

        private static string EnsureOpenUpmRegistry(string manifestJson)
        {
            if (manifestJson.IndexOf($"\"{OpenUpmRegistryUrl}\"", System.StringComparison.Ordinal) >= 0
                && manifestJson.IndexOf($"\"{OpenUpmScope}\"", System.StringComparison.Ordinal) >= 0)
            {
                return manifestJson;
            }

            if (manifestJson.IndexOf($"\"{OpenUpmRegistryUrl}\"", System.StringComparison.Ordinal) >= 0)
            {
                if (TryFindContainingObjectRange(manifestJson, $"\"{OpenUpmRegistryUrl}\"", out var objectStart, out var objectEnd)
                    && TryFindPropertyValueRange(manifestJson, "\"scopes\"", objectStart, objectEnd, out var scopesStart, out var scopesEnd))
                {
                    var newScopeEntry = scopesEnd - scopesStart > 2
                        ? ",\n        \"" + OpenUpmScope + "\""
                        : "\n        \"" + OpenUpmScope + "\"\n      ";
                    return manifestJson.Insert(scopesEnd - 1, newScopeEntry);
                }

                return manifestJson;
            }

            var registryEntry =
                "  \"scopedRegistries\": [\n" +
                "    {\n" +
                $"      \"name\": \"{OpenUpmRegistryName}\",\n" +
                $"      \"url\": \"{OpenUpmRegistryUrl}\",\n" +
                "      \"scopes\": [\n" +
                $"        \"{OpenUpmScope}\"\n" +
                "      ]\n" +
                "    }\n" +
                "  ],\n";

            var dependenciesKeyIndex = manifestJson.IndexOf("\"dependencies\"", System.StringComparison.Ordinal);
            if (dependenciesKeyIndex >= 0)
            {
                return manifestJson.Insert(dependenciesKeyIndex, registryEntry);
            }

            var rootEnd = manifestJson.LastIndexOf('}');
            if (rootEnd < 0)
            {
                return manifestJson;
            }

            return manifestJson.Insert(rootEnd, registryEntry);
        }

        private static string EnsureWebXRDependency(string manifestJson)
        {
            if (manifestJson.IndexOf($"\"{WebXRPackageName}\"", System.StringComparison.Ordinal) >= 0)
            {
                return manifestJson;
            }

            if (!TryFindPropertyValueRange(manifestJson, "\"dependencies\"", 0, manifestJson.Length, out var dependenciesStart, out var dependenciesEnd))
            {
                var dependenciesEntry =
                    "  \"dependencies\": {\n" +
                    $"    \"{WebXRPackageName}\": \"{WebXRPackageVersion}\"\n" +
                    "  }\n";

                var rootEnd = manifestJson.LastIndexOf('}');
                if (rootEnd < 0)
                {
                    return manifestJson;
                }

                return manifestJson.Insert(rootEnd, dependenciesEntry);
            }

            var dependencyEntry = dependenciesEnd - dependenciesStart > 2
                ? ",\n    \"" + WebXRPackageName + "\": \"" + WebXRPackageVersion + "\""
                : "\n    \"" + WebXRPackageName + "\": \"" + WebXRPackageVersion + "\"\n  ";

            return manifestJson.Insert(dependenciesEnd - 1, dependencyEntry);
        }

        private static bool TryFindPropertyValueRange(string json, string key, int searchStart, int searchEnd, out int valueStart, out int valueEnd)
        {
            valueStart = -1;
            valueEnd = -1;

            var keyIndex = json.IndexOf(key, searchStart, System.StringComparison.Ordinal);
            if (keyIndex >= searchEnd)
            {
                keyIndex = -1;
            }
            if (keyIndex < 0)
            {
                return false;
            }

            var colonIndex = json.IndexOf(':', keyIndex + key.Length);
            if (colonIndex < 0 || colonIndex >= searchEnd)
            {
                return false;
            }

            valueStart = colonIndex + 1;
            while (valueStart < searchEnd && char.IsWhiteSpace(json[valueStart]))
            {
                valueStart++;
            }

            if (valueStart >= searchEnd)
            {
                return false;
            }

            var openingChar = json[valueStart];
            var closingChar = openingChar == '{' ? '}' : openingChar == '[' ? ']' : '\0';
            if (closingChar == '\0')
            {
                return false;
            }

            var depth = 0;
            var inString = false;
            var escaping = false;

            for (var i = valueStart; i < searchEnd; i++)
            {
                var character = json[i];

                if (inString)
                {
                    if (escaping)
                    {
                        escaping = false;
                    }
                    else if (character == '\\')
                    {
                        escaping = true;
                    }
                    else if (character == '"')
                    {
                        inString = false;
                    }

                    continue;
                }

                if (character == '"')
                {
                    inString = true;
                    continue;
                }

                if (character == openingChar)
                {
                    depth++;
                }
                else if (character == closingChar)
                {
                    depth--;
                    if (depth == 0)
                    {
                        valueEnd = i + 1;
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool TryFindContainingObjectRange(string json, string searchText, out int objectStart, out int objectEnd)
        {
            objectStart = -1;
            objectEnd = -1;

            var searchIndex = json.IndexOf(searchText, System.StringComparison.Ordinal);
            if (searchIndex < 0)
            {
                return false;
            }

            for (var i = searchIndex; i >= 0; i--)
            {
                if (json[i] != '{')
                {
                    continue;
                }

                if (TryFindMatchingRange(json, i, '{', '}', out objectEnd) && objectEnd > searchIndex)
                {
                    objectStart = i;
                    return true;
                }
            }

            return false;
        }

        private static bool TryFindMatchingRange(string json, int startIndex, char openingChar, char closingChar, out int endIndex)
        {
            endIndex = -1;

            var depth = 0;
            var inString = false;
            var escaping = false;

            for (var i = startIndex; i < json.Length; i++)
            {
                var character = json[i];

                if (inString)
                {
                    if (escaping)
                    {
                        escaping = false;
                    }
                    else if (character == '\\')
                    {
                        escaping = true;
                    }
                    else if (character == '"')
                    {
                        inString = false;
                    }

                    continue;
                }

                if (character == '"')
                {
                    inString = true;
                    continue;
                }

                if (character == openingChar)
                {
                    depth++;
                }
                else if (character == closingChar)
                {
                    depth--;
                    if (depth == 0)
                    {
                        endIndex = i + 1;
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
