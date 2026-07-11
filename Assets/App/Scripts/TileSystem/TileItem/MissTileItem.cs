using DG.Tweening;
using MagicTile.Events;
using MagicTile.Pool;
using MagicTile.ServiceLocator;
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
        _spriteRenderer.DOFade(0.3f, 0.15f)
            .SetEase(Ease.Linear)
            .SetLoops(4, LoopType.Yoyo)
            .OnComplete(FinishRunAnimation);
    }

    public void OnReleaseToPool()
    {

    }

    private void FinishRunAnimation()
    {
        EventBus.Instance.Publish(new FinishRunMissTileEvent());

        var poolService = ServiceLocator.Get<PoolService>();
        if (poolService)
        {
            poolService.Release(this);
        }
        else
        {
            Destroy(this);
        }
    }

    public void UpdateSpriteRenderer(float height)
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
