using UnityEngine;
using MagicTile.TileSystem;
using MagicTile.Booster;

namespace MagicTile.ServiceLocator
{
    public class GameManager : MonoBehaviour
    {
        private void Awake()
        {
            Init();

            Application.targetFrameRate = 60;
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
            var uiManager = new UIManager();
            ServiceLocator.Register<IUiService>(uiManager);

            var boosterManager = new BoosterManager();
            ServiceLocator.Register<IBoosterService>(boosterManager);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<ILevelService>();
            ServiceLocator.Unregister<IUiService>();
        }
    }
}
