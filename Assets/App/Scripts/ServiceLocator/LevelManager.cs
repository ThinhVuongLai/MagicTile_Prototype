using MagicTile.Background;
using MagicTile.Pool;
using MagicTile.TileSystem;
using UnityEngine;

namespace MagicTile.ServiceLocator
{
    public class LevelManager : MonoBehaviour, ILevelService
    {
        [SerializeField] private Transform _backgroundContainer;
        [SerializeField] private Transform _levelLineContainer;
        [SerializeField] private BoardPresenter _boardPresenter;

        private LevelConfig _levelConfig;
        private LevelBackgroundConfig _backgroundConfig;

        private LevelConfigInfor _currentLevelConfig;
        private LevelStatus _currentStatus = LevelStatus.None;

        private bool _isInitConfig = false;

        private StartTileItem _spawnedStartTile;
        private ReplayTileItem _spawnedReplayTile;

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

        public void InitLevel(int levelIndex)
        {
            InitConfig();

            if (_levelConfig != null)
                SetCurrentLevel(levelIndex);

            var configManager = ServiceLocator.Get<ConfigManager>();
            var tileConfig = configManager?.TileConfig;
            var poolService = ServiceLocator.Get<PoolService>();
            var runTimeConfig = configManager?.LevelRunTimeConfig;

            if (tileConfig == null || poolService == null || runTimeConfig == null)
            {
                Debug.LogError("[LevelManager] Thiếu TileConfig, PoolService hoặc LevelRunTimeConfig.");
                return;
            }

            if (tileConfig.StartTileItem != null)
            {
                _spawnedStartTile = poolService.Get(tileConfig.StartTileItem);
                var laneX = runTimeConfig.LaneXPositions[1];
                var screenBottom = Camera.main.transform.position.y - Camera.main.orthographicSize;
                _spawnedStartTile.transform.position = new Vector3(laneX, screenBottom + 3f, 0f);
            }

            _boardPresenter.InitLevel(_backgroundContainer, _levelLineContainer);
        }

        public void StartLevel()
        {
            if (_boardPresenter == null)
            {
                Debug.LogError("[LevelManager] BoardPresenter chưa được gán.");
                return;
            }

            _currentStatus = LevelStatus.Start;
            _boardPresenter.StartGame();
        }

        public void SetLevelStatus(LevelStatus status) => _currentStatus = status;

        public bool IsStatus(LevelStatus status) => _currentStatus == status;

        public void ReplayLevel()
        {
            _boardPresenter?.ResetLevel();
        }

        private void SetCurrentLevel(int levelIndex)
        {
            _currentLevelConfig = _levelConfig?.GetLevelByIndex(levelIndex);
            if (_currentLevelConfig == null)
                Debug.LogWarning($"[LevelManager] Level index {levelIndex} not found.");
        }
    }
}
