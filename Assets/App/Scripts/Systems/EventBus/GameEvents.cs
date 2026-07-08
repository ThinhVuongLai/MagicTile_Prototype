using MagicTile.TileSystem;

namespace MagicTile.Events
{
    /// <summary>
    /// Đánh giá độ chính xác dựa trên timing delta giữa thời điểm tap và note time.
    /// </summary>
    public enum HitAccuracy
    {
        Miss    = 0,   // |delta| > 0.15s hoặc tile bị miss hoàn toàn
        Good    = 1,   // |delta| <= 0.15s
        Great   = 2,   // |delta| <= 0.10s
        Perfect = 3    // |delta| <= 0.05s
    }

    /// <summary>
    /// Được publish bởi TilePresenter khi tile được tap hoặc drag-complete.
    /// ScoreLevelManager subscribe để tính điểm và combo.
    /// </summary>
    public struct TileHitEvent
    {
        public TileType TileType;
        public int Lane;
        public int BasePoints;         // 2 cho tap, bonus cho drag_complete
        public bool IsDragComplete;    // true nếu là sự kiện drag-completion
    }

    /// <summary>
    /// Được publish bởi ScoreLevelManager sau khi xử lý TileHitEvent.
    /// UI subscriber dùng để cập nhật hiển thị điểm.
    /// </summary>
    public struct ScoreChangedEvent
    {
        public int TotalScore;
        public int AddedScore;
        public float ComboMultiplier;
    }

    /// <summary>
    /// Được publish bởi ScoreLevelManager mỗi khi combo thay đổi (tăng hoặc reset).
    /// UI subscriber dùng để hiển thị combo, VFX, v.v.
    /// </summary>
    public struct ComboChangedEvent
    {
        public int CurrentCombo;
        public float ComboMultiplier;
    }

    /// <summary>
    /// Được publish bởi TilePresenter khi tile tự động hide (rơi quá màn hình) mà chưa được hit.
    /// BoardPresenter subscribe để tua level về TileTime.
    /// </summary>
    public struct LoseEvent
    {
        public float TileTime;
    }
}
