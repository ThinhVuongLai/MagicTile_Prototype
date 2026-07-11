using MagicTile.Events;
using MagicTile.ServiceLocator;
using UnityEngine;

namespace MagicTile.TileSystem
{
    public class TilePresenter
    {
        private const float TOUCH_BOUND_SIZE = 0.4f;

        private readonly TileModel _model;
        private readonly TileView _view;
        private readonly float _visualSpeed;
        private readonly float _missThreshold;
        private readonly float _hitThreshold;
        private readonly float _hitLineY;

        public TileModel Model => _model;
        public TileView View => _view;
        public bool ShouldRemove => _model.IsHidden;

        private float scaleUnit = 1;

        public TilePresenter(TileModel model, TileView view, float visualSpeed,
                             float missThreshold, float hitThreshold, float hitLineY)
        {
            _model = model;
            _view = view;
            _visualSpeed = visualSpeed;
            _missThreshold = missThreshold;
            _hitThreshold = hitThreshold;
            _hitLineY = hitLineY;

            var configManager = ServiceLocator.ServiceLocator.Get<ConfigManager>();
            if (configManager)
            {
                scaleUnit = configManager.GlobalData == null ? 1 : configManager.GlobalData.ScaleUnitForMoveTile;
            }

        }

        public bool CheckHit(Vector2 worldPos)
        {
            if (_model.IsHit || _model.IsMissed || _model.IsHidden) return false;
            if (_model.Type == TileType.Mood) return false;

            Bounds touchBounds = new Bounds(worldPos, new Vector3(TOUCH_BOUND_SIZE, TOUCH_BOUND_SIZE, 1f));
            Bounds spriteBounds = _view.GetSpriteBounds();

            if (!Intersects2D(touchBounds, spriteBounds)) return false;

            OnInput(worldPos);

            return true;
        }

        public bool Intersects2D(SpriteRenderer source, SpriteRenderer target)
        {
            if (source == null || target == null) return false;

            // Lấy Bounds 3D gốc của 2 Sprite
            Bounds b1 = source.bounds;
            Bounds b2 = target.bounds;

            // Thuật toán AABB 2D: Chỉ so sánh biên của trục X và trục Y
            bool overlapX = b1.min.x <= b2.max.x && b1.max.x >= b2.min.x;
            bool overlapY = b1.min.y <= b2.max.y && b1.max.y >= b2.min.y;

            // Cả 2 trục cùng đè lên nhau thì mới tính là Overlap
            return overlapX && overlapY;
        }

        public bool Intersects2D(Bounds customBounds, SpriteRenderer targetSprite)
        {
            if (targetSprite == null) return false;

            // Lấy Bounds 3D của SpriteRenderer
            Bounds spriteBounds = targetSprite.bounds;

            // Thuật toán AABB 2D: Chỉ so sánh biên của trục X và trục Y
            bool overlapX = customBounds.min.x <= spriteBounds.max.x && customBounds.max.x >= spriteBounds.min.x;
            bool overlapY = customBounds.min.y <= spriteBounds.max.y && customBounds.max.y >= spriteBounds.min.y;

            // Cả 2 trục cùng thỏa mãn thì là đè lên nhau
            return overlapX && overlapY;
        }

        public bool Intersects2D(Bounds customBounds, Bounds targetBounds)
        {
            if (targetBounds == null) return false;

            // Thuật toán AABB 2D: Chỉ so sánh biên của trục X và trục Y
            bool overlapX = customBounds.min.x <= targetBounds.max.x && customBounds.max.x >= targetBounds.min.x;
            bool overlapY = customBounds.min.y <= targetBounds.max.y && customBounds.max.y >= targetBounds.min.y;

            // Cả 2 trục cùng thỏa mãn thì là đè lên nhau
            return overlapX && overlapY;
        }

        public void Tick(float currentTime)
        {
            // Mood notes: không di chuyển, trigger Action khi time match.
            if (_model.Type == TileType.Mood)
            {
                if (_view is MoodTileView moodView)
                    moodView.TryTrigger(currentTime, _model.Time, _model.Metas);

                if (currentTime >= _model.Time)
                    _model.MarkHidden();
                return;
            }

            // Tính vị trí: (noteTime - currentTime) * visualSpeed
            SetPositionByTime(currentTime);

            // Miss detection: tile rơi quá màn hình mà chưa hit
            float screenBottomY = Camera.main.transform.position.y - Camera.main.orthographicSize;

            float autoHideOffsetBelowScreen = 0;
            var configManager = ServiceLocator.ServiceLocator.Get<ConfigManager>();
            if (configManager)
            {
                autoHideOffsetBelowScreen = configManager.GlobalData == null ? 1 : configManager.GlobalData.AutoHideOffsetBelowScreen;
            }

            if (_view.GetTopEdgeY() < screenBottomY - autoHideOffsetBelowScreen)
            {
                if (!_model.IsHit && !_model.IsMissed)
                {
                    _model.SetMissed();
                    _model.SetState(TileState.Missed);
                    EventBus.Instance.Publish(new LoseEvent { TileTime = _model.Time, NoteIndex = _model.NoteIndex });
                }
                else
                {
                    _model.MarkHidden();
                }
                return;
            }
        }

        public void SetPositionByTime(float currentTime)
        {
            float position = (_model.Time - currentTime) * _visualSpeed * scaleUnit;
            _model.Position = position;
            _view.SetPosition(_hitLineY + position);
        }

        public void AddPositionY(float addValue)
        {
            _view.AddPositionY(addValue);
        }

        public void OnInput(Vector2 touchPos)
        {
            if (_model.IsHit || _model.IsMissed || _model.IsHidden)
                return;

            if (_model.Type == TileType.Mood)
                return;

            float pos = _model.Position;
            // if (Mathf.Abs(pos) > _hitThreshold)
            //     return;

            _model.SetHit();

            EventBus.Instance.Publish(new TileHitEvent
            {
                TileType = _model.Type,
                Lane = _model.Lane,
                BasePoints = 2,
                IsDragComplete = false,
                Accuracy = CalculateAccuracy(),
                TouchPosition = touchPos
            });

            (_view as IHitStrategy)?.Hit(_model);
        }

        public void OnDragComplete()
        {
            if (!_model.IsHit || _model.IsHidden) return;

            int bonus = Mathf.FloorToInt(_model.Duration / 0.2f);

            EventBus.Instance.Publish(new TileHitEvent
            {
                TileType = _model.Type,
                Lane = _model.Lane,
                BasePoints = bonus,
                IsDragComplete = true,
                Accuracy = CalculateAccuracy()
            });

            _model.SetDragCompleted();
            _model.SetState(TileState.Completed);
            _model.MarkHidden();
        }

        private HitAccuracy CalculateAccuracy()
        {
            Camera cam = Camera.main;
            float screenTop = cam.transform.position.y + cam.orthographicSize;
            float screenHeight = cam.orthographicSize * 2f;
            float tileWorldY = _hitLineY + _model.Position;
            float distanceFromTop = screenTop - tileWorldY;
            float percentFromTop = distanceFromTop / screenHeight;

            if (percentFromTop <= 0.75f) return HitAccuracy.Perfect;
            if (percentFromTop <= 0.90f) return HitAccuracy.Great;
            if (percentFromTop <= 1.0f) return HitAccuracy.Good;
            return HitAccuracy.Miss;
        }

        public float GetTopEdgeWorldPositionY()
        {
            return _view == null ? 0 : _view.GetTopEdgeY();
        }

        public float GetTileHeight()
        {
            return _view == null ? 0 : _view.GetHeight();
        }
    }
}