namespace MagicTile.TileSystem
{
    /// <summary>
    /// Factory Method interface — chỉ tạo TileView theo TileType.
    /// Release là trách nhiệm của Pool, không thuộc Factory.
    /// </summary>
    public interface ITileFactory
    {
        /// <summary>Tạo hoặc lấy TileView từ pool theo TileType.</summary>
        TileView Create(TileType type);
    }
}