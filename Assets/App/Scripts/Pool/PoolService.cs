using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using MagicTile.ServiceLocator;

namespace MagicTile.Pool
{
    /// <summary>
    /// Manages multiple GameObjectPools by prefab. Reusable for tiles, VFX, etc.
    /// Assign prefabs in the inspector to pre-register pools at startup.
    /// </summary>
    public class PoolService : MonoBehaviour
    {
        [System.Serializable]
        public class PoolConfig
        {
            public GameObject prefab;
            [Tooltip("Number of instances to pre-instantiate at startup.")]
            public int prewarmCount = 10;
            [Tooltip("Maximum inactive instances kept in the pool. Excess are destroyed.")]
            public int maxSize = 100;
        }

        [SerializeField] private PoolConfig[] _configs;

        private readonly Dictionary<GameObject, GameObjectPool> _poolsByPrefab = new();
        private readonly Dictionary<GameObject, GameObjectPool> _poolByInstance = new();

        private void Awake()
        {
            ServiceLocator.ServiceLocator.Register(this);

            foreach (PoolConfig config in _configs)
            {
                if (config.prefab == null)
                    continue;

                var pool = new GameObjectPool(config.prefab, transform, config.prewarmCount, config.maxSize);
                _poolsByPrefab[config.prefab] = pool;
            }
        }

        /// <summary>
        /// Gets an instance of the given prefab from its pool (creates the pool on first request).
        /// </summary>
        public GameObject Get(GameObject prefab)
        {
            GameObjectPool pool = GetOrCreatePool(prefab);
            GameObject obj = pool.Get();
            _poolByInstance[obj] = pool;
            return obj;
        }

        /// <summary>
        /// Generic overload — gets a Component of type T from the prefab's pool.
        /// </summary>
        public T Get<T>(T prefab) where T : Component
        {
            return Get(prefab.gameObject).GetComponent<T>();
        }

        /// <summary>
        /// Returns an instance to its originating pool. Destroys if not from any pool.
        /// </summary>
        public void Release(GameObject obj)
        {
            if (obj == null)
                return;

            if (_poolByInstance.TryGetValue(obj, out GameObjectPool pool))
            {
                _poolByInstance.Remove(obj);
                pool.Release(obj);
            }
            else
            {
                Debug.LogWarning($"[PoolService] Object '{obj.name}' was not from any pool. Destroying.");
                Destroy(obj);
            }
        }

        /// <summary>
        /// Generic overload — releases a Component's GameObject to its pool.
        /// </summary>
        public void Release(Component component)
        {
            if (!component.IsDestroyed())
            {
                Release(component.gameObject);
            }
        }

        /// <summary>
        /// Destroys all inactive instances across all pools and clears registrations.
        /// </summary>
        public void ClearAll()
        {
            foreach (GameObjectPool pool in _poolsByPrefab.Values)
                pool.Clear();

            _poolsByPrefab.Clear();
            _poolByInstance.Clear();
        }

        private GameObjectPool GetOrCreatePool(GameObject prefab)
        {
            if (!_poolsByPrefab.TryGetValue(prefab, out GameObjectPool pool))
            {
                pool = new GameObjectPool(prefab, transform);
                _poolsByPrefab[prefab] = pool;
            }
            return pool;
        }

        private void OnDestroy()
        {
            ServiceLocator.ServiceLocator.Unregister<PoolService>();
            ClearAll();
        }
    }
}
