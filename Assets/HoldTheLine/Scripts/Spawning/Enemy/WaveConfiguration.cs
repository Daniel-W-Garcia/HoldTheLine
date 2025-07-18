using System.Collections.Generic;
using UnityEngine;

namespace HoldTheLine.Scripts.Spawning.Enemy
{
    public class WaveConfig : MonoBehaviour
    {
        [Header("Wave Settings")]
        public int spawnCount = 10;
        public float spawnFrequencyMin = 1f;
        public float spawnFrequencyMax = 2f;
        
        [Header("Enemy Composition")]
        public List<EnemySpawnData> enemyTypes;
        
        [Header("Spawn Behavior")]
        public float spawnRadius = 2f;
        
        /// <summary>
        /// Gets a random enemy prefab based on weights
        /// </summary>
        public GameObject GetRandomEnemyPrefab()
        {
            if (enemyTypes == null || enemyTypes.Count == 0) return null;
            
            float totalWeight = 0f;
            foreach (var enemy in enemyTypes)
            {
                totalWeight += enemy.spawnWeight;
            }
            
            float randomValue = Random.Range(0f, totalWeight);
            float currentWeight = 0f;
            
            foreach (var enemy in enemyTypes)
            {
                currentWeight += enemy.spawnWeight;
                if (randomValue <= currentWeight)
                {
                    return enemy.enemyPrefab;
                }
            }
            
            return enemyTypes[0].enemyPrefab; // Fallback
        }
    }
}