using System;

namespace MagicTile.TileSystem
{
    /// <summary>
    /// Root DTO for the MT3 beatmap JSON format.
    /// </summary>
    [Serializable]
    public class BeatMapData
    {
        public NoteData[] notes;
        public SongMeta songMeta;
        public string format;
    }

    /// <summary>
    /// A single note in the beatmap. Maps directly to the JSON note object.
    /// </summary>
    [Serializable]
    public class NoteData
    {
        public int lane;            // 1-based lane index (1..nLanes)
        public float time;          // hit time in seconds
        public string type;         // "short" | "long" | "mood" | "zigzag"
        public MetaData[] metas;    // key-value metadata (bg_color, shadow, etc.)
        public ControlPoint[] controls;  // end/release points for long & zigzag
        public float duration;      // duration in seconds (long & zigzag only)
    }

    /// <summary>
    /// Key-value metadata attached to a note (e.g. bg_color, shadow).
    /// </summary>
    [Serializable]
    public class MetaData
    {
        public string key;
        public string value;
    }

    /// <summary>
    /// A control point defining the end of a long or zigzag note.
    /// </summary>
    [Serializable]
    public class ControlPoint
    {
        public int lane;
        public float time;
    }

    /// <summary>
    /// Song-level metadata from the beatmap.
    /// </summary>
    [Serializable]
    public class SongMeta
    {
        public float bpm;
        public int nLanes;
        public float visualSpeed;
        public float audioDuration;
    }
}
