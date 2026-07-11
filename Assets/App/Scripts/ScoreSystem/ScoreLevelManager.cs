using MagicTile.Events;
using UnityEngine;

namespace MagicTile.ScoreSystem
{
    /// <summary>
    /// Quản lý điểm số và combo tập trung. Subscribe EventBus để nhận TileHitEvent,
    /// tính combo/score, publish ScoreChangedEvent &amp; ComboChangedEvent.
    /// Plain C# singleton — không phụ thuộc MonoBehaviour.
    /// </summary>
    public class ScoreLevelManager : Singleton<ScoreLevelManager>
    {
        // --- Combo tiers: (minCombo, multiplier) ---
        private static readonly (int min, float mult)[] ComboTiers =
        {
            (0,  1.0f),
            (5,  2.0f),
            (10, 3.0f),
            (20, 5.0f)
        };

        // --- State ---
        private int _currentScore;
        private int _currentCombo;
        private int _maxCombo;

        // --- Public read-only properties ---
        public int CurrentScore => _currentScore;
        public int CurrentCombo => _currentCombo;
        public int MaxCombo => _maxCombo;

        private ScoreLevelManager()
        {
            EventBus.Instance.Subscribe<TileHitEvent>(OnTileHit);
        }

        /// <summary>
        /// Reset điểm và combo cho level mới.
        /// </summary>
        public void Reset()
        {
            _currentScore = 0;
            _currentCombo = 0;
            _maxCombo = 0;
        }

        private void OnTileHit(TileHitEvent e)
        {
            _currentCombo++;
            if (_currentCombo > _maxCombo)
                _maxCombo = _currentCombo;

            float comboMult = GetComboMultiplier(_currentCombo);
            int addedScore = Mathf.RoundToInt(e.BasePoints * comboMult);

            _currentScore += addedScore;

            EventBus.Instance.Publish(new ScoreChangedEvent
            {
                TotalScore = _currentScore,
                AddedScore = addedScore,
                ComboMultiplier = comboMult,
                Accuracy = e.Accuracy
            });

            EventBus.Instance.Publish(new ComboChangedEvent
            {
                CurrentCombo = _currentCombo,
                ComboMultiplier = comboMult
            });
        }

        // --- Công thức tính combo ---

        private static float GetComboMultiplier(int combo)
        {
            for (int i = ComboTiers.Length - 1; i >= 0; i--)
                if (combo >= ComboTiers[i].min)
                    return ComboTiers[i].mult;
            return 1.0f;
        }

        protected override void OnDestroy()
        {
            if (EventBus.HasInstance)
                EventBus.Instance.Unsubscribe<TileHitEvent>(OnTileHit);
            base.OnDestroy();
        }
    }
}
