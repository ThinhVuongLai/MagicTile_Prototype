using MagicTile.TileSystem;

namespace MagicTile.ServiceLocator
{
    public enum LevelStatus
    {
        None = -1,
        Start,
        Pause,
        Lose,
        Complete
    }

    public interface ILevelService
    {
        LevelConfigInfor CurrentLevelConfig { get; }
        void StartLevel(int levelIndex);
        void SetLevelStatus(LevelStatus status);
        bool IsStatus(LevelStatus status);
        void ResetLevel();
        void SetSlowLevel(float duration = 5f);
    }
}
