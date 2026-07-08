using UnityEngine;
using UnityEngine.EventSystems;
using MagicTile.Pool;

namespace MagicTile.TileSystem
{
    /// <summary>
    /// MonoBehaviour view cho 1 tile. Cập nhật vị trí và nhận input tap.
    /// Subclasses: ShortTileView, LongTileView, ZigzagTileView, MoodTileView.
    /// MoodTileView không có hình, không di chuyển, không cần Collider2D.
    /// Các subclass khác cần Collider2D + EventSystem + Physics2DRaycaster.
    /// </summary>
    public class TileView : MonoBehaviour, IPointerDownHandler, IPoolable
    {
        protected TilePresenter Presenter { get; private set; }

        public void Initialize(TilePresenter presenter)
        {
            Presenter = presenter;
        }

        public void SetPosition(float y)
        {
            Vector3 pos = transform.position;
            pos.y = y;
            transform.position = pos;
        }

        public void SetLanePosition(float x)
        {
            Vector3 pos = transform.position;
            pos.x = x;
            transform.position = pos;
        }

        /// <summary>
        /// Cấu hình view dựa trên NoteData. Override trong subclass cho từng TileType.
        /// </summary>
        public virtual void Configure(NoteData note, float visualSpeed, float[] laneXPositions)
        {
        }

        public virtual void PlayHit() { }
        public virtual void PlayMiss() { }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        // --- IPoolable ---

        public void OnGetFromPool()
        {
            Presenter = null;
            OnGetFromPoolInternal();
        }

        public void OnReleaseToPool()
        {
            Presenter = null;
        }

        protected virtual void OnGetFromPoolInternal() { }

        // --- Input ---

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            Presenter?.OnInput();
        }

        public virtual float GetTopEdgeY()
        {
            return transform.position.y;
        }
    }
}