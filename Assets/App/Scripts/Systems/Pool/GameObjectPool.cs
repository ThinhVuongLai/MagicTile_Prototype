using System.Collections.Generic;
using UnityEngine;

namespace MagicTile.Pool
{
    /// <summary>
    /// Generic object pool for a single prefab type.
    /// Retrieves inactive instances or instantiates new ones; releases by deactivating and queueing.
    /// </summary>
    public class GameObjectPool
    {
        private readonly GameObject _prefab;
        private readonly Transform _parent;
        private readonly int _maxSize;
        private readonly Queue<GameObject> _inactive = new();

        public int InactiveCount => _inactive.Count;

        public GameObjectPool(GameObject prefab, Transform parent, int prewarmCount = 0, int maxSize = 100)
        {
            _prefab = prefab;
            _parent = parent;
            _maxSize = maxSize;
            Prewarm(prewarmCount);
        }

        /// <summary>
        /// Gets an instance from the pool (or creates a new one), activates it, and calls IPoolable.OnGetFromPool.
        /// </summary>
        public GameObject Get()
        {
            GameObject obj = _inactive.Count > 0 ? _inactive.Dequeue() : Object.Instantiate(_prefab, _parent);

            obj.SetActive(true);
            if (obj.TryGetComponent<IPoolable>(out var poolable))
                poolable.OnGetFromPool();

            return obj;
        }

        /// <summary>
        /// Deactivates the instance and returns it to the pool. Destroys if pool is full.
        /// </summary>
        public void Release(GameObject obj)
        {
            if (obj == null)
                return;

            if (obj.TryGetComponent<IPoolable>(out var poolable))
                poolable.OnReleaseToPool();

            obj.SetActive(false);

            if (_inactive.Count < _maxSize)
                _inactive.Enqueue(obj);
            else
                Object.Destroy(obj);
        }

        /// <summary>
        /// Destroys all inactive instances in the pool.
        /// </summary>
        public void Clear()
        {
            while (_inactive.Count > 0)
                Object.Destroy(_inactive.Dequeue());
        }

        private void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                GameObject obj = Object.Instantiate(_prefab, _parent);
                obj.SetActive(false);
                _inactive.Enqueue(obj);
            }
        }
    }
}
