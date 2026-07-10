using System;
using System.Collections.Generic;
using UnityEngine;

namespace MagicTile.TileSystem
{
    [Serializable]
    public class TilePrefabInfor
    {
        public TileType Type;
        public TileView Prefab;
    }

    [CreateAssetMenu(fileName = "TileConfig", menuName = "MagicTile/TileConfig")]
    public class TileConfig : ScriptableObject
    {
        [SerializeField] private List<TilePrefabInfor> _tilePrefabs;
        [SerializeField] private MissTileItem _missPrefab;

        public MissTileItem MissPrefab => _missPrefab;

        public TileView GetPrefabByType(TileType type)
        {
            foreach (var info in _tilePrefabs)
            {
                if (info.Type == type)
                    return info.Prefab;
            }
            Debug.LogError($"[TileConfig] No prefab found for type {type}");
            return null;
        }
    }
}
