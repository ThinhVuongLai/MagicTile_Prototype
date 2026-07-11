using MagicTile.Booster;
using MagicTile.Events;
using MagicTile.ServiceLocator;
using UnityEngine;

namespace MagicTile.UI.Menu
{
    public class IngameMenuPresenter : ICanvasPresenter
    {
        private readonly IngameMenuView _view;
        private readonly MilestoneData[] _milestones;
        private HitAccuracy _latestAccuracy = HitAccuracy.Good;
        private float _latestMultiplier = 1.0f;

        public IngameMenuPresenter(IngameMenuView view)
        {
            _view = view;

            _view.SetPresenter(this);

            var levelService = ServiceLocator.ServiceLocator.Get<ServiceLocator.ILevelService>();
            var configManager = ServiceLocator.ServiceLocator.Get<ServiceLocator.ConfigManager>();

            _milestones = levelService?.CurrentLevelConfig?.milestones?.ToArray();
            var prefabs = configManager?.ScoreMilestoneItemConfig?.MilestonePrefabs?.ToArray();

            _view.Init(_milestones, prefabs);

            _view.SlowBoosterButton.onClick.AddListener(OnSlowBoosterClicked);
        }

        public void Init(params object[] parameters)
        {
            if (EventBus.HasInstance)
            {
                EventBus.Instance.Subscribe<ScoreChangedEvent>(OnScoreChanged);
                EventBus.Instance.Subscribe<ComboChangedEvent>(OnComboChanged);
                EventBus.Instance.Subscribe<BoosterEvent>(OnBoosterEvent);
                EventBus.Instance.Subscribe<SlowStartEvent>(OnSlowStart);
                EventBus.Instance.Subscribe<SlowEndEvent>(OnSlowEnd);
                EventBus.Instance.Subscribe<FinishRunMissTileEvent>(OnFinishRunMissTile);
                EventBus.Instance.Subscribe<ReplayLevel>(OnReplayLevel);
            }

            _view.OnShow();
        }

        public void Hide()
        {
            if (EventBus.HasInstance)
            {
                EventBus.Instance.Unsubscribe<ScoreChangedEvent>(OnScoreChanged);
                EventBus.Instance.Unsubscribe<ComboChangedEvent>(OnComboChanged);
                EventBus.Instance.Unsubscribe<BoosterEvent>(OnBoosterEvent);
                EventBus.Instance.Unsubscribe<SlowStartEvent>(OnSlowStart);
                EventBus.Instance.Unsubscribe<SlowEndEvent>(OnSlowEnd);
                EventBus.Instance.Unsubscribe<FinishRunMissTileEvent>(OnFinishRunMissTile);
                EventBus.Instance.Unsubscribe<ReplayLevel>(OnReplayLevel);
            }
        }

        private void OnScoreChanged(ScoreChangedEvent e)
        {
            _latestAccuracy = e.Accuracy;
            _latestMultiplier = e.ComboMultiplier;

            _view.SetTotalScore(e.TotalScore);
            _view.SetAccuracy(AccuracyToText(e.Accuracy));

            float maxScore = _milestones?.Length > 0 ? _milestones[^1].score : 0f;
            float ratio = maxScore > 0 ? (float)e.TotalScore / maxScore : 0f;
            _view.SetProgress(ratio);

            _view.UpdateAllMilestones(e.TotalScore);
            UpdateMultiplierVisibility();
        }

        private void OnComboChanged(ComboChangedEvent e)
        {
            _latestMultiplier = e.ComboMultiplier;
            UpdateMultiplierVisibility();
        }

        private void UpdateMultiplierVisibility()
        {
            bool show = _latestAccuracy == HitAccuracy.Perfect && _latestMultiplier > 1.0f;
            string text = show ? $"x{(int)_latestMultiplier}" : "";
            _view.SetMultiplier(text, show);
        }

        private void OnSlowBoosterClicked()
        {
            var LevelManager = ServiceLocator.ServiceLocator.Get<ILevelService>();
            if (LevelManager != null && !LevelManager.IsStatus(LevelStatus.Start))
                return;

            var boosterService = ServiceLocator.ServiceLocator.Get<IBoosterService>();
            boosterService?.RunBooster(BoosterType.Slow);

            _view.EnableSlowBoosterButton(false);
        }

        private void OnSlowStart(SlowStartEvent e)
        {
            _view.ShowFillCountDown();
        }

        private void OnSlowEnd(SlowEndEvent e)
        {
            _view.HideFillCountDown();
            _view.EnableSlowBoosterButton(true);
        }

        private void OnBoosterEvent(BoosterEvent e)
        {
            if (e.Type == BoosterType.Slow)
            {
                _view.SetFillCountDown(1f - e.RemainingPercent);
            }
        }

        private void OnFinishRunMissTile(FinishRunMissTileEvent e)
        {
            _view.ShowReplayObject();
        }

        private static string AccuracyToText(HitAccuracy a) => a switch
        {
            HitAccuracy.Perfect => "Perfect",
            HitAccuracy.Great => "Great",
            HitAccuracy.Good => "Cool",
            _ => ""
        };

        public void Dispose()
        {
            _view.SlowBoosterButton.onClick.RemoveListener(OnSlowBoosterClicked);
        }

        public void OnClickReplayNutton()
        {
            var levelManager = ServiceLocator.ServiceLocator.Get<ILevelService>();

            if (levelManager != null)
            {
                levelManager.ReplayLevel();
            }
#if UNITY_EDITOR
            else
            {
                Debug.LogError("Not Replay Level by not found LevelManager");
            }
#endif
        }

        private void OnReplayLevel(ReplayLevel replayLevel)
        {
            _view.ResetMenu();
        }
    }
}
