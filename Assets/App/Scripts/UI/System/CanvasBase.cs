using UnityEngine;

namespace MagicTile.UI
{
    public abstract class CanvasBase : MonoBehaviour, ICanvasView
    {
        [SerializeField] private UIType _uiType;

        public UIType UIType => _uiType;
        public bool IsActive { get; set; }

        public abstract ICanvasPresenter FirstSpawn();
    }
}
