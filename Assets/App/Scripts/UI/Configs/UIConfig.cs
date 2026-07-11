using System.Collections.Generic;
using UnityEngine;

namespace MagicTile.UI
{
    [CreateAssetMenu(fileName = "UIConfig", menuName = "MagicTile/UIConfig")]
    public class UIConfig : ScriptableObject
    {
        [SerializeField] private List<UIInfo> _uiInfos;

        public List<UIInfo> UIInfos => _uiInfos;
    }
}
