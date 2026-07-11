using UnityEngine;

namespace MagicTile.TileSystem
{
    public interface IDragStrategy
    {
        void StartDrag(Vector2 worldPos);
        void UpdateDrag(Vector2 worldPos);
        void EndDrag();
    }
}
