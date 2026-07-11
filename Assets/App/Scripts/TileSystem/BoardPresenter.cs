using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using MagicTile.Events;
using MagicTile.Audio;
using MagicTile.Pool;
using MagicTile.ScoreSystem;
using MagicTile.UI.Menu;
using MagicTile.ServiceLocator;
using Cysharp.Threading.Tasks;
using System.Threading;
using MagicTile.Background;
using MagicTile.GlobalData;

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

        [Header("Level Lines")]
        [SerializeField] private GameObject _levelLinePrefab;

        [Header("UI")]
        [SerializeField] private IngameMenuView _ingameMenuPrefab;

        // --- Runtime ---

        private BeatMapModel _beatMap;
        private ITileFactory _tileFactory;
        private readonly List<TilePresenter> _tiles = new();
        private int _nextNoteIndex = 0;
        private float _currentTime = 0f;
        private bool _isCountdownPhase = false;

        // Service
        private ILevelService _levelService;
        private PoolService _poolService;
        private AudioService _audioService;

        // Config
        private LevelRunTimeConfig _levelConfig;
        private TileConfig _tileConfig;
        private LevelBackgroundConfig _levelBackgroundConfig;

        // Background
        private LevelBackgroundPresenter _backgroundPresenter;

        private float _missThreshold;
        private float _hitThreshold;
        private Camera _mainCamera;
        private readonly Dictionary<int, TilePresenter> _dragTiles = new();
        private readonly Dictionary<MoodTileView, Action<MetaData[]>> _moodHandlers = new();
        private const int MOUSE_POINTER_ID = -1;
        private readonly List<GameObject> _levelLines = new();

        public event Action OnLevelFinished;

        private float _loopDelay = 2;

        private CancellationTokenSource _cancellationTokenSource = null;

        private void Start()
        {
            _poolService = ServiceLocator.ServiceLocator.Get<PoolService>();
            _audioService = ServiceLocator.ServiceLocator.Get<AudioService>();
            _levelService = ServiceLocator.ServiceLocator.Get<ILevelService>();
            _mainCamera = Camera.main;

            var configManager = ServiceLocator.ServiceLocator.Get<ConfigManager>();
            if (configManager)
            {
                _loopDelay = configManager.GlobalData == null ? 2 : configManager.GlobalData.LoopRestartDelay;

                _levelConfig = configManager.LevelRunTimeConfig;
                _tileConfig = configManager.TileConfig;
                _levelBackgroundConfig = configManager.LevelBackgroundConfig;
            }

            _tileFactory = new TileFactory();
        }

        public void StartGame()
        {
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

            _missThreshold = _levelConfig.MissThreshold;
            _hitThreshold = _levelConfig.HitThreshold;
            if (_missThreshold <= 0f)
                _missThreshold = _beatMap.VisualSpeed * 0.5f;
            if (_hitThreshold <= 0f)
                _hitThreshold = _beatMap.VisualSpeed * 0.15f;

            float countdown = levelInfo?.countdownDuration ?? 3f;
            _currentTime = -countdown;
            _isCountdownPhase = true;
        }

        public void InitLevel(Transform backgroundContainer, Transform lineContainer)
        {
            var levelService = ServiceLocator.ServiceLocator.Get<ILevelService>();
            var levelInfo = levelService?.CurrentLevelConfig;

            _nextNoteIndex = 0;

            ScoreLevelManager.Instance.Reset();

            SpawnLevelLines(lineContainer);

            SpawnBackground(levelInfo.backgroundIndex, backgroundContainer);

            EventBus.Instance.Subscribe<LoseEvent>(OnLose);
            EventBus.Instance.Subscribe<TileHitEvent>(OnTileHit);
        }

        public void Pause()
        {
            _levelService?.SetLevelStatus(LevelStatus.Pause);

            _audioService.Pause();
        }

        public void Resume()
        {
            _levelService?.SetLevelStatus(LevelStatus.Start);

            _audioService.Resume();
        }

        public void Stop()
        {
            _levelService?.SetLevelStatus(LevelStatus.None);

            _audioService.Stop();

            ClearLevel();
        }

        public void ResetLevel()
        {
            _audioService.Stop();
            ClearAllTiles();
            _audioService.Pitch = 1f;
            ScoreLevelManager.Instance.Reset();

            _nextNoteIndex = 0;

            EventBus.Instance.Publish<ReplayLevel>(new ReplayLevel());

            StartGame();
        }

        private IEnumerator RunSlowLevel(float duration)
        {
            float originalPitch = _audioService.Pitch;
            _audioService.Pitch = 0.7f;
            yield return new WaitForSeconds(duration);
            _audioService.Pitch = originalPitch;
        }

        private void Update()
        {
            if (_levelService == null || !_levelService.IsStatus(LevelStatus.Start)) return;

            if (_isCountdownPhase)
            {
                _currentTime += Time.deltaTime;
                if (_currentTime >= 0f)
                {
                    _currentTime = 0f;
                    _isCountdownPhase = false;
                    _audioService.Play();
                }
            }
            else
            {
                _currentTime = _audioService.Time;
            }

            SpawnTiles();

            for (int i = 0; i < _tiles.Count; i++)
                _tiles[i].Tick(_currentTime);

            for (int i = _tiles.Count - 1; i >= 0; i--)
            {
                if (_tiles[i].ShouldRemove)
                {
                    UnsubscribeMoodHandler(_tiles[i].View);
                    _poolService.Release(_tiles[i].View);
                    _tiles.RemoveAt(i);
                }
            }

            ProcessInput();

            if (_nextNoteIndex > 0 && _nextNoteIndex >= _beatMap.NoteCount && _tiles.Count <= 0)
            {
                if (_loopLevel)
                {
                    _cancellationTokenSource = new CancellationTokenSource();
                    AsyncLoopRestart(_cancellationTokenSource.Token).Forget();
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
            {
                Action<MetaData[]> handler = metas => EventBus.Instance.Publish(new MoodTriggeredEvent { Metas = metas });
                moodView.OnTrigger += handler;
                _moodHandlers[moodView] = handler;
            }

            var presenter = new TilePresenter(
                model, view, _beatMap.VisualSpeed,
                _missThreshold, _hitThreshold, _levelConfig.HitLineY);

            view.Initialize(presenter);
            _tiles.Add(presenter);
        }

        // --- Centralized Input Polling ---

        private void ProcessInput()
        {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            // --- Pointer Down + Up (Touch) ---
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (IsPointerOverUI(touch.fingerId)) continue;

                Vector2 worldPos = _mainCamera.ScreenToWorldPoint(touch.position);

                if (touch.phase == TouchPhase.Began)
                    ProcessPointerDown(touch.fingerId, worldPos);

                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    ProcessPointerUp(touch.fingerId);
            }
#else
            // --- Pointer Down (Mouse) ---
            if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
            {
                Vector2 worldPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
                ProcessPointerDown(MOUSE_POINTER_ID, worldPos);
            }

            // --- Mouse Up ---
            if (Input.GetMouseButtonUp(0))
                ProcessPointerUp(MOUSE_POINTER_ID);
#endif

            // --- Drag Update (every frame) ---
            ProcessDrags();
        }

        private void ProcessPointerDown(int pointerId, Vector2 worldPos)
        {
            int lane = GetLaneFromWorldX(worldPos.x);
            if (lane < 0) return;

            // --- Thử CheckHit trên tile cùng lane ---
            for (int i = 0; i < _tiles.Count; i++)
            {
                var tile = _tiles[i];
                if (tile.Model.Lane != lane) continue;
                if (tile.Model.IsHit || tile.Model.IsMissed || tile.Model.IsHidden) continue;

                if (tile.CheckHit(worldPos))
                {
                    if (tile.View is IDragStrategy dragView)
                    {
                        dragView.StartDrag(worldPos);
                        _dragTiles[pointerId] = tile;
                    }
                    return;
                }
            }

            // --- Không hit tile nào → tìm tile gần nhất theo Y ---
            SpawnMissEffect(lane, worldPos.y);
        }

        private void SpawnBackground(int backgroundIndex, Transform backgroundContainer)
        {
            var info = _levelBackgroundConfig?.GetByIndex(backgroundIndex);
            if (info?.View == null)
            {
                Debug.LogWarning($"[LevelManager] No background found with index {backgroundIndex}");
                return;
            }

            _backgroundPresenter?.Dispose();
            var view = UnityEngine.Object.Instantiate(info.View, backgroundContainer);
            view.transform.localPosition = Vector3.zero;
            view.FitToScreen();
            _backgroundPresenter = new LevelBackgroundPresenter(view);
        }

        private void SpawnLevelLines(Transform lineContainer)
        {
            if (_levelLinePrefab == null) return;

            float[] laneXPositions = _levelConfig.LaneXPositions;
            int laneCount = laneXPositions.Length;

            for (int i = 0; i < laneCount - 1; i++)
            {
                float midX = (laneXPositions[i] + laneXPositions[i + 1]) * 0.5f;
                var lineObj = _poolService.Get(_levelLinePrefab);
                lineObj.transform.SetParent(lineContainer);
                lineObj.transform.position = new Vector3(midX, 0f, 0f);
                _levelLines.Add(lineObj);
            }
        }

        private void SpawnMissEffect(int lane, float touchY)
        {
            TilePresenter closestTile = null;
            float closestDist = float.MaxValue;

            for (int i = 0, length = _tiles.Count; i < length; i++)
            {
                var tile = _tiles[i];
                if (tile.Model.IsHit || tile.Model.IsMissed || tile.Model.IsHidden) continue;
                if (tile.Model.Type == TileType.Mood) continue;

                float tileY = tile.View.transform.position.y;
                float topY = tile.GetTopEdgeWorldPositionY();

                if (touchY >= tileY && touchY <= topY)
                {
                    float dist = topY - touchY;
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestTile = tile;
                    }
                }
            }

            if (closestTile == null) return;

            Pause();

            if (_tileConfig.MissPrefab != null)
            {
                var poolService = ServiceLocator.ServiceLocator.Get<PoolService>();
                MissTileItem missObj = poolService?.Get(_tileConfig.MissPrefab);

                if (missObj == null) return;

                float laneX = _levelConfig.LaneXPositions[lane - 1];
                float tileY = closestTile.View.transform.position.y;
                missObj.transform.position = new Vector3(laneX, tileY, 0f);

                float height = closestTile.GetTileHeight();
                missObj.UpdateSpriteRenderer(height);
            }
        }

        private void ProcessPointerUp(int pointerId)
        {
            if (_dragTiles.TryGetValue(pointerId, out var tile))
            {
                (tile.View as IDragStrategy)?.EndDrag();
                _dragTiles.Remove(pointerId);
            }
        }

        private void ProcessDrags()
        {
            if (_dragTiles.Count == 0) return;

            var endedPointerIds = new List<int>();

            foreach (var kvp in _dragTiles)
            {
                int pointerId = kvp.Key;
                var tile = kvp.Value;

                if (!IsPointerActive(pointerId, out Vector2 worldPos))
                {
                    (tile.View as IDragStrategy)?.EndDrag();
                    endedPointerIds.Add(pointerId);
                    continue;
                }

                if (!IsPointerOverTile(tile, worldPos))
                {
                    (tile.View as IDragStrategy)?.EndDrag();
                    endedPointerIds.Add(pointerId);
                    continue;
                }

                (tile.View as IDragStrategy)?.UpdateDrag(worldPos);
            }

            foreach (int id in endedPointerIds)
                _dragTiles.Remove(id);
        }

        private bool IsPointerOverTile(TilePresenter tile, Vector2 worldPos)
        {
            if (tile.Model.Type == TileType.Zigzag)
            {
                RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
                return hit.collider != null && hit.collider.gameObject == tile.View.gameObject;
            }

            Bounds touchBounds = new Bounds(worldPos, new Vector3(0.4f, 0.4f, 1f));
            Bounds spriteBounds = tile.View.GetSpriteBounds();
            return touchBounds.Intersects(spriteBounds);
        }

        private bool IsPointerActive(int pointerId, out Vector2 worldPos)
        {
            if (pointerId == MOUSE_POINTER_ID)
            {
                worldPos = Input.GetMouseButton(0)
                    ? (Vector2)_mainCamera.ScreenToWorldPoint(Input.mousePosition)
                    : Vector2.zero;
                return Input.GetMouseButton(0);
            }

            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.fingerId == pointerId)
                {
                    worldPos = _mainCamera.ScreenToWorldPoint(touch.position);
                    return touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled;
                }
            }

            worldPos = Vector2.zero;
            return false;
        }

        private int GetLaneFromWorldX(float worldX)
        {
            float[] laneXPositions = _levelConfig.LaneXPositions;
            int closestLane = -1;
            float closestDist = float.MaxValue;
            float laneWidth = (laneXPositions.Length >= 2)
                ? Mathf.Abs(laneXPositions[1] - laneXPositions[0]) * 0.5f
                : 1f;

            for (int i = 0; i < laneXPositions.Length; i++)
            {
                float dist = Mathf.Abs(worldX - laneXPositions[i]);
                if (dist < closestDist && dist <= laneWidth)
                {
                    closestDist = dist;
                    closestLane = i + 1;
                }
            }
            return closestLane;
        }

        private bool IsPointerOverUI(int fingerId = -1)
        {
            if (EventSystem.current == null) return false;
            return fingerId >= 0
                ? EventSystem.current.IsPointerOverGameObject(fingerId)
                : EventSystem.current.IsPointerOverGameObject();
        }

        // --- Tile Management ---
        private void ClearAllTiles()
        {
            for (int i = 0; i < _tiles.Count; i++)
            {
                UnsubscribeMoodHandler(_tiles[i].View);
                _poolService.Release(_tiles[i].View);
            }
            _tiles.Clear();
            _dragTiles.Clear();

            _nextNoteIndex = 0;
            _currentTime = 0f;
            _isCountdownPhase = false;
            _moodHandlers.Clear();
        }

        private void UnsubscribeMoodHandler(TileView view)
        {
            if (view is MoodTileView moodView && _moodHandlers.TryGetValue(moodView, out var handler))
            {
                moodView.OnTrigger -= handler;
                _moodHandlers.Remove(moodView);
            }
        }

        private void ClearLevelLine()
        {
            for (int i = 0; i < _levelLines.Count; i++)
                _poolService.Release(_levelLines[i]);
            _levelLines.Clear();
        }

        private void ClearBackground()
        {
            _backgroundPresenter = null;
        }

        private void ClearLevel()
        {
            ClearAllTiles();
            ClearLevelLine();
            ClearBackground();

            if (EventBus.HasInstance)
            {
                EventBus.Instance.Unsubscribe<LoseEvent>(OnLose);
                EventBus.Instance.Unsubscribe<TileHitEvent>(OnTileHit);
            }
        }

        private void OnDestroy()
        {
            if (EventBus.HasInstance)
            {
                EventBus.Instance.Unsubscribe<LoseEvent>(OnLose);
                EventBus.Instance.Unsubscribe<TileHitEvent>(OnTileHit);
            }

            ClearLevel();

            CancelAsync();
        }

        private void OnDisable()
        {
            CancelAsync();
        }

        private void OnLose(LoseEvent e)
        {
            _levelService?.SetLevelStatus(LevelStatus.Lose);
            _audioService.Pause();

            StartCoroutine(RunLoseSeekAndSpawnReplay(e.TileTime));
        }

        private void OnTileHit(TileHitEvent e)
        {
            if (e.IsDragComplete) return;

            var effectPrefab = ServiceLocator.ServiceLocator.Get<ConfigManager>()?.GlobalData?.EffectTouchItem;
            if (effectPrefab == null) return;

            var effect = _poolService.Get(effectPrefab);
            if (effect == null) return;

            effect.transform.position = new Vector3(e.TouchPosition.x, e.TouchPosition.y, 0f);
        }

        private IEnumerator RunLoseSeekAndSpawnReplay(float targetTime)
        {
            yield return new WaitForEndOfFrame();

            SeekTo(targetTime, false);

            yield return new WaitForSecondsRealtime(_levelConfig.SeekDuration);

            SpawnReplayTileItem();
        }

        private void SpawnReplayTileItem()
        {
            var runTimeConfig = ServiceLocator.ServiceLocator.Get<ConfigManager>()?.LevelRunTimeConfig;

            if (_tileConfig?.ReplayTileItem == null || runTimeConfig == null) return;

            var laneX = runTimeConfig.LaneXPositions[1];
            var screenBottom = _mainCamera.transform.position.y - _mainCamera.orthographicSize;

            var replayItem = _poolService.Get(_tileConfig.ReplayTileItem);
            replayItem.transform.position = new Vector3(laneX, screenBottom + 3f, 0f);
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
                    UnsubscribeMoodHandler(_tiles[i].View);
                    _poolService.Release(_tiles[i].View);
                    _tiles.RemoveAt(i);
                }
            }

            if (_isCountdownPhase) return;

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

        private async UniTask AsyncLoopRestart(CancellationToken cancellationToken)
        {
            _audioService.Stop();
            ClearAllTiles();

            int delayMiliSecond = Mathf.CeilToInt(_loopDelay * 1000);
            await UniTask.Delay(delayMiliSecond, cancellationToken: cancellationToken);

            _audioService.Pitch += 0.2f;
            StartGame();
        }

        private void CancelAsync()
        {
            if (_cancellationTokenSource == null)
                return;

            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }
    }
}
