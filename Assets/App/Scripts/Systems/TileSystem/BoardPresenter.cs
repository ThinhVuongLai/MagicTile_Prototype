using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MagicTile.Events;
using MagicTile.Pool;
using MagicTile.ScoreSystem;

namespace MagicTile.TileSystem
{
    /// <summary>
    /// Điều khiển chính của tile system: spawn tiles theo thời gian, Tick tất cả
    /// TilePresenter mỗi frame, cleanup tiles đã hide. Dùng TileFactory (Create)
    /// và PoolService (Release) riêng biệt.
    /// </summary>
    public class BoardPresenter : MonoBehaviour
    {
        [Header("Beatmap")]
        [SerializeField] private TextAsset _beatMapJson;

        [Header("Tile Prefabs")]
        [SerializeField] private ShortTileView _shortPrefab;
        [SerializeField] private LongTileView _longPrefab;
        [SerializeField] private ZigzagTileView _zigzagPrefab;
        [SerializeField] private MoodTileView _moodPrefab;

        [Header("Pooling")]
        [SerializeField] private PoolService _poolService;

        [Header("Timing & Thresholds")]
        [SerializeField] private float _spawnLeadTime = 5f;
        [SerializeField] private float _missThreshold = 0f;
        [SerializeField] private float _hitThreshold = 0f;

        [Header("Layout")]
        [SerializeField] private float _hitLineY = -4f;
        [SerializeField] private float[] _laneXPositions = { -4.5f, -1.5f, 1.5f, 4.5f };

        [Header("Audio")]
        [SerializeField] private AudioSource _audioSource;

        [Header("Loop Settings")]
        [SerializeField] private bool _loopLevel;

        // --- Runtime ---

        private BeatMapModel _beatMap;
        private ITileFactory _tileFactory;
        private readonly List<TilePresenter> _tiles = new();
        private int _nextNoteIndex = 0;
        private float _currentTime = 0f;
        private bool _isPlaying = false;
        private float? _pendingSeekTime = null;

        /// <summary>Triggered khi mood note đến time. Subscribe để xử lý visual (bg_color, shadow...).</summary>
        public event Action<MetaData[]> OnMoodTriggered;

        public event Action OnLevelFinished;

        private void Start()
        {
            if (_beatMapJson != null)
                StartGame();
        }

        public void StartGame()
        {
            Time.timeScale = 0.5f;

            _beatMap = BeatMapLoader.Load(_beatMapJson);
            if (_beatMap == null)
            {
                Debug.LogError("[BoardPresenter] Failed to load beatmap.");
                return;
            }

            _tileFactory = new TileFactory(
                _poolService, _shortPrefab, _longPrefab, _zigzagPrefab, _moodPrefab);

            if (_missThreshold <= 0f)
                _missThreshold = _beatMap.VisualSpeed * 0.5f;
            if (_hitThreshold <= 0f)
                _hitThreshold = _beatMap.VisualSpeed * 0.15f;

            _nextNoteIndex = 0;
            _currentTime = 0f;
            _isPlaying = true;

            ScoreLevelManager.Instance.Reset();

            EventBus.Instance.Subscribe<LoseEvent>(OnLose);

            _audioSource.Play();
        }

        public void Pause() => _isPlaying = false;
        public void Resume() => _isPlaying = true;

        public void Stop()
        {
            _isPlaying = false;
            ClearAllTiles();
        }

        private void Update()
        {
            if (!_isPlaying) return;

            _currentTime = _audioSource.time;

            SpawnTiles();

            for (int i = 0; i < _tiles.Count; i++)
                _tiles[i].Tick(_currentTime);

            for (int i = _tiles.Count - 1; i >= 0; i--)
            {
                if (_tiles[i].ShouldRemove)
                {
                    _poolService.Release(_tiles[i].View);
                    _tiles.RemoveAt(i);
                }
            }

            if (_pendingSeekTime.HasValue)
            {
                float targetTime = _pendingSeekTime.Value;
                _pendingSeekTime = null;
                SeekTo(targetTime);
                return;
            }

            if (_nextNoteIndex > 0 && _nextNoteIndex >= _beatMap.NoteCount && _tiles.Count <= 0)
            {
                if (_loopLevel)
                {
                    StartCoroutine(LoopRestart());
                    _isPlaying = false;
                }
                else
                {
                    _isPlaying = false;
                    OnLevelFinished?.Invoke();
                }
            }
        }

        private void SpawnTiles()
        {
            while (_nextNoteIndex < _beatMap.NoteCount)
            {
                NoteData note = _beatMap.GetNote(_nextNoteIndex);

                if (note.time <= _currentTime + _spawnLeadTime)
                {
                    CreateTile(note);
                    _nextNoteIndex++;
                }
                else
                {
                    break;
                }
            }
        }

        private void CreateTile(NoteData note)
        {
            var model = new TileModel(note);
            TileView view = _tileFactory.Create(model.Type);
            if (view == null) return;

            view.Configure(note, _beatMap.VisualSpeed, _laneXPositions);

            // Mood không cần lane position (không di chuyển).
            if (model.Type != TileType.Mood)
                view.SetLanePosition(_laneXPositions[note.lane - 1]);

            // Subscribe mood event
            if (view is MoodTileView moodView)
                moodView.OnTrigger += metas => OnMoodTriggered?.Invoke(metas);

            var presenter = new TilePresenter(
                model, view, _beatMap.VisualSpeed,
                _missThreshold, _hitThreshold, _hitLineY);

            view.Initialize(presenter);
            _tiles.Add(presenter);
        }

        private void ClearAllTiles()
        {
            for (int i = 0; i < _tiles.Count; i++)
                _poolService.Release(_tiles[i].View);
            _tiles.Clear();
            _nextNoteIndex = 0;
            _currentTime = 0f;
        }

        private void OnDestroy()
        {
            if (EventBus.HasInstance)
                EventBus.Instance.Unsubscribe<LoseEvent>(OnLose);
            ClearAllTiles();
        }

        private void OnLose(LoseEvent e)
        {
            _pendingSeekTime = e.TileTime;
        }

        private void SeekTo(float targetTime)
        {
            _isPlaying = false;
            ClearAllTiles();

            _audioSource.time = targetTime;
            _currentTime = targetTime;

            _nextNoteIndex = 0;
            while (_nextNoteIndex < _beatMap.NoteCount
                   && _beatMap.GetNote(_nextNoteIndex).time < targetTime)
                _nextNoteIndex++;

            SpawnTiles();

            for (int i = 0; i < _tiles.Count; i++)
                _tiles[i].Tick(_currentTime);
        }

        private IEnumerator LoopRestart()
        {
            _audioSource.Stop();
            ClearAllTiles();

            yield return new WaitForSeconds(GlobalData.Instance.LoopRestartDelay);

            _audioSource.pitch += 0.2f;
            StartGame();
        }
    }
}