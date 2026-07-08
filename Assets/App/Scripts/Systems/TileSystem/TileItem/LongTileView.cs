using UnityEngine;
using UnityEngine.EventSystems;

namespace MagicTile.TileSystem
{
    public class LongTileView : TileView, IHitStrategy, IPointerUpHandler
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

        [SerializeField] private bool _isPointerHolding;
        private BoxCollider2D _boxCollider;

        protected override void OnGetFromPoolInternal()
        {
            _isPointerHolding = false;
            _boxCollider = GetComponent<BoxCollider2D>();
            if (_fillRenderer != null)
            {
                _fillRenderer.size = new Vector2(_fillRenderer.size.x, 0f);
            }
        }

        public override void Configure(NoteData note, float visualSpeed, float[] laneXPositions)
        {
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

        public override void OnPointerDown(PointerEventData eventData)
        {
            _isPointerHolding = true;

            Debug.LogError("Is Long Hit");

            SetFillSize(Camera.main.ScreenToWorldPoint(eventData.position));

            base.OnPointerDown(eventData);
        }

        private void Update()
        {
            if (!_isPointerHolding) return;
            if (Mathf.Approximately(Time.timeScale, 0f)) return;

            Vector3 fingerWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            SetFillSize(fingerWorldPos);

            float thresholdY = GetTopCenterY() + _dragCompletionOffset;

            if (fingerWorldPos.y >= thresholdY)
            {
                _isPointerHolding = false;

                Debug.LogError("Is Long Complete");

                Presenter?.OnDragComplete();
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isPointerHolding = false;
        }

        private float GetTopCenterY()
        {
            if (_boxCollider != null)
                return _boxCollider.bounds.max.y;
            return transform.position.y;
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
            if (_fillRenderer == null) return;

            Vector3 localPos = transform.InverseTransformPoint(fingerWorldPos);

            float fillSizeY = localPos.y + _offsetFromTouch /* - _startFillY */;

            float maxFill = _spriteRenderer.size.y - _offsetYForMaxFill;

            fillSizeY = Mathf.Clamp(fillSizeY, 0, maxFill);

            if (fillSizeY < _fillRenderer.size.y) return;

            var size = _fillRenderer.size;
            size.y = fillSizeY;
            _fillRenderer.size = size;
        }

        public override float GetTopEdgeY()
        {
            if (_boxCollider != null)
                return _boxCollider.bounds.max.y;
            return transform.position.y;
        }
    }
}
