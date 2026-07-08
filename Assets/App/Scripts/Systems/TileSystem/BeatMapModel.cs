namespace MagicTile.TileSystem
{
    /// <summary>
    /// Runtime model holding all parsed note data and song metadata for the current level.
    /// Read-only after construction.
    /// </summary>
    public class BeatMapModel
    {
        private readonly NoteData[] _notes;

        public SongMeta SongMeta { get; }
        public int NoteCount => _notes.Length;
        public float VisualSpeed => SongMeta.visualSpeed;
        public int LaneCount => SongMeta.nLanes;
        public float Bpm => SongMeta.bpm;
        public float AudioDuration => SongMeta.audioDuration;

        public BeatMapModel(BeatMapData data)
        {
            _notes = data.notes ?? System.Array.Empty<NoteData>();
            SongMeta = data.songMeta;
        }

        /// <summary>
        /// Returns the note at the given index. Notes are sorted by time.
        /// </summary>
        public NoteData GetNote(int index)
        {
            return _notes[index];
        }
    }
}
