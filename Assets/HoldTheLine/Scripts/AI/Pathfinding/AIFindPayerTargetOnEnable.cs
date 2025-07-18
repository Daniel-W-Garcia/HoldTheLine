using System.Collections;
using MoreMountains.Tools;
using UnityEngine;

namespace HoldTheLine.Scripts.AI.Pathfinding
{
    /// <summary>
    /// This component finds a target for the AIBrain every time the character is enabled.
    /// This is crucial for working with object poolers, ensuring recycled enemies
    /// correctly re-acquire their target upon spawning.
    /// Updated for Unity 6 with proper object pooling support and delayed target acquisition.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AI Find Target On Enable")]
    [RequireComponent(typeof(AIBrain))]
    public class AIFindPayerTargetOnEnable : MonoBehaviour
    {
        [Header("Targeting")]
        [Tooltip("The tag to look for when searching for a target (typically 'Player').")]
        public string TargetTag = "Player";
    
        [Header("Timing")]
        [Tooltip("Maximum time to wait for a target to be found (in seconds).")]
        public float MaxWaitTime = 3f;
    
        [Tooltip("How often to check for a target when not found (in seconds).")]
        public float CheckInterval = 0.1f;
    
        [Header("Debug")]
        [Tooltip("Whether to show debug logs.")]
        public bool ShowDebugLogs = false;

        private AIBrain _brain;
        private Coroutine _targetSearchCoroutine;
    
        // Static reference to the player for performance optimization
        private static Transform _cachedPlayerTransform;
        private static bool _playerCacheValid;

        /// <summary>
        /// Static method to register a player transform for all enemies to use
        /// Call this from your player spawn script
        /// </summary>
        public static void RegisterPlayer(Transform playerTransform)
        {
            _cachedPlayerTransform = playerTransform;
            _playerCacheValid = true;
        
            // Notify all existing AIFindPayerTargetOnEnable components
            var allTargetFinders = FindObjectsByType<AIFindPayerTargetOnEnable>(FindObjectsSortMode.None);
            foreach (var finder in allTargetFinders)
            {
                if (finder._brain != null && finder._brain.Target == null)
                {
                    finder.SetTarget(playerTransform);
                }
            }
        }

        /// <summary>
        /// Static method to clear the player cache (call when player is destroyed)
        /// </summary>
        public static void UnregisterPlayer()
        {
            _cachedPlayerTransform = null;
            _playerCacheValid = false;
        }

        /// <summary>
        /// On Awake, we get a reference to the AIBrain for efficiency.
        /// </summary>
        protected virtual void Awake()
        {
            _brain = GetComponent<AIBrain>();
            if (_brain == null)
            {
                Debug.LogError($"AIFindPayerTargetOnEnable on {gameObject.name} could not find AIBrain component!");
            }
        }

        /// <summary>
        /// OnEnable is called every time the GameObject is set to active.
        /// This is perfect for object pooling from your WaveManager.
        /// </summary>
        protected virtual void OnEnable()
        {
            if (_brain == null) return;

            // Stop any existing search coroutine
            if (_targetSearchCoroutine != null)
            {
                StopCoroutine(_targetSearchCoroutine);
            }

            // Always reset the target to null when enabled to ensure fresh targeting
            _brain.Target = null;

            // Start the target search process
            _targetSearchCoroutine = StartCoroutine(FindTargetCoroutine());
        }

        /// <summary>
        /// Clean up when disabled
        /// </summary>
        protected virtual void OnDisable()
        {
            if (_targetSearchCoroutine != null)
            {
                StopCoroutine(_targetSearchCoroutine);
                _targetSearchCoroutine = null;
            }
        }

        /// <summary>
        /// Coroutine to find and assign a target with retry logic
        /// </summary>
        protected virtual IEnumerator FindTargetCoroutine()
        {
            float startTime = Time.time;
        
            while (Time.time - startTime < MaxWaitTime)
            {
                Transform target = FindTarget();
            
                if (target != null)
                {
                    SetTarget(target);
                    yield break; // Successfully found target, exit coroutine
                }
            
                // Wait before trying again
                yield return new WaitForSeconds(CheckInterval);
            }
        
            // If we reach here, we couldn't find a target within the time limit
            if (ShowDebugLogs)
            {
                Debug.LogWarning($"{gameObject.name} could not find a target with tag '{TargetTag}' within {MaxWaitTime} seconds.");
            }
        }

        /// <summary>
        /// Find the target using multiple methods
        /// </summary>
        protected virtual Transform FindTarget()
        {
            // First, try to use the cached player reference for performance
            if (_playerCacheValid && _cachedPlayerTransform != null)
            {
                return _cachedPlayerTransform;
            }

            // Fallback to finding by tag (less efficient but more flexible)
            GameObject targetObject = GameObject.FindGameObjectWithTag(TargetTag);
            if (targetObject != null)
            {
                // Cache the result for future use
                if (TargetTag == "Player")
                {
                    RegisterPlayer(targetObject.transform);
                }
                return targetObject.transform;
            }

            return null;
        }

        /// <summary>
        /// Set the target on the brain
        /// </summary>
        // In your SetTarget method, add this:
        protected virtual void SetTarget(Transform target)
        {
            if (_brain != null && target != null)
            {
                _brain.Target = target;
        
                // Also set the pathfinder target
                var pathfinder = GetComponent<CharacterPathfinder3DAStar>();
                if (pathfinder != null)
                {
                    pathfinder.SetNewDestination(target, true);
                }
        
                if (ShowDebugLogs)
                {
                    Debug.Log($"{gameObject.name} successfully found and assigned target: {target.name}");
                }
            }
        }


        /// <summary>
        /// Manual method to force target finding (can be called from other scripts)
        /// </summary>
        [ContextMenu("Force Find Target")]
        public virtual void ForceFind()
        {
            if (_targetSearchCoroutine != null)
            {
                StopCoroutine(_targetSearchCoroutine);
            }
        
            _targetSearchCoroutine = StartCoroutine(FindTargetCoroutine());
        }

        /// <summary>
        /// Check if we currently have a valid target
        /// </summary>
        public virtual bool HasValidTarget()
        {
            return _brain != null && _brain.Target != null;
        }

        /// <summary>
        /// Get the current target (null if none)
        /// </summary>
        public virtual Transform GetCurrentTarget()
        {
            return _brain?.Target;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor validation
        /// </summary>
        protected virtual void OnValidate()
        {
            if (MaxWaitTime <= 0f)
            {
                MaxWaitTime = 3f;
            }
        
            if (CheckInterval <= 0f)
            {
                CheckInterval = 0.1f;
            }
        }
#endif
    }
}