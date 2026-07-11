using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace MagicTile.Background
{
    public enum BackgroundColor
    {
        Yellow,
        Purple,
        Blue,
        Green
    }

    [Serializable]
    public class LevelBackgroundSpriteInfor
    {
        public BackgroundColor Color;
        public Sprite Sprite;
    }

    public class LevelBackgroundView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private List<LevelBackgroundSpriteInfor> _backgroundSprites;

        private readonly float _punchDuration = 0.2f;
        private readonly float _punchScaleAmount = 0.15f;

        public void SetSprite(Sprite sprite)
        {
            _spriteRenderer.sprite = sprite;
        }

        public void SetSpriteByColor(BackgroundColor color)
        {
            var info = _backgroundSprites.Find(b => b.Color == color);
            if (info != null)
            {
                _spriteRenderer.sprite = info.Sprite;
            }
            else
            {
                Debug.LogWarning($"LevelBackgroundView: No sprite found for color {color}.");
            }
        }

        public void PunchScale()
        {
            Vector3 originalScale = transform.localScale;
            Vector3 punchScale = originalScale + Vector3.one * _punchScaleAmount;

            transform.DOScale(punchScale, _punchDuration * 0.5f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    transform.DOScale(originalScale, _punchDuration * 0.5f)
                        .SetEase(Ease.InQuad);
                });
        }

        public void FitToScreen()
        {
            if (_spriteRenderer.sprite == null)
            {
                Debug.LogWarning("LevelBackgroundView: No sprite assigned to SpriteRenderer.");
                return;
            }

            float worldHeight = Camera.main.orthographicSize * 2f;
            float worldWidth = worldHeight * Camera.main.aspect;
            float spriteWidth = _spriteRenderer.sprite.bounds.size.x;
            float spriteHeight = _spriteRenderer.sprite.bounds.size.y;

            float scaleX = worldWidth / spriteWidth;
            float scaleY = worldHeight / spriteHeight;

            transform.localScale = Vector3.one * Mathf.Max(scaleX, scaleY);
            transform.position = new Vector3(0f, 0f, 5f);
        }
    }
}
