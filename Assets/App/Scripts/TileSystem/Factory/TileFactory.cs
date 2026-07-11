using MagicTile.Pool;
using MagicTile.ServiceLocator;

namespace MagicTile.TileSystem
{
    public class TileFactory : ITileFactory
    {
        private readonly PoolService _poolService;
        private readonly TileConfig _tileConfig;

        public TileFactory()
        {
            _poolService = ServiceLocator.ServiceLocator.Get<PoolService>();
            _tileConfig = ServiceLocator.ServiceLocator.Get<ConfigManager>().TileConfig;
        }

        public TileView Create(TileType type)
        {
            var prefab = _tileConfig.GetPrefabByType(type);
            return prefab != null ? _poolService.Get(prefab) : null;
        }
    }
}