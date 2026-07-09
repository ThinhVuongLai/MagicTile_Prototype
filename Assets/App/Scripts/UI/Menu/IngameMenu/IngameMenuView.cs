using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MagicTile.UI.Menu
{
    public class IngameMenuView : MonoBehaviour
    {
        [SerializeField] private RectTransform _progressFill;
        [SerializeField] private RectTransform _progressBarArea;
        [SerializeField] private TextMeshProUGUI _totalScoreText;
        [SerializeField] private TextMeshProUGUI _accuracyText;
        [SerializeField] private TextMeshProUGUI _multiplierText;

        private float _progressMaxWidth;
        private MilestoneData[] _milestoneData;
        private LevelScoreMilestonePrefabData[] _milestonePrefabs;
        private LevelScoreMilestone[] _milestoneInstances;

        private void Awake()
        {
            _progressMaxWidth = _progressFill.rect.width;
        }

        public void Init(MilestoneData[] milestoneData, LevelScoreMilestonePrefabData[] milestonePrefabs)
        {
            _milestoneData = milestoneData;
            _milestonePrefabs = milestonePrefabs;

            _progressMaxWidth = _progressFill.rect.width;

            SetProgress(0f);
            SetTotalScore(0);
            SetAccuracy("");
            SetMultiplier("", false);

            DestroyMilestones();
            SpawnMilestones();
        }

        public void SetProgress(float ratio)
        {
            float width = Mathf.Clamp01(ratio) * _progressMaxWidth;
            _progressFill.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        }

        public void SetMilestoneAchieved(int index, bool achieved)
        {
            if (index < 0 || index >= _milestoneInstances.Length || _milestoneInstances[index] == null)
                return;

            _milestoneInstances[index].SetAchieved(achieved);
        }

        public void UpdateAllMilestones(int totalScore)
        {
            for (int i = 0; i < _milestoneData.Length; i++)
                SetMilestoneAchieved(i, totalScore >= _milestoneData[i].score);
        }

        public void SetTotalScore(int score)
        {
            _totalScoreText.text = score.ToString();
        }

        public void SetAccuracy(string text)
        {
            _accuracyText.text = text;
            _accuracyText.enabled = !string.IsNullOrEmpty(text);
        }

        public void SetMultiplier(string text, bool visible)
        {
            _multiplierText.text = text;
            _multiplierText.enabled = visible;
        }

        private void DestroyMilestones()
        {
            if (_milestoneInstances == null) return;

            for (int i = 0; i < _milestoneInstances.Length; i++)
            {
                if (_milestoneInstances[i] != null)
                    Destroy(_milestoneInstances[i].gameObject);
            }
            _milestoneInstances = null;
        }

        private void SpawnMilestones()
        {
            if (_milestoneData == null || _milestoneData.Length == 0 || _milestonePrefabs == null || _milestonePrefabs.Length == 0)
                return;

            _milestoneInstances = new LevelScoreMilestone[_milestoneData.Length];
            float maxScore = _milestoneData[^1].score;

            for (int i = 0; i < _milestoneData.Length; i++)
            {
                GameObject prefab = GetPrefabByType(_milestoneData[i].type);
                if (prefab == null) continue;

                LevelScoreMilestone instance = Instantiate(prefab, _progressBarArea).GetComponent<LevelScoreMilestone>();
                _milestoneInstances[i] = instance;

                float ratio = maxScore > 0 ? (float)_milestoneData[i].score / maxScore : 0f;
                float xPos = ratio * _progressMaxWidth;

                RectTransform rt = instance.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(xPos, rt.anchoredPosition.y);
            }
        }

        private GameObject GetPrefabByType(MilestoneType type)
        {
            for (int i = 0; i < _milestonePrefabs.Length; i++)
                if (_milestonePrefabs[i].type == type)
                    return _milestonePrefabs[i].prefab;
            return null;
        }
    }
}
