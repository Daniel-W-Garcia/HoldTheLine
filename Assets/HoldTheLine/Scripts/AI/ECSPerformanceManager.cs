using System.Collections.Generic;
using MoreMountains.Tools;
using Pathfinding;
using Pathfinding.ECS;
using UnityEngine;

namespace HoldTheLine.Scripts.AI
{
    [AddComponentMenu("TopDown Engine/Character/AI/ECS Performance Manager")]
    public class ECSPerformanceManager : MonoBehaviour
    {
        [Header("LOD Settings")]
        public float[] LODDistances = { 15f, 30f, 60f, 100f };
    
        [Header("Update Frequencies")]
        public float[] BrainDecisionFrequencies = { 0.1f, 0.3f, 0.6f, 1f };
        public float[] BrainActionFrequencies = { 0.1f, 0.2f, 0.4f, 0.8f };
        public float[] PathRecalculationRates = { 0.5f, 1f, 2f, 5f };
    
        [Header("Culling")]
        public float MaxActiveDistance = 150f;
        public float UpdateInterval = 0.5f;
    
        private struct EnemyData
        {
            public Transform transform;
            public AIBrain brain;
            public FollowerEntity follower;
            public GameObject gameObject;
        }
    
        private List<EnemyData> _enemies;
        private Transform _playerTransform;
        private float _nextUpdateTime;
    
        void Start()
        {
            _enemies = new List<EnemyData>();

            var player = GameObject.FindWithTag("Player");
            if (player != null) _playerTransform = player.transform;

            // Gather all existing enemies
            foreach (var brain in FindObjectsByType<AIBrain>(FindObjectsSortMode.None))
            {
                var go = brain.gameObject;
                _enemies.Add(new EnemyData {
                    transform  = brain.transform,
                    brain      = brain,
                    follower   = go.GetComponent<FollowerEntity>(),
                    gameObject = go
                });
            }
        }
    
        void Update()
        {
            if (Time.time < _nextUpdateTime || _playerTransform == null) return;
            _nextUpdateTime = Time.time + UpdateInterval;
            Vector3 playerPos = _playerTransform.position;

            for (int i = 0; i < _enemies.Count; i++)
            {
                var data = _enemies[i];
                if (data.transform == null) continue;

                float distance = Vector3.Distance(data.transform.position, playerPos);
                if (distance > MaxActiveDistance)
                {
                    data.gameObject.SetActive(false);
                    continue;
                }
                data.gameObject.SetActive(true);

                int lodLevel = GetLODLevel(distance);
                ApplyLODSettings(data, lodLevel);
            }
        }
        public void RegisterEnemy(GameObject go)
        {
            var brain    = go.GetComponent<AIBrain>();
            var follower = go.GetComponent<FollowerEntity>();
            if (brain == null || follower == null) return;
            _enemies.Add(new EnemyData {
                transform  = go.transform,
                brain      = brain,
                follower   = follower,
                gameObject = go
            });
        }
    
        int GetLODLevel(float distance)
        {
            for (int i = 0; i < LODDistances.Length; i++)
            {
                if (distance < LODDistances[i]) return i;
            }
            return LODDistances.Length - 1;
        }
    
        void ApplyLODSettings(EnemyData enemy, int lodLevel)
        {
            if (enemy.brain != null)
            {
                enemy.brain.DecisionFrequency = BrainDecisionFrequencies[lodLevel];
                enemy.brain.ActionsFrequency  = BrainActionFrequencies[lodLevel];
                enemy.brain.enabled           = lodLevel < LODDistances.Length - 1;
            }
            if (enemy.follower != null)
            {
                var autoRepath = enemy.follower.autoRepath;
                autoRepath.period = PathRecalculationRates[lodLevel];
                enemy.follower.autoRepath = autoRepath;
                if (lodLevel > 2)
                {
                    enemy.follower.movementSettings = new MovementSettings {
                        follower          = enemy.follower.movementSettings.follower,
                        stopDistance      = enemy.follower.movementSettings.stopDistance,
                        isStopped         = enemy.follower.movementSettings.isStopped,
                        rotationSmoothing = 0.5f,
                        groundMask        = 0
                    };
                }
            }
        }
    }
}
