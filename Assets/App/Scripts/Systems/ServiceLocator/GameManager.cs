using UnityEngine;
using MagicTile.TileSystem;

namespace MagicTile.ServiceLocator
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private BoardPresenter _boardPresenter;

        private void Awake()
        {
            Init();
        }

        private void Start()
        {
            StartCoroutine(CRStartLevel());
        }

        private System.Collections.IEnumerator CRStartLevel()
        {
            yield return null;

            var levelSevice = ServiceLocator.Get<ILevelService>();

            if (levelSevice != null)
            {
                levelSevice.StartLevel(0);
            }
        }

        public void Init()
        {
            var configManager = ServiceLocator.Get<ConfigManager>();
            var levelManager = new LevelManager(_boardPresenter, configManager.LevelConfig);
            ServiceLocator.Register<ILevelService>(levelManager);

            var uiManager = new UIManager();
            ServiceLocator.Register<IUiService>(uiManager);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<ILevelService>();
            ServiceLocator.Unregister<IUiService>();
        }
    }
}
