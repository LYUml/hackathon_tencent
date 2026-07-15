using System.IO;
using UnityEditor;
using UnityEngine;

namespace TXGame.Editor
{
    public static class TxUiIntegrationUtility
    {
        private const string RuntimeFontPath = "Assets/Resources/Fonts/simkai.ttf";
        private const string EditorFontPath = "Assets/Fonts/simkai.ttf";
        private const string UI2026Path = "Assets/Art/Sprites/UI2026";
        private const string AutoApplySessionKey = "TX_UI2026_KAITI_AUTO_APPLY_V1";

        [InitializeOnLoadMethod]
        private static void AutoApplyOnceAfterReload()
        {
            if (SessionState.GetBool(AutoApplySessionKey, false)) return;
            if (!File.Exists(RuntimeFontPath) || !File.Exists(EditorFontPath)) return;

            SessionState.SetBool(AutoApplySessionKey, true);
            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
                ApplyUi2026AndKaiTi();
            };
        }

        [MenuItem("TX/Integrate UI2026 and KaiTi Font")]
        public static void ApplyUi2026AndKaiTi()
        {
            AssetDatabase.ImportAsset(RuntimeFontPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(EditorFontPath, ImportAssetOptions.ForceUpdate);

            ConfigureUi2026Textures();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[TX] UI2026 texture integration complete. Runtime UI uses {RuntimeFontPath}; TMP text is patched at runtime.");
        }

        private static void ConfigureUi2026Textures()
        {
            if (!Directory.Exists(UI2026Path)) return;

            string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { UI2026Path });
            foreach (string guid in textureGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;

                bool changed = false;
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    changed = true;
                }

                if (importer.spriteImportMode != SpriteImportMode.Single)
                {
                    importer.spriteImportMode = SpriteImportMode.Single;
                    changed = true;
                }

                if (importer.mipmapEnabled)
                {
                    importer.mipmapEnabled = false;
                    changed = true;
                }

                if (importer.maxTextureSize != 4096)
                {
                    importer.maxTextureSize = 4096;
                    changed = true;
                }

                if (importer.filterMode != FilterMode.Bilinear)
                {
                    importer.filterMode = FilterMode.Bilinear;
                    changed = true;
                }

                bool shouldUseAlpha = Path.GetExtension(path).ToLowerInvariant() == ".png";
                if (importer.alphaIsTransparency != shouldUseAlpha)
                {
                    importer.alphaIsTransparency = shouldUseAlpha;
                    changed = true;
                }

                TextureImporterPlatformSettings standalone = importer.GetPlatformTextureSettings("Standalone");
                if (!standalone.overridden || standalone.maxTextureSize != 4096)
                {
                    standalone.overridden = true;
                    standalone.maxTextureSize = 4096;
                    importer.SetPlatformTextureSettings(standalone);
                    changed = true;
                }

                if (changed)
                    importer.SaveAndReimport();
            }
        }

    }
}
