using UnityEngine;
using UnityEngine.UI;

namespace MagicTile.UI.Menu
{
    public enum MilestoneType
    {
        Star,
        Crown
    }

    [System.Serializable]
    public class LevelScoreMilestonePrefabData
    {
        public MilestoneType type;
        public GameObject prefab;
    }

    [System.Serializable]
    public class MilestoneData
    {
        public MilestoneType type;
        public int score;
    }

    public class LevelScoreMilestone : MonoBehaviour
    {
        [SerializeField] private MilestoneType _type;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Sprite _offSprite;
        [SerializeField] private Sprite _onSprite;

        public MilestoneType Type => _type;

        public void SetAchieved(bool achieved)
        {
            if (_iconImage != null)
                _iconImage.sprite = achieved ? _onSprite : _offSprite;
        }
    }
}
