using UnityEngine;

public class GlobalData : SingletonMono<GlobalData>
{
    [SerializeField] private float _scaleUnitForMoveTile = 1f;
    [SerializeField] private float _autoHideOffsetBelowScreen = 2f;
    [SerializeField] private float _loopRestartDelay = 2f;

    public float ScaleUnitForMoveTile
    {
        get => _scaleUnitForMoveTile;
    }

    public float AutoHideOffsetBelowScreen
    {
        get => _autoHideOffsetBelowScreen;
    }

    public float LoopRestartDelay
    {
        get => _loopRestartDelay;
    }
}
