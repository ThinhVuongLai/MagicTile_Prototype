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
        // --- State ---
        private int _currentScore;
        private int _currentCombo;
        // --- Public read-only properties ---
        public int CurrentScore => _currentScore;
        public int CurrentCombo => _currentCombo;

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
        }

        private void OnTileHit(TileHitEvent e)
        {
            if (e.Accuracy == HitAccuracy.Perfect)
                _currentCombo++;
            else
                _currentCombo = 0;

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
                return combo;
        }

        protected override void OnDestroy()
        {
            if (EventBus.HasInstance)
                EventBus.Instance.Unsubscribe<TileHitEvent>(OnTileHit);
            base.OnDestroy();
        }
    }
}
