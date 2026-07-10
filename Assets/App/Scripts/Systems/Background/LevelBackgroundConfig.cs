using System;
using System.Collections.Generic;
using UnityEngine;

namespace MagicTile.Background
{
    [Serializable]
    public class LevelBackgroundInfor
    {
        public int Index;
        public LevelBackgroundView View;
    }

    [CreateAssetMenu(fileName = "LevelBackgroundConfig", menuName = "MagicTile/LevelBackgroundConfig")]
    public class LevelBackgroundConfig : ScriptableObject
    {
        [SerializeField] private List<LevelBackgroundInfor> _backgroundInfors;

        public LevelBackgroundInfor GetByIndex(int index)
        {
            foreach (var info in _backgroundInfors)
            {
                if (info.Index == index)
                    return info;
            }
            Debug.LogError($"[LevelBackgroundConfig] No background found for index {index}");
            return null;
        }
    }
}
