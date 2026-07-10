using UnityEngine;

namespace MagicTile.TileSystem
{
    public class LongTileView : TileView, IHitStrategy, IDragStrategy
    {
        [Header("Drag Completion")]
        [Tooltip("Offset from the top-center of the tile. 0 = top edge, 1 = 1 unit above.")]
        [SerializeField] private float _dragCompletionOffset = 0f;

        [Header("Fill")]
        [SerializeField] private SpriteRenderer _fillRenderer;
        [SerializeField] private float _startFillY;
        [SerializeField] private float _offsetYForMaxFill;
        [SerializeField] private float _offsetFromTouch;

        [SerializeField] private SpriteRenderer _spriteRenderer;

        private BoxCollider2D _boxCollider;

        private bool _isCompleteDrag = false;
        private float _maxFill = 0;
        private bool _isFinishFill = false;

        protected override void OnGetFromPoolInternal()
        {
            _boxCollider = GetComponent<BoxCollider2D>();
            if (_fillRenderer != null)
            {
                _fillRenderer.size = new Vector2(_fillRenderer.size.x, 0f);
            }
        }

        public override void Configure(NoteData note, float visualSpeed, float[] laneXPositions)
        {
            _isCompleteDrag = false;
            _isFinishFill = false;

            float length = note.duration * visualSpeed * GlobalData.Instance.ScaleUnitForMoveTile;
            if (_spriteRenderer != null)
            {
                Vector2 size = _spriteRenderer.size;
                size.y = length;
                _spriteRenderer.size = size;

                var pos = _spriteRenderer.transform.localPosition;
                pos.y = size.y * 0.5f;
                _spriteRenderer.transform.localPosition = pos;
            }

            _maxFill = _spriteRenderer.size.y - _offsetYForMaxFill;

            if (_fillRenderer)
            {
                _fillRenderer.size = new Vector2(_fillRenderer.size.x, 0);
            }

            AdjustColliderToSprite();
        }

        public void Hit(TileModel model)
        {
            model.SetState(TileState.Holding);
            PlayHit();
        }

        public override Bounds GetSpriteBounds()
        {
            if (_spriteRenderer != null) return _spriteRenderer.bounds;
            if (_boxCollider != null) return _boxCollider.bounds;
            return base.GetSpriteBounds();
        }

        // --- IDragStrategy ---

        public void StartDrag(Vector2 worldPos)
        {
            SetFillSize(worldPos);
        }

        public void UpdateDrag(Vector2 worldPos)
        {
            if (_isCompleteDrag)
                return;

            if (Mathf.Approximately(Time.timeScale, 0f)) return;

            SetFillSize(worldPos);

            float thresholdY = GetTopEdgeY() + _dragCompletionOffset;

            if (worldPos.y >= thresholdY)
            {
                _isCompleteDrag = true;
                Presenter?.OnDragComplete();
            }
        }

        public void EndDrag()
        {
            if (_isFinishFill && !_isCompleteDrag)
            {
                _isCompleteDrag = true;
                Presenter?.OnDragComplete();
            }
        }

        private void AdjustColliderToSprite()
        {
            if (_boxCollider != null && _spriteRenderer != null)
            {
                _boxCollider.size = _spriteRenderer.size;
                _boxCollider.offset = _spriteRenderer.transform.localPosition;
            }
        }

        private void SetFillSize(Vector3 fingerWorldPos)
        {
            if (_fillRenderer == null || _isFinishFill) return;

            Vector3 localPos = transform.InverseTransformPoint(fingerWorldPos);

            float fillSizeY = localPos.y + _offsetFromTouch;

            fillSizeY = Mathf.Clamp(fillSizeY, 0, _maxFill);

            if (fillSizeY < _fillRenderer.size.y) return;

            if (fillSizeY >= _maxFill)
            {
                _isFinishFill = true;
            }

            var size = _fillRenderer.size;
            size.y = fillSizeY;
            _fillRenderer.size = size;
        }

        public override float GetTopEdgeY()
        {
            return transform.position.y + _spriteRenderer.size.y;
        }
    }
}
