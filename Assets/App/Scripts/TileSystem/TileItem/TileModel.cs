namespace MagicTile.TileSystem
{
    public enum TileType
    {
        Short,
        Long,
        Zigzag,
        Mood
    }

    public enum TileState
    {
        Falling,    // Normal state, moving toward the hit line
        Holding,    // Long note — player is holding
        Swiping,    // Zigzag note — player tapped, swipe phase
        Hit,        // Successfully hit (short notes)
        Missed,     // Passed the miss threshold without being hit
        Completed   // Long/zigzag fully completed
    }

    /// <summary>
    /// Runtime state for a single active tile. Created by BoardPresenter from a NoteData.
    /// Updated by TilePresenter; read by TileView through the presenter.
    /// </summary>
    public class TileModel
    {
        // --- Static data (from NoteData, set at construction) ---

        public int NoteIndex { get; }
        public int Lane { get; }
        public float Time { get; }              // hit time in seconds
        public TileType Type { get; }
        public float Duration { get; }           // long/zigzag only
        public ControlPoint[] Controls { get; }  // long/zigzag end points
        public MetaData[] Metas { get; }

        // --- Runtime state (updated by TilePresenter) ---

        public bool IsHit { get; private set; }
        public bool IsMissed { get; private set; }
        public bool IsHidden { get; private set; }   // marked for removal
        public bool IsDragCompleted { get; private set; }
        public float Position { get; set; }           // Y offset from hit line
        public TileState State { get; private set; }

        public TileModel(NoteData note, int noteIndex)
        {
            NoteIndex = noteIndex;
            Lane = note.lane;
            Time = note.time;
            Type = ParseType(note.type);
            Duration = note.duration;
            Controls = note.controls ?? System.Array.Empty<ControlPoint>();
            Metas = note.metas ?? System.Array.Empty<MetaData>();

            IsHit = false;
            IsMissed = false;
            IsHidden = false;
            IsDragCompleted = false;
            Position = 0f;
            State = TileState.Falling;
        }

        // --- State mutators (called by TilePresenter) ---

        public void SetHit()
        {
            IsHit = true;
        }

        public void SetMissed()
        {
            IsMissed = true;
        }

        public void SetState(TileState state)
        {
            State = state;
        }

        public void MarkHidden()
        {
            IsHidden = true;
        }

        public void SetDragCompleted()
        {
            IsDragCompleted = true;
        }

        private static TileType ParseType(string typeString)
        {
            return typeString switch
            {
                "short"  => TileType.Short,
                "long"   => TileType.Long,
                "zigzag" => TileType.Zigzag,
                "mood"   => TileType.Mood,
                _        => TileType.Short
            };
        }
    }
}
