using UnityEditor;
using UnityEngine;

namespace TXGame.Editor
{
    public static class TxTextureImportFixer
    {
        [MenuItem("TX/Fix Background Texture Quality")]
        public static void FixBackgroundTextureQuality()
        {
            string[] texturePaths =
            {
                "Assets/Art/Sprites/Backgrounds/剧院外景_intro.png",
                "Assets/Art/Sprites/Backgrounds/剧院外景_intro_sharp.png",
                "Assets/Art/Sprites/Backgrounds/剧院外景_intro_4k.png",
                "Assets/Art/Sprites/Backgrounds/戏曲道具与盔头陈列室.png",
                "Assets/Art/Sprites/Backgrounds/剧社侧台门帘处.png",
                "Assets/Art/Sprites/Backgrounds/排练室.png",
                "Assets/Art/Sprites/Backgrounds/戏曲舞台.png",
                "Assets/Art/Sprites/Backgrounds/上座观演雅座.png",
                "Assets/Art/Sprites/Backgrounds/舞台与幕后.png",
                "Assets/Art/Sprites/Backgrounds/老旧大戏箱堆放角.png",
                "Assets/Art/Sprites/Overlays/fallen_performer_floor_overlay.png"
            };

            foreach (string path in texturePaths)
                FixTexture(path);

            AssetDatabase.SaveAssets();
            Debug.Log("Background texture quality fixed.");
        }

        private static void FixTexture(string path)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.maxTextureSize = 4096;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.filterMode = FilterMode.Bilinear;
            importer.anisoLevel = 1;
            importer.alphaIsTransparency = false;

            TextureImporterPlatformSettings standalone = importer.GetPlatformTextureSettings("Standalone");
            standalone.overridden = true;
            standalone.maxTextureSize = 4096;
            standalone.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SetPlatformTextureSettings(standalone);

            importer.SaveAndReimport();
        }
    }
}
