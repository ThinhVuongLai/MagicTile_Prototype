using MagicTile.Pool;

namespace MagicTile.TileSystem
{
    /// <summary>
    /// Factory Method — tạo TileView theo TileType từ PoolService.
    /// Chọn prefab tương ứng: Short, Long, Zigzag, Mood.
    /// </summary>
    public class TileFactory : ITileFactory
    {
        private readonly PoolService _poolService;
        private readonly ShortTileView _shortPrefab;
        private readonly LongTileView _longPrefab;
        private readonly ZigzagTileView _zigzagPrefab;
        private readonly MoodTileView _moodPrefab;

        public TileFactory(PoolService poolService,
                           ShortTileView shortPrefab,
                           LongTileView longPrefab,
                           ZigzagTileView zigzagPrefab,
                           MoodTileView moodPrefab)
        {
            _poolService = poolService;
            _shortPrefab = shortPrefab;
            _longPrefab = longPrefab;
            _zigzagPrefab = zigzagPrefab;
            _moodPrefab = moodPrefab;
        }

        public TileView Create(TileType type)
        {
            return type switch
            {
                TileType.Short  => _poolService.Get(_shortPrefab),
                TileType.Long   => _poolService.Get(_longPrefab),
                TileType.Zigzag => _poolService.Get(_zigzagPrefab),
                TileType.Mood   => _poolService.Get(_moodPrefab),
                _               => _poolService.Get(_shortPrefab)
            };
        }
    }
}