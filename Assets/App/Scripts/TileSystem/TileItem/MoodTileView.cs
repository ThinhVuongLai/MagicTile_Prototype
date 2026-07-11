using UnityEngine;

namespace MagicTile.TileSystem
{
    /// <summary>
    /// Tile view cho "mood" notes — không có hình ảnh, không di chuyển.
    /// Khi currentTime >= note.time, chạy Trigger() để invoke Action.
    /// GameObject có thể là rỗng (không cần SpriteRenderer/Collider2D).
    /// </summary>
    public class MoodTileView : TileView
    {
        private bool _triggered;

        /// <summary>Called khi mood note đến đúng thời điểm. Truyền metas của note.</summary>
        public event System.Action<MetaData[]> OnTrigger;

        protected override void OnGetFromPoolInternal()
        {
            _triggered = false;
        }

        public override void Configure(NoteData note, float visualSpeed, float[] laneXPositions)
        {
            _triggered = false;
        }

        /// <summary>
        /// Gọi mỗi frame bởi TilePresenter. Khi time match thì trigger 1 lần.
        /// </summary>
        public void TryTrigger(float currentTime, float noteTime, MetaData[] metas)
        {
            if (_triggered) return;

            if (currentTime >= noteTime)
            {
                _triggered = true;
                OnTrigger?.Invoke(metas);
            }
        }
    }
}