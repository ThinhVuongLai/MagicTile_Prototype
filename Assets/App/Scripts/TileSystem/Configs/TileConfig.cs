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

    [Serializable]
    public class LongTileSettings
    {
        [SerializeField] private float _dragCompletionOffset = 0f;
        [SerializeField] private float _startFillY = 0f;
        [SerializeField] private float _offsetYForMaxFill = 0f;
        [SerializeField] private float _offsetFromTouch = 0f;
        [SerializeField] private float _innerLineOffsetWithTop = 1.25f;

        public float DragCompletionOffset => _dragCompletionOffset;
        public float StartFillY => _startFillY;
        public float OffsetYForMaxFill => _offsetYForMaxFill;
        public float OffsetFromTouch => _offsetFromTouch;
        public float InnerLineOffsetWithTop => _innerLineOffsetWithTop;
    }

    [Serializable]
    public class ZigzagTileSettings
    {
        [SerializeField] private float _dragCompletionOffset = 0f;
        [SerializeField] private float _offsetYForMaxFill = 0f;
        [SerializeField] private float _offsetFromTouch = 0f;

        public float DragCompletionOffset => _dragCompletionOffset;
        public float OffsetYForMaxFill => _offsetYForMaxFill;
        public float OffsetFromTouch => _offsetFromTouch;
    }

    [CreateAssetMenu(fileName = "TileConfig", menuName = "MagicTile/TileConfig")]
    public class TileConfig : ScriptableObject
    {
        [Header("Tile Prefabs")]
        [SerializeField] private List<TilePrefabInfor> _tilePrefabs;
        [SerializeField] private MissTileItem _missPrefab;
        [SerializeField] private StartTileItem _startTileItem;
        [SerializeField] private ReplayTileItem _replayTileItem;

        [Header("Long Tile")]
        [SerializeField] private LongTileSettings _longTileSettings;

        [Header("Zigzag Tile")]
        [SerializeField] private ZigzagTileSettings _zigzagTileSettings;

        public MissTileItem MissPrefab => _missPrefab;
        public StartTileItem StartTileItem => _startTileItem;
        public ReplayTileItem ReplayTileItem => _replayTileItem;

        public LongTileSettings LongTile => _longTileSettings;
        public ZigzagTileSettings ZigzagTile => _zigzagTileSettings;

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
