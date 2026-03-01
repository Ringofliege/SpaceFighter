using System.Collections.Generic;
using UnityEngine;

namespace SpaceFighter
{
    public class ObjectPooler : MonoBehaviour
    {
        public static ObjectPooler Instance { get; private set; }

        private readonly Dictionary<GameObject, Queue<GameObject>> _pools = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public GameObject Get(GameObject prefab, Vector3 pos, Quaternion rot)
        {
            if (_pools.TryGetValue(prefab, out Queue<GameObject> queue) && queue.Count > 0)
            {
                GameObject obj = queue.Dequeue();
                obj.transform.SetPositionAndRotation(pos, rot);
                obj.SetActive(true);
                return obj;
            }

            GameObject instance = Instantiate(prefab, pos, rot);
            return instance;
        }

        public void Return(GameObject obj, GameObject prefab)
        {
            obj.SetActive(false);

            if (!_pools.ContainsKey(prefab))
                _pools[prefab] = new Queue<GameObject>();

            _pools[prefab].Enqueue(obj);
        }

        public void WarmPool(GameObject prefab, int count)
        {
            if (!_pools.ContainsKey(prefab))
                _pools[prefab] = new Queue<GameObject>();

            for (int i = 0; i < count; i++)
            {
                GameObject obj = Instantiate(prefab);
                obj.SetActive(false);
                _pools[prefab].Enqueue(obj);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
