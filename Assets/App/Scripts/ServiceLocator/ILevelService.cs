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
        void InitLevel(int levelIndex);
        void StartLevel();
        void SetLevelStatus(LevelStatus status);
        bool IsStatus(LevelStatus status);
        void ReplayLevel();
    }
}
