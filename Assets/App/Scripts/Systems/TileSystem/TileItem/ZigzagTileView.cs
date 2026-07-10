using UnityEngine;
using System.Collections.Generic;

namespace MagicTile.TileSystem
{
    [RequireComponent(typeof(NoteZigzagMesh))]
    public class ZigzagTileView : TileView, IHitStrategy, IDragStrategy
    {
        [Header("Drag Completion")]
        [Tooltip("Offset from the top-center of the tile. 0 = top edge, 1 = 1 unit above.")]
        [SerializeField] private float _dragCompletionOffset = 0f;

        [Header("Check Hit")]
        [SerializeField] private SpriteRenderer _checkHitSpriteRenderer;

        [Header("Fill")]
        [SerializeField] private NoteZigzagMesh _fillMesh;
        [SerializeField] private float _offsetYForMaxFill;
        [SerializeField] private float _offsetFromTouch;

        private static readonly int RevealYID = Shader.PropertyToID("_RevealY");

        private NoteZigzagMesh _segment;
        private float _currentRevealY;
        private MaterialPropertyBlock _fillPropertyBlock;
        private MeshRenderer _fillMeshRenderer;
        private PolygonCollider2D _polygonCollider;

        private float _maxY = 0;

        private bool _isCompleteDrag = false;
        private bool _finishFill = false;

        protected override void OnGetFromPoolInternal()
        {
            _segment = GetComponent<NoteZigzagMesh>();
            _currentRevealY = 0f;
            _polygonCollider = GetComponent<PolygonCollider2D>();

            if (_fillMesh != null && _fillMeshRenderer == null)
                _fillMeshRenderer = _fillMesh.GetComponent<MeshRenderer>();
            if (_fillPropertyBlock == null)
                _fillPropertyBlock = new MaterialPropertyBlock();

            if (_fillMeshRenderer != null)
            {
                _fillPropertyBlock.SetFloat(RevealYID, 0f);
                _fillMeshRenderer.SetPropertyBlock(_fillPropertyBlock);
            }
        }

        public override void Configure(NoteData note, float visualSpeed, float[] laneXPositions)
        {
            _isCompleteDrag = false;
            _finishFill = false;

            if (_segment == null)
                _segment = GetComponent<NoteZigzagMesh>();

            float startX = laneXPositions[note.lane - 1];

            List<Vector3> points = new List<Vector3>();
            points.Add(Vector3.zero);

            if (note.controls != null)
            {
                foreach (ControlPoint cp in note.controls)
                {
                    float x = laneXPositions[cp.lane - 1] - startX;
                    float y = (cp.time - note.time) * visualSpeed * GlobalData.Instance.ScaleUnitForMoveTile;

                    _maxY = y;

                    points.Add(new Vector3(x, y, 0f));
                }
            }
            _segment.SetPoints(points);
            if (_fillMesh != null)
            {
                _fillMesh.SetPoints(points);

                if (_fillMeshRenderer == null)
                    _fillMeshRenderer = _fillMesh.GetComponent<MeshRenderer>();
                if (_fillPropertyBlock == null)
                    _fillPropertyBlock = new MaterialPropertyBlock();

                _currentRevealY = 0f;
                _fillPropertyBlock.SetFloat(RevealYID, 0f);
                _fillMeshRenderer.SetPropertyBlock(_fillPropertyBlock);
            }

            UpdateCheckHitSpriteRenderer();
            _polygonCollider.enabled = false;
        }

        public void Hit(TileModel model)
        {
            model.SetState(TileState.Swiping);
            PlayHit();

            _polygonCollider.enabled = true;
            AdjustColliderToMesh();
        }

        public override Bounds GetSpriteBounds()
        {
            if (_checkHitSpriteRenderer != null) return _checkHitSpriteRenderer.bounds;
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

            float thresholdY = GetTopCenterY() + _dragCompletionOffset;

            if (worldPos.y >= thresholdY)
            {
                _isCompleteDrag = true;

                Presenter?.OnDragComplete();
            }
        }

        public void EndDrag()
        {
            if (_finishFill && !_isCompleteDrag)
            {
                _isCompleteDrag = true;

                Presenter?.OnDragComplete();
            }
        }

        private float GetTopCenterY()
        {
            if (_polygonCollider != null)
                return _polygonCollider.bounds.max.y;
            return transform.position.y;
        }

        private void SetFillSize(Vector3 fingerWorldPos)
        {
            if (_fillMesh == null || _fillMeshRenderer == null && _finishFill) return;

            Vector3 localPos = transform.InverseTransformPoint(fingerWorldPos);

            float revealY = localPos.y + _offsetFromTouch;

            float maxFill = _fillMesh.GetLastPointPositionY() - _offsetYForMaxFill;

            revealY = Mathf.Clamp(revealY, 0, maxFill);

            if (revealY <= _currentRevealY)
            {
                return;
            }

            if (revealY >= maxFill)
            {
                _finishFill = true;
            }

            _currentRevealY = revealY;
            _fillPropertyBlock.SetFloat(RevealYID, revealY);
            _fillMeshRenderer.SetPropertyBlock(_fillPropertyBlock);
        }

        private void AdjustColliderToMesh()
        {
            if (_polygonCollider == null || _segment == null) return;

            MeshFilter mf = _segment.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null) return;

            Mesh mesh = mf.sharedMesh;
            Vector3[] vertices = mesh.vertices;
            int vertCount = vertices.Length;
            int halfCount = vertCount / 2;

            _polygonCollider.pathCount = 1;
            Vector2[] path = new Vector2[vertCount];

            for (int i = 0; i < halfCount; i++)
            {
                int srcIndex = (halfCount - 1 - i) * 2;
                path[i] = new Vector2(vertices[srcIndex].x, vertices[srcIndex].y);
            }

            for (int i = 0; i < halfCount; i++)
            {
                int srcIndex = i * 2 + 1;
                path[halfCount + i] = new Vector2(vertices[srcIndex].x, vertices[srcIndex].y);
            }

            _polygonCollider.SetPath(0, path);
        }

        public override float GetTopEdgeY()
        {
            return transform.position.y + _maxY;
        }

        private void UpdateCheckHitSpriteRenderer()
        {
            if (_checkHitSpriteRenderer == null)
                return;

            float targetHeight = _segment.GetPointPositionY(1) / 2f;

            Vector2 targetSize = _checkHitSpriteRenderer.size;
            targetSize.y = targetHeight;
            _checkHitSpriteRenderer.size = targetSize;

            Vector3 targetLocalPosition = _checkHitSpriteRenderer.transform.localPosition;
            targetLocalPosition.y = targetHeight / 2f;
            _checkHitSpriteRenderer.transform.localPosition = targetLocalPosition;
        }
    }
}
