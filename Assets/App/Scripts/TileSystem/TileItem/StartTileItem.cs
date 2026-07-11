using System.Collections;
using System.Collections.Generic;
using MagicTile.Pool;
using MagicTile.ServiceLocator;
using UnityEngine;
using UnityEngine.EventSystems;

public class StartTileItem : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        var levelManager = ServiceLocator.Get<ILevelService>();

        if (levelManager != null)
        {
            levelManager.StartLevel();

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
#if UNITY_EDITOR
        else
        {
            Debug.LogError("Not Start Level by not found LevelManager");
        }
#endif
    }
}
