using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MagicTile.Pool;
using UnityEngine;

public class MissTileItem : MonoBehaviour, IPoolable
{
    [SerializeField] private SpriteRenderer _spriteRenderer = null;

    public void OnGetFromPool()
    {
        if (_spriteRenderer == null)
        {
            return;
        }

        _spriteRenderer.DOKill();
        _spriteRenderer.DOFade(0.3f, 0.15f).SetEase(Ease.Linear).SetLoops(4, LoopType.Yoyo);
    }

    public void OnReleaseToPool()
    {

    }

    public void UpdatespriteRenderer(float height)
    {
        if (_spriteRenderer == null)
        {
            return;
        }

        Vector2 targetSize = _spriteRenderer.size;
        targetSize.y = height;
        _spriteRenderer.size = targetSize;

        Vector3 localPositionTarget = _spriteRenderer.transform.localPosition;
        localPositionTarget.y = height / 2f;
        _spriteRenderer.transform.localPosition = localPositionTarget;
    }
}
