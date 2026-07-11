using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using MagicTile.Audio;
using MagicTile.Events;

namespace MagicTile.Booster
{
    public class BoosterPresenter
    {
        private readonly BoosterModel _model;
        private AudioService _audioService;
        private CancellationTokenSource _slowCts;
        private float _slowOriginalPitch;
        private int _slowGeneration;

        private AudioService AudioService
        {
            get
            {
                if (_audioService == null)
                {
                    _audioService = ServiceLocator.ServiceLocator.Get<AudioService>();
                }

                return _audioService;
            }
        }

        public BoosterPresenter(BoosterModel model)
        {
            _model = model;
        }

        public void RunBooster(BoosterType type)
        {
            switch (type)
            {
                case BoosterType.Slow:
                    if (_model.SlowBoosterCount > 0)
                    {
                        _model.SlowBoosterCount--;
                        CancelSlowBooster();
                        _slowCts = new CancellationTokenSource();
                        int gen = ++_slowGeneration;
                        if (gen == 1) _slowOriginalPitch = AudioService.Pitch;
                        AudioService.Pitch = 0.5f;
                        EventBus.Instance.Publish(new SlowStartEvent());
                        RunSlowBooster(_slowCts.Token, gen).Forget();
                    }
                    else
                    {
                        EventBus.Instance.Publish(new SlowEndEvent());
                    }
                    break;
            }
        }

        private async UniTaskVoid RunSlowBooster(CancellationToken token, int generation, float duration = 5f)
        {
            try
            {
                float elapsed = 0f;
                while (elapsed < duration)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update, token);

                    elapsed += Time.deltaTime;
                    float remainingPercent = 1f - (elapsed / duration);
                    float countdown = duration - elapsed;

                    EventBus.Instance.Publish(new BoosterEvent
                    {
                        Type = BoosterType.Slow,
                        RemainingPercent = remainingPercent,
                        Countdown = countdown
                    });
                }
            }
            finally
            {
                if (generation == _slowGeneration)
                {
                    AudioService.Pitch = _slowOriginalPitch;
                    _slowGeneration = 0;

                    EventBus.Instance.Publish(new BoosterEvent
                    {
                        Type = BoosterType.Slow,
                        RemainingPercent = 0f,
                        Countdown = 0f
                    });
                    EventBus.Instance.Publish(new SlowEndEvent());
                }
            }
        }

        public void CancelSlowBooster()
        {
            if (_slowCts != null)
            {
                _slowCts.Cancel();
                _slowCts.Dispose();
                _slowCts = null;
            }
        }
    }
}
