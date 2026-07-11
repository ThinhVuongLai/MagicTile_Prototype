using UnityEngine;
using MagicTile.Pool;

namespace MagicTile.TileSystem
{
    public class TileView : MonoBehaviour, IPoolable
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

        public void AddPositionY(float deltaY)
        {
            Vector3 pos = transform.position;
            pos.y += deltaY;
            transform.position = pos;
        }

        public virtual void Configure(NoteData note, float visualSpeed, float[] laneXPositions)
        {
        }

        public virtual void PlayHit() { }
        public virtual void PlayMiss() { }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public virtual Bounds GetSpriteBounds()
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) return sr.bounds;
            var col = GetComponent<Collider2D>();
            if (col != null) return col.bounds;
            return new Bounds(transform.position, Vector3.zero);
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

        public virtual float GetTopEdgeY()
        {
            return transform.position.y;
        }

        public virtual float GetHeight()
        {
            return 0;
        }
    }
}