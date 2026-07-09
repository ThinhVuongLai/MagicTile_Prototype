using MagicTile.TileSystem;
using UnityEngine;

namespace MagicTile.ServiceLocator
{
    public class LevelManager : ILevelService
    {
        private readonly BoardPresenter _boardPresenter;
        private readonly LevelConfig _levelConfig;
        private LevelConfigInfor _currentLevelConfig;
        private LevelStatus _currentStatus = LevelStatus.None;

        public LevelConfigInfor CurrentLevelConfig => _currentLevelConfig;

        public LevelManager(BoardPresenter boardPresenter, LevelConfig levelConfig)
        {
            _boardPresenter = boardPresenter;
            _levelConfig = levelConfig;
        }

        public void StartLevel(int levelIndex)
        {
            if (_boardPresenter == null)
            {
                Debug.LogError("[LevelManager] BoardPresenter chưa được gán.");
                return;
            }

            if (_levelConfig != null)
                SetCurrentLevel(levelIndex);

            _currentStatus = LevelStatus.Start;
            _boardPresenter.StartGame();
        }

        public void SetLevelStatus(LevelStatus status) => _currentStatus = status;

        public bool IsStatus(LevelStatus status) => _currentStatus == status;

        private void SetCurrentLevel(int levelIndex)
        {
            _currentLevelConfig = _levelConfig?.GetLevelByIndex(levelIndex);
            if (_currentLevelConfig == null)
                Debug.LogWarning($"[LevelManager] Level index {levelIndex} not found.");
        }
    }
}
