using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MagicTile.Events;
using MagicTile.Audio;
using MagicTile.Pool;
using MagicTile.ScoreSystem;
using MagicTile.UI.Menu;
using MagicTile.ServiceLocator;

namespace MagicTile.TileSystem
{
    /// <summary>
    /// Điều khiển chính của tile system: spawn tiles theo thời gian, Tick tất cả
    /// TilePresenter mỗi frame, cleanup tiles đã hide. Dùng TileFactory (Create)
    /// và PoolService (Release) riêng biệt.
    /// </summary>
    public class BoardPresenter : MonoBehaviour
    {
        [Header("Loop Settings")]
        [SerializeField] private bool _loopLevel;

        [Header("UI")]
        [SerializeField] private IngameMenuView _ingameMenuPrefab;

        // --- Runtime ---

        private BeatMapModel _beatMap;
        private ITileFactory _tileFactory;
        private readonly List<TilePresenter> _tiles = new();
        private int _nextNoteIndex = 0;
        private float _currentTime = 0f;

        private ILevelService _levelService;

        private PoolService _poolService;
        private AudioService _audioService;
        private LevelRunTimeConfig _levelConfig;
        private float _missThreshold;
        private float _hitThreshold;

        /// <summary>Triggered khi mood note đến time. Subscribe để xử lý visual (bg_color, shadow...).</summary>
        public event Action<MetaData[]> OnMoodTriggered;

        public event Action OnLevelFinished;

        private void Start()
        {
            _poolService = ServiceLocator.ServiceLocator.Get<PoolService>();
            _audioService = ServiceLocator.ServiceLocator.Get<AudioService>();
            _levelService = ServiceLocator.ServiceLocator.Get<ILevelService>();
        }

        public void StartGame()
        {
            Time.timeScale = 0.5f;

            IngameMenuPresenter ingameMenuPresenter = new IngameMenuPresenter(_ingameMenuPrefab);

            var levelService = ServiceLocator.ServiceLocator.Get<ILevelService>();
            levelService?.SetLevelStatus(LevelStatus.Start);
            var configManager = ServiceLocator.ServiceLocator.Get<ConfigManager>();
            var levelInfo = levelService?.CurrentLevelConfig;

            if (!string.IsNullOrEmpty(levelInfo?.jsonPath))
                _beatMap = BeatMapLoader.LoadFromResources(levelInfo.jsonPath);

            if (_beatMap == null)
                _beatMap = BeatMapLoader.Load(configManager?.LevelConfig?.DefaultJson);

            if (_beatMap == null)
            {
                Debug.LogError("[BoardPresenter] Failed to load beatmap.");
                return;
            }

            _tileFactory = new TileFactory();

            _levelConfig = ServiceLocator.ServiceLocator.Get<ConfigManager>().LevelRunTimeConfig;

            _missThreshold = _levelConfig.MissThreshold;
            _hitThreshold = _levelConfig.HitThreshold;
            if (_missThreshold <= 0f)
                _missThreshold = _beatMap.VisualSpeed * 0.5f;
            if (_hitThreshold <= 0f)
                _hitThreshold = _beatMap.VisualSpeed * 0.15f;

            _nextNoteIndex = 0;
            _currentTime = 0f;

            ScoreLevelManager.Instance.Reset();

            EventBus.Instance.Subscribe<LoseEvent>(OnLose);

            _audioService.Play();
        }

        public void Pause()
        {
            _levelService?.SetLevelStatus(LevelStatus.Pause);
        }

        public void Resume()
        {
            _levelService?.SetLevelStatus(LevelStatus.Start);
        }

        public void Stop()
        {
            _levelService?.SetLevelStatus(LevelStatus.None);
            ClearAllTiles();
        }

        private void Update()
        {
            if (_levelService == null || !_levelService.IsStatus(LevelStatus.Start)) return;

            _currentTime = _audioService.Time;

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

            if (_nextNoteIndex > 0 && _nextNoteIndex >= _beatMap.NoteCount && _tiles.Count <= 0)
            {
                if (_loopLevel)
                {
                    StartCoroutine(LoopRestart());
                    _levelService?.SetLevelStatus(LevelStatus.None);
                }
                else
                {
                    _levelService?.SetLevelStatus(LevelStatus.Complete);
                    OnLevelFinished?.Invoke();
                }
            }
        }

        private void SpawnTiles()
        {
            while (_nextNoteIndex < _beatMap.NoteCount)
            {
                NoteData note = _beatMap.GetNote(_nextNoteIndex);

                if (note.time <= _currentTime + _levelConfig.SpawnLeadTime)
                {
                    CreateTile(note, _nextNoteIndex);
                    _nextNoteIndex++;
                }
                else
                {
                    break;
                }
            }
        }

        private void CreateTile(NoteData note, int noteIndex)
        {
            var model = new TileModel(note, noteIndex);
            TileView view = _tileFactory.Create(model.Type);
            if (view == null) return;

            view.Configure(note, _beatMap.VisualSpeed, _levelConfig.LaneXPositions);

            // Mood không cần lane position (không di chuyển).
            if (model.Type != TileType.Mood)
                view.SetLanePosition(_levelConfig.LaneXPositions[note.lane - 1]);

            // Subscribe mood event
            if (view is MoodTileView moodView)
                moodView.OnTrigger += metas => OnMoodTriggered?.Invoke(metas);

            var presenter = new TilePresenter(
                model, view, _beatMap.VisualSpeed,
                _missThreshold, _hitThreshold, _levelConfig.HitLineY);

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
            _levelService?.SetLevelStatus(LevelStatus.Lose);
            _audioService.Pause();

            StartCoroutine(RunSeek(e.TileTime, false));
        }

        private IEnumerator RunSeek(float targetTime, bool force = true)
        {
            yield return new WaitForEndOfFrame();

            SeekTo(targetTime, force);
        }

        private void SeekTo(float targetTime, bool force = true)
        {
            for (int i = _tiles.Count - 1; i >= 0; i--)
            {
                if (_tiles[i].ShouldRemove)
                {
                    _poolService.Release(_tiles[i].View);
                    _tiles.RemoveAt(i);
                }
            }

            _audioService.Time = targetTime;
            _currentTime = targetTime;

            if (force)
            {
                for (int i = 0; i < _tiles.Count; i++)
                {
                    if (_tiles[i].Model.Type != TileType.Mood)
                        _tiles[i].SetPositionByTime(targetTime);
                }
            }
            else
            {
                if (_tiles.Count == 0) return;

                TilePresenter firstTile = null;
                for (int i = 0; i < _tiles.Count; i++)
                {
                    if (_tiles[i].Model.Type != TileType.Mood)
                    {
                        firstTile = _tiles[i];
                        break;
                    }
                }
                if (firstTile == null) return;

                float currentY = firstTile.View.transform.position.y;
                firstTile.SetPositionByTime(targetTime);
                float targetY = firstTile.View.transform.position.y;
                firstTile.View.SetPosition(currentY);

                float totalDeltaY = targetY - currentY;
                StartCoroutine(AnimateSeek(totalDeltaY, _levelConfig.SeekDuration));
            }
        }

        private IEnumerator AnimateSeek(float totalDeltaY, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float dt = Time.unscaledDeltaTime;
                float clampedDt = Mathf.Min(dt, duration - elapsed);
                elapsed += clampedDt;
                float frameDeltaY = totalDeltaY * (clampedDt / duration);

                for (int i = 0; i < _tiles.Count; i++)
                {
                    if (_tiles[i].Model.Type != TileType.Mood)
                        _tiles[i].AddPositionY(frameDeltaY);
                }

                if (clampedDt <= 0f)
                    yield return null;
                else
                    yield return null;
            }
        }

        private IEnumerator LoopRestart()
        {
            _audioService.Stop();
            ClearAllTiles();

            yield return new WaitForSeconds(GlobalData.Instance.LoopRestartDelay);

            _audioService.Pitch += 0.2f;
            StartGame();
        }
    }
}
