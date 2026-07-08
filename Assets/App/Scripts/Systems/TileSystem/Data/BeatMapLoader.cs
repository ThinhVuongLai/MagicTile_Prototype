using System.IO;
using UnityEngine;

namespace MagicTile.TileSystem
{
    /// <summary>
    /// Static utility for loading a beatmap JSON file and creating a BeatMapModel.
    /// </summary>
    public static class BeatMapLoader
    {
        /// <summary>
        /// Loads a beatmap from a TextAsset (assigned in the Inspector or loaded via Resources).
        /// </summary>
        public static BeatMapModel Load(TextAsset jsonAsset)
        {
            if (jsonAsset == null)
            {
                Debug.LogError("[BeatMapLoader] TextAsset is null.");
                return null;
            }

            return Parse(jsonAsset.text);
        }

        /// <summary>
        /// Loads a beatmap from an absolute or relative file path on disk.
        /// </summary>
        public static BeatMapModel LoadFromFile(string path)
        {
            if (!File.Exists(path))
            {
                Debug.LogError($"[BeatMapLoader] File not found: {path}");
                return null;
            }

            string json = File.ReadAllText(path);
            return Parse(json);
        }

        /// <summary>
        /// Loads a beatmap from the Resources folder (without extension).
        /// e.g. LoadFromResources("Levels/Nobatidao")
        /// </summary>
        public static BeatMapModel LoadFromResources(string resourcePath)
        {
            TextAsset asset = Resources.Load<TextAsset>(resourcePath);
            if (asset == null)
            {
                Debug.LogError($"[BeatMapLoader] Resource not found: {resourcePath}");
                return null;
            }

            BeatMapModel model = Parse(asset.text);
            Resources.UnloadAsset(asset);
            return model;
        }

        private static BeatMapModel Parse(string json)
        {
            BeatMapData data = JsonUtility.FromJson<BeatMapData>(json);

            if (data == null || data.notes == null)
            {
                Debug.LogError("[BeatMapLoader] Failed to parse beatmap JSON.");
                return null;
            }

            if (string.IsNullOrEmpty(data.format) || data.format != "MT3")
            {
                Debug.LogWarning($"[BeatMapLoader] Unexpected format: '{data.format}'. Expected 'MT3'.");
            }

            return new BeatMapModel(data);
        }
    }
}
