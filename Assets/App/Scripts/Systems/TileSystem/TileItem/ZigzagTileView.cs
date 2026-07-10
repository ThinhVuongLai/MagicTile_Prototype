using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using MagicTile.ServiceLocator;

namespace MagicTile.TileSystem
{
    [RequireComponent(typeof(NoteZigzagMesh))]
    public class ZigzagTileView : TileView, IHitStrategy, IPointerUpHandler
    {
        [Header("Drag Completion")]
        [Tooltip("Offset from the top-center of the tile. 0 = top edge, 1 = 1 unit above.")]
        [SerializeField] private float _dragCompletionOffset = 0f;

        [Header("Fill")]
        [SerializeField] private NoteZigzagMesh _fillMesh;
        [SerializeField] private float _offsetYForMaxFill;
        [SerializeField] private float _offsetFromTouch;

        private static readonly int RevealYID = Shader.PropertyToID("_RevealY");

        private NoteZigzagMesh _segment;
        private bool _isPointerHolding;
        private float _currentRevealY;
        private MaterialPropertyBlock _fillPropertyBlock;
        private MeshRenderer _fillMeshRenderer;
        private PolygonCollider2D _polygonCollider;

        private float _maxY = 0;

        protected override void OnGetFromPoolInternal()
        {
            _segment = GetComponent<NoteZigzagMesh>();
            _isPointerHolding = false;
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
            AdjustColliderToMesh();
        }

        public void Hit(TileModel model)
        {
            model.SetState(TileState.Swiping);
            PlayHit();
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            var levelService = ServiceLocator.ServiceLocator.Get<ILevelService>();
            if (levelService == null || !levelService.IsStatus(LevelStatus.Start))
                return;

            _isPointerHolding = true;
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
                Presenter?.OnDragComplete();
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isPointerHolding = false;
        }

        private float GetTopCenterY()
        {
            if (_polygonCollider != null)
                return _polygonCollider.bounds.max.y;
            return transform.position.y;
        }

        private void SetFillSize(Vector3 fingerWorldPos)
        {
            if (_fillMesh == null || _fillMeshRenderer == null) return;

            Vector3 localPos = transform.InverseTransformPoint(fingerWorldPos);

            float revealY = localPos.y + _offsetFromTouch;

            float maxFill = _fillMesh.points[_fillMesh.points.Count - 1].y - _offsetYForMaxFill;

            revealY = Mathf.Clamp(revealY, 0, maxFill);

            if (revealY <= _currentRevealY) return;

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
    }
}
