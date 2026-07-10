using MagicTile.Background;
using MagicTile.TileSystem;
using UnityEngine;

namespace MagicTile.ServiceLocator
{
    public class LevelManager : MonoBehaviour, ILevelService
    {
        [SerializeField] private Transform _backgroundContainer;
        [SerializeField] private BoardPresenter _boardPresenter;

        private LevelConfig _levelConfig;
        private LevelBackgroundConfig _backgroundConfig;

        private LevelConfigInfor _currentLevelConfig;
        private LevelStatus _currentStatus = LevelStatus.None;
        private LevelBackgroundPresenter _backgroundPresenter;

        private bool _isInitConfig = false;

        public LevelConfigInfor CurrentLevelConfig => _currentLevelConfig;

        private void Awake()
        {
            ServiceLocator.Register<ILevelService>(this);
        }

        private void InitConfig()
        {
            if (_isInitConfig)
                return;

            _isInitConfig = true;

            var configManager = ServiceLocator.Get<ConfigManager>();

            _levelConfig = configManager?.LevelConfig;
            _backgroundConfig = configManager?.LevelBackgroundConfig;
        }

        public void StartLevel(int levelIndex)
        {
            InitConfig();

            if (_boardPresenter == null)
            {
                Debug.LogError("[LevelManager] BoardPresenter chưa được gán.");
                return;
            }

            if (_levelConfig != null)
                SetCurrentLevel(levelIndex);

            SpawnBackground(_currentLevelConfig.levelIndex);

            _currentStatus = LevelStatus.Start;
            _boardPresenter.StartGame();
        }

        public void SetLevelStatus(LevelStatus status) => _currentStatus = status;

        public bool IsStatus(LevelStatus status) => _currentStatus == status;

        public void ResetLevel()
        {
            _boardPresenter?.ResetLevel();
        }

        private void SpawnBackground(int backgroundIndex)
        {
            var info = _backgroundConfig?.GetByIndex(backgroundIndex);
            if (info?.View == null)
            {
                Debug.LogWarning($"[LevelManager] No background found with index {backgroundIndex}");
                return;
            }

            _backgroundPresenter?.Dispose();
            var view = Object.Instantiate(info.View, _backgroundContainer);
            view.transform.localPosition = Vector3.zero;
            view.FitToScreen();
            _backgroundPresenter = new LevelBackgroundPresenter(view);
        }

        private void SetCurrentLevel(int levelIndex)
        {
            _currentLevelConfig = _levelConfig?.GetLevelByIndex(levelIndex);
            if (_currentLevelConfig == null)
                Debug.LogWarning($"[LevelManager] Level index {levelIndex} not found.");
        }
    }
}
