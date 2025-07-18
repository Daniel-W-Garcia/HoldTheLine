using System;
using System.Collections;
using System.Collections.Generic;
using HoldTheLine.Scripts.AI;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using UnityEngine;
using Character = MoreMountains.TopDownEngine.Character;
using Random = UnityEngine.Random;

namespace HoldTheLine.Scripts.Spawning.Enemy
{
    [AddComponentMenu("TopDown Engine/Spawner/Wave Manager")]
    public class WaveManager : MonoBehaviour
    {
        [Header("Wave Management")]
        [Tooltip("List of Wave GameObjects with WaveConfig components")]
        public List<GameObject> waveGameObjects;
        
        [Tooltip("Time between waves")]
        public float timeBetweenWaves = 5f;
        
        [Header("Spawn Locations")]
        [Tooltip("All available spawn point transforms")]
        public List<Transform> SpawnPoints;
        
        [Header("Object Pooling")]
        [Tooltip("Multiple Object Pooler for enemies")]
        public MMMultipleObjectPooler objectPooler;
        
        [Header("Win Condition")]
        [Tooltip("The KillsManager that will track our win condition")]
        public KillsManager TheKillsManager;

        /// <summary>
        /// Current wave index
        /// </summary>
        public int CurrentWaveIndex { get; private set; }
        
        /// <summary>
        /// Current wave configuration
        /// </summary>
        public WaveConfig CurrentWaveConfig { get; private set; }
        
        /// <summary>
        /// Events
        /// </summary>
        public event Action<int> OnWaveStarted;
        public event Action<int> OnWaveCompleted;
        public event Action OnAllWavesCompleted;

        protected int _currentSpawnCount;
        protected float _lastSpawnTime;
        protected float _nextSpawnDelay;
        protected bool _isSpawning;
        protected ECSPerformanceManager _perfManager;

        protected virtual void Start()
        {
            if (objectPooler == null)
            {
                objectPooler = GetComponent<MMMultipleObjectPooler>();
            }
            
            if (objectPooler == null)
            {
                Debug.LogError("WaveManager requires a MMMultipleObjectPooler component!");
                return;
            }
            
            ValidateSetup();
            
            if (TheKillsManager != null)
            {
                // Calculate the total number of enemies across all waves
                int totalEnemiesToKill = 0;
                foreach (var waveGO in waveGameObjects)
                {
                    totalEnemiesToKill += waveGO.GetComponent<WaveConfig>().spawnCount;
                }

                // Set the death threshold on the KillsManager
                TheKillsManager.DeathThreshold = totalEnemiesToKill;
                TheKillsManager.RefreshRemainingDeaths();
            }
            
            StartWaves();
            _perfManager = FindAnyObjectByType<ECSPerformanceManager>();
        }

        protected virtual void Update()
        {
            if (_isSpawning && Time.time >= _lastSpawnTime + _nextSpawnDelay)
            {
                SpawnEnemy();
            }
        }

        /// <summary>
        /// Start the wave sequence
        /// </summary>
        public virtual void StartWaves()
        {
            if (waveGameObjects == null || waveGameObjects.Count == 0)
            {
                Debug.LogError("No wave GameObjects assigned!");
                return;
            }
            
            CurrentWaveIndex = 0;
            StartCurrentWave();
        }

        /// <summary>
        /// Start the current wave
        /// </summary>
        protected virtual void StartCurrentWave()
        {
            if (CurrentWaveIndex >= waveGameObjects.Count)
            {
                OnAllWavesCompleted?.Invoke();
                return;
            }

            GameObject waveGO = waveGameObjects[CurrentWaveIndex];
            CurrentWaveConfig = waveGO.GetComponent<WaveConfig>();
            
            if (CurrentWaveConfig == null)
            {
                Debug.LogError($"Wave GameObject {waveGO.name} is missing WaveConfig component!");
                return;
            }

            _currentSpawnCount = 0;
            _lastSpawnTime = Time.time;
            _nextSpawnDelay = Random.Range(CurrentWaveConfig.spawnFrequencyMin, CurrentWaveConfig.spawnFrequencyMax);
            _isSpawning = true;
            
            OnWaveStarted?.Invoke(CurrentWaveIndex);
            
        }

        /// <summary>
        /// Spawn a single enemy
        /// </summary>
        protected virtual void SpawnEnemy()
        {
            if (CurrentWaveConfig == null) return;

            GameObject enemyPrefab = CurrentWaveConfig.GetRandomEnemyPrefab();
            if (enemyPrefab == null) return;

            // Get spawn position
            Transform spawnPoint = SpawnPoints[Random.Range(0, SpawnPoints.Count)];
            Vector3 spawnPosition = GetSpawnPosition(spawnPoint);

            // Pull from pool
            GameObject spawnedEnemy = objectPooler.GetPooledGameObjectOfType(enemyPrefab.name);
            if (spawnedEnemy == null) return;

            // OPTIMIZATION: Reset position BEFORE activating to avoid physics calculations
            spawnedEnemy.transform.position = spawnPosition;

            // Activate object
            spawnedEnemy.SetActive(true);

            // ---vvv--- NEW CODE TO ENSURE ENEMY IS FULLY FUNCTIONAL ---vvv---

            // Explicitly re-enable critical components to ensure they are active
            var brain = spawnedEnemy.GetComponent<AIBrain>();
            if (brain != null)
            {
                brain.enabled = true;
                brain.ResetBrain(); // Good practice to reset its state as well
            }

            var character = spawnedEnemy.GetComponent<Character>();
            if (character != null)
            {
                character.enabled = true;
                character.Reset(); // Use the Character's Reset method if it exists, or re-enable abilities individually.
        
                // You might need to re-enable specific abilities if they also get disabled
                foreach(var ability in character.CharacterAbilities)
                {
                    ability.enabled = true;
                }
            }
    
            var health = spawnedEnemy.GetComponent<Health>();
            if (health != null)
            {
                health.enabled = true;
                health.Revive();
            }
    
            // OPTIMIZATION: Only trigger callbacks if needed
            var poolable = spawnedEnemy.GetComponent<MMPoolableObject>();
            poolable?.TriggerOnSpawnComplete();

            // Register with performance manager
            _perfManager?.RegisterEnemy(spawnedEnemy);

            // Update counters
            UpdateWaveCounters();
        }
        
        private Vector3 GetSpawnPosition(Transform spawnPoint)
        {
            Vector3 basePosition = spawnPoint.position;
            if (CurrentWaveConfig.spawnRadius > 0f)
            {
                Vector2 randomOffset = Random.insideUnitCircle * CurrentWaveConfig.spawnRadius;
                basePosition += new Vector3(randomOffset.x, 0f, randomOffset.y);
            }
            return basePosition;
        }
        private void UpdateWaveCounters()
        {
            _currentSpawnCount++;
            _lastSpawnTime = Time.time;
            _nextSpawnDelay = Random.Range(CurrentWaveConfig.spawnFrequencyMin, CurrentWaveConfig.spawnFrequencyMax);

            // Complete wave if done
            if (_currentSpawnCount >= CurrentWaveConfig.spawnCount)
            {
                CompleteCurrentWave();
            }
        }


        /// <summary>
        /// Complete the current wave and start the next one
        /// </summary>
        protected virtual void CompleteCurrentWave()
        {
            _isSpawning = false;
            
            OnWaveCompleted?.Invoke(CurrentWaveIndex);
            CurrentWaveIndex++;
            
            if (CurrentWaveIndex < waveGameObjects.Count)
            {
                StartCoroutine(StartNextWaveAfterDelay());
            }
            else
            {
                OnAllWavesCompleted?.Invoke();
            }
        }

        /// <summary>
        /// Wait before starting the next wave
        /// </summary>
        protected virtual IEnumerator StartNextWaveAfterDelay()
        {
            yield return new WaitForSeconds(timeBetweenWaves);
            StartCurrentWave();
        }

        /// <summary>
        /// Validate the setup
        /// </summary>
        protected virtual void ValidateSetup()
        {
            if (SpawnPoints == null || SpawnPoints.Count == 0)
            {
                Debug.LogWarning("No spawn points assigned to WaveManager!");
            }

            if (waveGameObjects == null || waveGameObjects.Count == 0)
            {
                Debug.LogWarning("No wave GameObjects assigned to WaveManager!");
            }
        }
    }
}