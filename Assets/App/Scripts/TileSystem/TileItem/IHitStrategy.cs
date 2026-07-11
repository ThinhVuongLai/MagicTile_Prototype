namespace MagicTile.TileSystem
{
    /// <summary>
    /// Strategy Pattern — mỗi TileView có thể input implement interface này.
    /// Presenter gọi Hit() thay vì switch theo TileType.
    /// </summary>
    public interface IHitStrategy
    {
        void Hit(TileModel model);
    }
}
