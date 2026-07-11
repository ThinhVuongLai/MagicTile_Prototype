using UnityEngine;
namespace MagicTile.GlobalData
{
    [CreateAssetMenu(fileName = "GlobalData", menuName = "MagicTile/GlobalData")]
    public class GlobalData : ScriptableObject
    {
        [SerializeField] private float _scaleUnitForMoveTile = 1f;
        [SerializeField] private float _autoHideOffsetBelowScreen = 2f;
        [SerializeField] private float _loopRestartDelay = 2f;
        [SerializeField] private EffectTouchItem _effectTouchItem;

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

        public EffectTouchItem EffectTouchItem
        {
            get => _effectTouchItem;
        }
    }
}

