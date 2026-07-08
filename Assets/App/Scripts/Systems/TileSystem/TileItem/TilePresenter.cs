using MagicTile.Events;
using UnityEngine;

namespace MagicTile.TileSystem
{
    /// <summary>
    /// Presenter liên kết TileModel và TileView. Chứa logic per-tile:
    /// tính vị trí, kiểm tra miss, hit, và trigger mood action.
    /// Plain C# class — không kế thừa MonoBehaviour.
    /// </summary>
    public class TilePresenter
    {
        private readonly TileModel _model;
        private readonly TileView _view;
        private readonly float _visualSpeed;
        private readonly float _missThreshold;
        private readonly float _hitThreshold;
        private readonly float _hitLineY;

        public TileModel Model => _model;
        public TileView View => _view;
        public bool ShouldRemove => _model.IsHidden;

        public TilePresenter(TileModel model, TileView view, float visualSpeed,
                             float missThreshold, float hitThreshold, float hitLineY)
        {
            _model = model;
            _view = view;
            _visualSpeed = visualSpeed;
            _missThreshold = missThreshold;
            _hitThreshold = hitThreshold;
            _hitLineY = hitLineY;
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
            float position = (_model.Time - currentTime) * _visualSpeed * GlobalData.Instance.ScaleUnitForMoveTile;
            _model.Position = position;
            _view.SetPosition(_hitLineY + position);

            // Auto-hide khi cạnh trên của tile nằm dưới cạnh dưới màn hình
            float screenBottomY = Camera.main.transform.position.y - Camera.main.orthographicSize;
            if (_view.GetTopEdgeY() < screenBottomY - GlobalData.Instance.AutoHideOffsetBelowScreen)
            {
                if (!_model.IsHit && !_model.IsMissed)
                {
                    _model.SetMissed();
                    _model.SetState(TileState.Missed);
                    EventBus.Instance.Publish(new LoseEvent { TileTime = _model.Time });
                }
                _model.MarkHidden();
                return;
            }

            // Kiểm tra miss
            // if (!_model.IsHit && !_model.IsMissed && position < -_missThreshold)
            // {
            //     _model.SetMissed();
            //     _model.SetState(TileState.Missed);
            //     _view.PlayMiss();
            //     _model.MarkHidden();
            //     return;
            // }

            // // Kiểm tra completion cho long/zigzag
            // if (_model.IsHit && _model.Type != TileType.Short)
            // {
            //     float endTime = _model.Time + _model.Duration;
            //     if (currentTime >= endTime)
            //     {
            //         _model.SetState(TileState.Completed);
            //         _model.MarkHidden();
            //     }
            // }
        }

        public void OnInput()
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
                TileType       = _model.Type,
                Lane           = _model.Lane,
                BasePoints     = 2,
                IsDragComplete = false
            });

            (_view as IHitStrategy)?.Hit(_model);
        }

        public void OnDragComplete()
        {
            if (!_model.IsHit || _model.IsHidden) return;

            int bonus = Mathf.FloorToInt(_model.Duration / 0.2f);

            EventBus.Instance.Publish(new TileHitEvent
            {
                TileType       = _model.Type,
                Lane           = _model.Lane,
                BasePoints     = bonus,
                IsDragComplete = true
            });

            _model.SetDragCompleted();
            _model.SetState(TileState.Completed);
            _model.MarkHidden();
        }
    }
}