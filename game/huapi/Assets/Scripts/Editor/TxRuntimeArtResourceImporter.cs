using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace TXGame.Editor
{
    public sealed class TxRuntimeArtResourceImporter : IPreprocessBuildWithReport
    {
        private const string SourceRoot = "Assets/Art/Sprites";
        private const string RuntimeRoot = "Assets/Resources/Art/Sprites";

        public int callbackOrder => -1000;

        [MenuItem("TX/Sync Runtime Art Resource Import Settings")]
        public static void SyncRuntimeArtResourceImportSettings()
        {
            if (!Directory.Exists(RuntimeRoot)) return;

            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { RuntimeRoot });
            foreach (string guid in guids)
            {
                string runtimePath = AssetDatabase.GUIDToAssetPath(guid);
                string sourcePath = SourceRoot + runtimePath.Substring(RuntimeRoot.Length);
                CopyTextureImportSettings(sourcePath, runtimePath);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[TX] Runtime art resource import settings synced.");
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            SyncRuntimeArtResourceImportSettings();
        }

        private static void CopyTextureImportSettings(string sourcePath, string runtimePath)
        {
            TextureImporter source = AssetImporter.GetAtPath(sourcePath) as TextureImporter;
            TextureImporter runtime = AssetImporter.GetAtPath(runtimePath) as TextureImporter;
            if (source == null || runtime == null) return;

            bool changed = false;
            if (runtime.textureType != source.textureType) { runtime.textureType = source.textureType; changed = true; }
            if (runtime.spriteImportMode != source.spriteImportMode) { runtime.spriteImportMode = source.spriteImportMode; changed = true; }
            if (runtime.mipmapEnabled != source.mipmapEnabled) { runtime.mipmapEnabled = source.mipmapEnabled; changed = true; }
            if (runtime.maxTextureSize != source.maxTextureSize) { runtime.maxTextureSize = source.maxTextureSize; changed = true; }
            if (runtime.textureCompression != source.textureCompression) { runtime.textureCompression = source.textureCompression; changed = true; }
            if (runtime.compressionQuality != source.compressionQuality) { runtime.compressionQuality = source.compressionQuality; changed = true; }
            if (runtime.filterMode != source.filterMode) { runtime.filterMode = source.filterMode; changed = true; }
            if (runtime.anisoLevel != source.anisoLevel) { runtime.anisoLevel = source.anisoLevel; changed = true; }
            if (runtime.alphaIsTransparency != source.alphaIsTransparency) { runtime.alphaIsTransparency = source.alphaIsTransparency; changed = true; }
            if (runtime.wrapMode != source.wrapMode) { runtime.wrapMode = source.wrapMode; changed = true; }

            TextureImporterPlatformSettings sourceDefault = source.GetDefaultPlatformTextureSettings();
            TextureImporterPlatformSettings runtimeDefault = runtime.GetDefaultPlatformTextureSettings();
            if (!SamePlatformSettings(sourceDefault, runtimeDefault))
            {
                runtime.SetPlatformTextureSettings(sourceDefault);
                changed = true;
            }

            TextureImporterPlatformSettings sourceStandalone = source.GetPlatformTextureSettings("Standalone");
            TextureImporterPlatformSettings runtimeStandalone = runtime.GetPlatformTextureSettings("Standalone");
            if (!SamePlatformSettings(sourceStandalone, runtimeStandalone))
            {
                runtime.SetPlatformTextureSettings(sourceStandalone);
                changed = true;
            }

            TextureImporterPlatformSettings sourceWebGL = source.GetPlatformTextureSettings("WebGL");
            if (sourceWebGL.overridden)
            {
                TextureImporterPlatformSettings runtimeWebGL = runtime.GetPlatformTextureSettings("WebGL");
                if (!SamePlatformSettings(sourceWebGL, runtimeWebGL))
                {
                    runtime.SetPlatformTextureSettings(sourceWebGL);
                    changed = true;
                }
            }

            if (changed)
                runtime.SaveAndReimport();
        }

        private static bool SamePlatformSettings(TextureImporterPlatformSettings a, TextureImporterPlatformSettings b)
        {
            return a.overridden == b.overridden
                && a.maxTextureSize == b.maxTextureSize
                && a.textureCompression == b.textureCompression
                && a.compressionQuality == b.compressionQuality
                && a.format == b.format
                && a.crunchedCompression == b.crunchedCompression;
        }
    }
}
