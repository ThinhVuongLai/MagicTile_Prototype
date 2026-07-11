using UnityEngine;

namespace MagicTile.TileSystem
{
    [CreateAssetMenu(fileName = "LevelRunTimeConfig", menuName = "MagicTile/LevelRunTimeConfig")]
    public class LevelRunTimeConfig : ScriptableObject
    {
        [Header("Timing & Thresholds")]
        [SerializeField] private float _spawnLeadTime = 5f;
        [SerializeField] private float _missThreshold = 0f;
        [SerializeField] private float _hitThreshold = 0f;

        [Header("Layout")]
        [SerializeField] private float _hitLineY = -4f;
        [SerializeField] private float[] _laneXPositions = { -4.5f, -1.5f, 1.5f, 4.5f };

        [Header("Seek")]
        [SerializeField] private float _seekDuration = 1f;

        public float SpawnLeadTime => _spawnLeadTime;
        public float MissThreshold => _missThreshold;
        public float HitThreshold => _hitThreshold;
        public float HitLineY => _hitLineY;
        public float[] LaneXPositions => _laneXPositions;
        public float SeekDuration => _seekDuration;
    }
}
