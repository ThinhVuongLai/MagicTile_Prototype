using DG.Tweening;
using MagicTile.Pool;
using MagicTile.ServiceLocator;
using UnityEngine;

public class EffectTouchItem : MonoBehaviour, IPoolable
{
    [SerializeField] private Transform _touchIconTransform;
    [SerializeField] private SpriteRenderer _touchIconSpriteRenderer;
    [SerializeField] private float _animationDuration = 0.4f;

    public void OnGetFromPool()
    {
        RunAnimation();
    }

    public void OnReleaseToPool()
    {
        _touchIconTransform.DOKill();
        _touchIconSpriteRenderer.DOKill();
    }

    private void RunAnimation()
    {
        _touchIconTransform.DOKill();
        _touchIconSpriteRenderer.DOKill();

        Color targetColor = _touchIconSpriteRenderer.color;
        targetColor.a = 1;
        _touchIconSpriteRenderer.color = targetColor;

        _touchIconTransform.localPosition = Vector3.zero;
        _touchIconTransform.localScale = Vector3.one;

        _touchIconTransform.DOLocalMoveY(2, 0.3f);
        _touchIconTransform.DOScale(Vector3.one * 0.7f, _animationDuration);
        _touchIconSpriteRenderer.DOFade(0, 0.3f)
            .OnComplete(FinishRunAnimation);
    }

    private void FinishRunAnimation()
    {
        var poolService = ServiceLocator.Get<PoolService>();
        if (poolService)
        {
            poolService.Release(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
