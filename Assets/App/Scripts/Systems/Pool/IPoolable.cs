namespace MagicTile.Pool
{
    /// <summary>
    /// Interface for objects that need reset logic when retrieved from or returned to a pool.
    /// Implement on any Component that participates in object pooling.
    /// </summary>
    public interface IPoolable
    {
        /// <summary>Called right after the object is activated from the pool.</summary>
        void OnGetFromPool();

        /// <summary>Called right before the object is deactivated and returned to the pool.</summary>
        void OnReleaseToPool();
    }
}
