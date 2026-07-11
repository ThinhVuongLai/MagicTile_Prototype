using System;
using DG.Tweening;
using MagicTile.Events;
using UnityEngine;

namespace MagicTile.CameraEffect
{
    public class CameraShakePresenter : IDisposable
    {
        private readonly Camera _camera;
        private readonly Vector3 _originalPosition;

        private readonly float _shakeDuration = 0.3f;
        private readonly float _shakeStrength = 0.5f;
        private readonly int _shakeVibrato = 10;

        public CameraShakePresenter(Camera camera)
        {
            _camera = camera;
            _originalPosition = _camera.transform.localPosition;

            EventBus.Instance.Subscribe<LoseEvent>(OnLose);
        }

        public void Dispose()
        {
            if (EventBus.HasInstance)
            {
                EventBus.Instance.Unsubscribe<LoseEvent>(OnLose);
            }
        }

        private void OnLose(LoseEvent e)
        {
            if (_camera == null) return;
            _camera.transform.DOShakePosition(_shakeDuration, _shakeStrength, _shakeVibrato)
                .OnComplete(() => _camera.transform.localPosition = _originalPosition);
        }
    }
}
