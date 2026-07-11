using MagicTile.ServiceLocator;
using Unity.VisualScripting;
using UnityEngine;

namespace MagicTile.TileSystem
{
    public class LongTileView : TileView, IHitStrategy, IDragStrategy
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;

        [Header("Fill")]
        [SerializeField] private SpriteRenderer _fillRenderer;

        [Header("Intter Line")]
        [SerializeField] private Transform _beginLine;
        [SerializeField] private SpriteRenderer _line;

        private BoxCollider2D _boxCollider;

        private bool _isCompleteDrag = false;
        private float _maxFill = 0;
        private bool _isFinishFill = false;

        private TileConfig _tileConfig;

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

            _tileConfig = ServiceLocator.ServiceLocator.Get<ConfigManager>()?.TileConfig;

            var configManager = ServiceLocator.ServiceLocator.Get<ConfigManager>();
            float scaleUnit = 1;
            if (configManager)
            {
                scaleUnit = configManager.GlobalData == null ? 1 : configManager.GlobalData.ScaleUnitForMoveTile;
            }

            float length = note.duration * visualSpeed * scaleUnit;
            if (_spriteRenderer != null)
            {
                Vector2 size = _spriteRenderer.size;
                size.y = length;
                _spriteRenderer.size = size;

                var pos = _spriteRenderer.transform.localPosition;
                pos.y = size.y * 0.5f;
                _spriteRenderer.transform.localPosition = pos;
            }

            _maxFill = _spriteRenderer.size.y - _tileConfig.LongTile.OffsetYForMaxFill;

            UpdateInnerLine();

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

        private void UpdateInnerLine()
        {
            float tileHeight = _spriteRenderer.size.y;

            float beginLineParentLocalPosY = _beginLine.transform.parent.localPosition.y;

            float targetHeight = tileHeight - beginLineParentLocalPosY - _tileConfig.LongTile.InnerLineOffsetWithTop;

            float beginLineLocalPosY = _beginLine.transform.localPosition.y;

            float lineHeight = targetHeight - beginLineLocalPosY;

            Vector2 targetSize = _line.size;
            targetSize.x = lineHeight;
            _line.size = targetSize;

            Vector3 targetLocalPosition = _line.transform.localPosition;
            targetLocalPosition.x = lineHeight / 2f;
            _line.transform.localPosition = targetLocalPosition;
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

            float thresholdY = GetTopEdgeY() + _tileConfig.LongTile.DragCompletionOffset;

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

            float fillSizeY = localPos.y + _tileConfig.LongTile.OffsetFromTouch;

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
    }
}
