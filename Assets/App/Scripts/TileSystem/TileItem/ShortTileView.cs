using MagicTile.ServiceLocator;
using UnityEngine;

namespace MagicTile.TileSystem
{
    /// <summary>
    /// Tile view cho "short" notes. Chiều dài cố định.
    /// Hit: state → Hit, play feedback, mark hidden ngay lập tức.
    /// </summary>
    public class ShortTileView : TileView, IHitStrategy
    {
        [SerializeField] private BoxCollider2D _boxCollider;
        [SerializeField] private SpriteRenderer _spriteRenderer;

        public override void Configure(NoteData note, float visualSpeed, float[] laneXPositions)
        {
            if (_spriteRenderer != null)
            {
                Vector2 size = _spriteRenderer.size;

                size.y = 1;
                var configManager = ServiceLocator.ServiceLocator.Get<ConfigManager>();
                if (configManager)
                {
                    size.y = configManager.GlobalData == null ? 1 : configManager.GlobalData.ScaleUnitForMoveTile;
                }

                _spriteRenderer.size = size;

                var pos = _spriteRenderer.transform.localPosition;
                pos.y = size.y * 0.5f;
                _spriteRenderer.transform.localPosition = pos;
            }
            AdjustColliderToSprite();
        }

        public void Hit(TileModel model)
        {
            model.SetState(TileState.Hit);
            PlayHit();
            model.MarkHidden();
        }

        public override Bounds GetSpriteBounds()
        {
            if (_spriteRenderer != null) return _spriteRenderer.bounds;
            return base.GetSpriteBounds();
        }

        public override float GetTopEdgeY()
        {
            return transform.position.y + _spriteRenderer.size.y;
        }

        public override float GetHeight()
        {
            if (_spriteRenderer != null) return _spriteRenderer.size.y;
            return base.GetHeight();
        }


        private void AdjustColliderToSprite()
        {
            if (_boxCollider != null && _spriteRenderer != null)
            {
                _boxCollider.size = _spriteRenderer.size;
                _boxCollider.offset = _spriteRenderer.transform.localPosition;
            }
        }
    }
}
