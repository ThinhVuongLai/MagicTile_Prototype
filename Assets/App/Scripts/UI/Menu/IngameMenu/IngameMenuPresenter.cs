using MagicTile.Events;
using MagicTile.ServiceLocator;

namespace MagicTile.UI.Menu
{
    public class IngameMenuPresenter
    {
        private readonly IngameMenuView _view;
        private readonly MilestoneData[] _milestones;
        private HitAccuracy _latestAccuracy = HitAccuracy.Good;
        private float _latestMultiplier = 1.0f;

        public IngameMenuPresenter(IngameMenuView view)
        {
            _view = view;

            var levelService = ServiceLocator.ServiceLocator.Get<ServiceLocator.ILevelService>();
            var configManager = ServiceLocator.ServiceLocator.Get<ServiceLocator.ConfigManager>();

            _milestones = levelService?.CurrentLevelConfig?.milestones?.ToArray();
            var prefabs = configManager?.ScoreMilestoneItemConfig?.MilestonePrefabs?.ToArray();

            _view.Init(_milestones, prefabs);

            EventBus.Instance.Subscribe<ScoreChangedEvent>(OnScoreChanged);
            EventBus.Instance.Subscribe<ComboChangedEvent>(OnComboChanged);
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
            string text = show ? $"x{_latestMultiplier:F1}" : "";
            _view.SetMultiplier(text, show);
        }

        private static string AccuracyToText(HitAccuracy a) => a switch
        {
            HitAccuracy.Perfect => "Perfect",
            HitAccuracy.Great   => "Great",
            HitAccuracy.Good    => "Cool",
            _                   => ""
        };

        public void Dispose()
        {
            if (EventBus.HasInstance)
            {
                EventBus.Instance.Unsubscribe<ScoreChangedEvent>(OnScoreChanged);
                EventBus.Instance.Unsubscribe<ComboChangedEvent>(OnComboChanged);
            }
        }
    }
}
