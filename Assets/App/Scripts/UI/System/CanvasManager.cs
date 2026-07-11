using System;
using System.Collections.Generic;
using UnityEngine;

namespace MagicTile.UI
{
    public class CanvasManager : MonoBehaviour
    {
        [SerializeField] private UIConfig _uiConfig;

        private readonly Dictionary<UIName, ICanvasPresenter> _presenters = new();
        private readonly Dictionary<UIName, CanvasBase> _views = new();
        private readonly Dictionary<UIType, Transform> _typeParents = new();

        private void Awake()
        {
            foreach (UIType type in Enum.GetValues(typeof(UIType)))
            {
                var parent = new GameObject(type.ToString());
                parent.transform.SetParent(transform);

                var rect = parent.AddComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;

                _typeParents[type] = parent.transform;
            }

            ServiceLocator.ServiceLocator.Register(this);
        }

        public void Spawn(UIName uiName, params object[] parameters)
        {
            if (!_views.TryGetValue(uiName, out var view))
            {
                var info = FindUIInfo(uiName);
                if (info.Prefab == null)
                {
                    Debug.LogError($"[CanvasManager] UIName {uiName} not found in UIConfig.");
                    return;
                }

                var parent = _typeParents[info.Prefab.UIType];
                view = Instantiate(info.Prefab, parent);
                view.transform.SetAsLastSibling();

                var presenter = view.FirstSpawn();
                _presenters[uiName] = presenter;
                _views[uiName] = view;
            }

            _presenters[uiName].Init(parameters);
            view.IsActive = true;
        }

        public void Hide(UIName uiName)
        {
            if (_presenters.TryGetValue(uiName, out var presenter))
            {
                presenter.Hide();
                _views[uiName].IsActive = false;
            }
        }

        private UIInfo FindUIInfo(UIName uiName)
        {
            var infos = _uiConfig.UIInfos;
            for (int i = 0; i < infos.Count; i++)
            {
                if (infos[i].UIName == uiName)
                    return infos[i];
            }
            return default;
        }

        private void OnDestroy()
        {
            ServiceLocator.ServiceLocator.Unregister<CanvasManager>();
        }
    }
}
