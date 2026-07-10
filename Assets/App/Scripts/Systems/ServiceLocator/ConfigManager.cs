using UnityEngine;

namespace MagicTile.ServiceLocator
{
    public class ConfigManager : MonoBehaviour
    {
        [field: SerializeField] public TileSystem.TileConfig TileConfig { get; private set; }
        [field: SerializeField] public TileSystem.LevelRunTimeConfig LevelRunTimeConfig { get; private set; }
        [field: SerializeField] public Background.LevelBackgroundConfig LevelBackgroundConfig { get; private set; }
        [field: SerializeField] public TileSystem.LevelConfig LevelConfig { get; private set; }
        [field: SerializeField] public TileSystem.ScoreMilestoneItemConfig ScoreMilestoneItemConfig { get; private set; }

        private void Awake()
        {
            ServiceLocator.Register(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<ConfigManager>();
        }
    }
}
