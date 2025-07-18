using UnityEngine;

namespace HoldTheLine.Scripts.Spawning.Enemy
{
    public class EnemySpawnData
    {
        public GameObject enemyPrefab;
        [Range(0f, 1f)] public float spawnWeight = 1f;
    }
}


