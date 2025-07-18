using System.Collections.Generic;
using UnityEngine;

namespace HoldTheLine.Scripts.Spawning.Enemy
{
    public class EnemyPoolManager : MonoBehaviour
    {
        [Header("Pool Settings")]
        public int initialPoolSize = 50;
        public int maxPoolSize = 100;
    
        private Dictionary<string, Queue<GameObject>> _pools = new Dictionary<string, Queue<GameObject>>();
        private Dictionary<string, GameObject> _prefabLookup = new Dictionary<string, GameObject>();
    
        public void InitializePool(GameObject prefab, int size)
        {
            string key = prefab.name;
            if (_pools.ContainsKey(key)) return;
        
            _pools[key] = new Queue<GameObject>();
            _prefabLookup[key] = prefab;
        
            // Pre-warm pool
            for (int i = 0; i < size; i++)
            {
                GameObject obj = Instantiate(prefab, transform);
                obj.SetActive(false);
                _pools[key].Enqueue(obj);
            }
        }
    
        public GameObject GetPooledObject(string prefabName)
        {
            if (!_pools.ContainsKey(prefabName)) return null;
        
            Queue<GameObject> pool = _pools[prefabName];
        
            if (pool.Count > 0)
            {
                return pool.Dequeue();
            }
        
            // Pool exhausted - create new if under limit
            if (transform.childCount < maxPoolSize)
            {
                return Instantiate(_prefabLookup[prefabName], transform);
            }
        
            return null;
        }
    
        public void ReturnToPool(GameObject obj)
        {
            string key = obj.name.Replace("(Clone)", "");
            if (_pools.ContainsKey(key))
            {
                obj.SetActive(false);
                _pools[key].Enqueue(obj);
            }
        }
    }
}