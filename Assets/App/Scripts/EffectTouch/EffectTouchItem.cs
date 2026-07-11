using DG.Tweening;
using MagicTile.Pool;
using MagicTile.ServiceLocator;
using UnityEngine;

public class EffectTouchItem : MonoBehaviour, IPoolable
{
    [SerializeField] private Transform _touchIconTransform;
    [SerializeField] private SpriteRenderer _touchIconSpriteRenderer;
    [SerializeField] private Transform _touchVfxTransform;
    [SerializeField] private SpriteRenderer _touchVfxSpriteRenderer;
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

        _touchIconTransform.DOKill();
        _touchVfxSpriteRenderer.DOKill();

        Color targetColor = _touchIconSpriteRenderer.color;
        targetColor.a = 1;
        _touchIconSpriteRenderer.color = targetColor;

        _touchIconTransform.localPosition = Vector3.zero + new Vector3(0, -0.5f, 0);
        _touchIconTransform.localScale = Vector3.one;

        targetColor = _touchVfxSpriteRenderer.color;
        targetColor.a = 1;
        _touchVfxSpriteRenderer.color = targetColor;

        _touchVfxTransform.localScale = Vector3.one * 0.5f;

        Sequence mySequence = DOTween.Sequence();

        mySequence.Join(_touchIconTransform.DOLocalMoveY(2, 0.4f));
        mySequence.Join(_touchIconTransform.DOScale(Vector3.one * 0.7f, 0.5f));
        mySequence.Join(_touchIconSpriteRenderer.DOFade(0, 0.5f));
        mySequence.Join(_touchVfxTransform.DOScale(Vector3.one * 1.2f, 0.25f));
        mySequence.Join(_touchVfxSpriteRenderer.DOFade(0, 0.27f));

        mySequence.OnComplete(FinishRunAnimation);
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
