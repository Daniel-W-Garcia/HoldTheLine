using System.Collections.Generic;
using MoreMountains.Tools;
using Pathfinding;
using UnityEngine;

// Add this to your enemy spawn manager
namespace HoldTheLine.Scripts.Spawning.Enemy
{
    public class EnemySpawnOptimizer : MonoBehaviour
    {
        [Header("Pooling")]
        public GameObject EnemyPrefab;
        public int PoolSize = 100;
    
        private Queue<GameObject> _pool = new Queue<GameObject>();
    
        void Start()
        {
            // Pre-warm the pool
            for (int i = 0; i < PoolSize; i++)
            {
                var enemy = Instantiate(EnemyPrefab);
                enemy.SetActive(false);
                _pool.Enqueue(enemy);
            }
        }
    
        public GameObject SpawnEnemy(Vector3 position)
        {
            GameObject enemy = null;
        
            if (_pool.Count > 0)
            {
                enemy = _pool.Dequeue();
            }
            else
            {
                enemy = Instantiate(EnemyPrefab);
            }
        
            enemy.transform.position = position;
            enemy.SetActive(true);
        
            // Reset the AI state
            var brain = enemy.GetComponent<AIBrain>();
            if (brain != null) brain.ResetBrain();
        
            // Teleport A* agent to avoid path calculation from origin
            var follower = enemy.GetComponent<FollowerEntity>();
            if (follower != null) follower.Teleport(position);
        
            return enemy;
        }
    
        public void DespawnEnemy(GameObject enemy)
        {
            enemy.SetActive(false);
            _pool.Enqueue(enemy);
        }
    }
}
