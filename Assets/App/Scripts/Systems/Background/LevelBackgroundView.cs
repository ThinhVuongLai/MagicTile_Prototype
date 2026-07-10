using System;
using System.Collections.Generic;
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
