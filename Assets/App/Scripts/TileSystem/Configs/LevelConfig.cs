using System;
using System.Collections.Generic;
using MagicTile.UI.Menu;
using UnityEngine;

namespace MagicTile.TileSystem
{
    [Serializable]
    public class LevelConfigInfor
    {
        public int levelIndex;
        public string jsonPath;
        public int backgroundIndex;
        public float countdownDuration = 3f;
        public List<MilestoneData> milestones;
    }

    [CreateAssetMenu(fileName = "LevelConfig", menuName = "MagicTile/LevelConfig")]
    public class LevelConfig : ScriptableObject
    {
        [field: SerializeField] public TextAsset DefaultJson { get; private set; }

        [SerializeField] private List<LevelConfigInfor> _levels;

        public LevelConfigInfor GetLevelByIndex(int index)
        {
            return _levels?.Find(l => l.levelIndex == index);
        }
    }
}
