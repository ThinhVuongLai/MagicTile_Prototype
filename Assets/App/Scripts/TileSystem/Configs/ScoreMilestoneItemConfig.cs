using System.Collections.Generic;
using MagicTile.UI.Menu;
using UnityEngine;

namespace MagicTile.TileSystem
{
    [CreateAssetMenu(fileName = "ScoreMilestoneItemConfig", menuName = "MagicTile/ScoreMilestoneItemConfig")]
    public class ScoreMilestoneItemConfig : ScriptableObject
    {
        [field: SerializeField] public List<LevelScoreMilestonePrefabData> MilestonePrefabs { get; private set; }
    }
}
