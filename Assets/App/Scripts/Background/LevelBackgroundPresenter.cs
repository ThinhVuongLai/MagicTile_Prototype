using System;
using MagicTile.Events;
using MagicTile.TileSystem;
using UnityEngine;

namespace MagicTile.Background
{
    public class LevelBackgroundPresenter : IDisposable
    {
        private readonly LevelBackgroundView _view;

        public LevelBackgroundPresenter(LevelBackgroundView view)
        {
            _view = view;
            EventBus.Instance.Subscribe<MoodTriggeredEvent>(OnMoodTriggered);
        }

        ~LevelBackgroundPresenter()
        {
            if (_view != null)
            {
                UnityEngine.Object.Destroy(_view.gameObject);
            }

            if (EventBus.HasInstance)
                EventBus.Instance.Unsubscribe<MoodTriggeredEvent>(OnMoodTriggered);
        }

        public void Dispose()
        {
            if (EventBus.HasInstance)
                EventBus.Instance.Unsubscribe<MoodTriggeredEvent>(OnMoodTriggered);
        }

        private void OnMoodTriggered(MoodTriggeredEvent e)
        {
            if (e.Metas == null) return;

            foreach (var meta in e.Metas)
            {
                if (meta.key == "bg_color")
                {
                    if (Enum.TryParse(meta.value, true, out BackgroundColor color))
                    {
                        _view.SetSpriteByColor(color);
                        _view.PunchScale();
                    }
                    else
                    {
                        Debug.LogWarning($"[LevelBackgroundPresenter] Unknown bg_color value: {meta.value}");
                    }
                    break;
                }
            }
        }
    }
}
