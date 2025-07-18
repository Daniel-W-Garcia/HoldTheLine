using System.Collections.Generic;
using MoreMountains.TopDownEngine;
using UnityEngine;

namespace HoldTheLine.Scripts.Spawning.Enemy
{
    /// <summary>
    /// Pool-optimized Character class that prevents expensive re-initialization
    /// </summary>
    public class PoolOptimizedCharacter : Character
    {
        // Static cache to track which GameObjects have been fully initialized
        private static readonly Dictionary<GameObject, bool> _initializationFlags = new Dictionary<GameObject, bool>();
    
        // Instance flag for quick checks
        private bool _isFullyInitialized = false;

        protected override void Awake()
        {
            // Check if this specific GameObject has been fully initialized before
            if (_initializationFlags.ContainsKey(gameObject) && _initializationFlags[gameObject])
            {
                _isFullyInitialized = true;
                Debug.Log($"[OPTIMIZATION] {gameObject.name} quickly reactivated from pool");
            
                // Do minimal reactivation setup
                QuickReactivation();
                return;
            }
        
            // First time initialization - run the full setup
            Debug.Log($"[OPTIMIZATION] {gameObject.name} is being initialized for the VERY FIRST TIME");
            base.Awake();
        
            // Mark as initialized
            _initializationFlags[gameObject] = true;
            _isFullyInitialized = true;
        }

        /// <summary>
        /// Lightweight setup when reactivating from pool
        /// </summary>
        private void QuickReactivation()
        {
            // Reset essential state without expensive operations
            if (MovementState != null)
            {
                MovementState.ChangeState(CharacterStates.MovementStates.Idle);
            }
        
            if (ConditionState != null)
            {
                ConditionState.ChangeState(CharacterStates.CharacterConditions.Normal);
            }
        
            // Reset controller without full reinitialization
            if (_controller != null)
            {
                _controller.Reset();
            }
        
            // Reset health without expensive setup
            if (CharacterHealth != null)
            {
                CharacterHealth.Revive();
            }
        
            // Reset brain state if AI
            if (CharacterBrain != null)
            {
                CharacterBrain.ResetBrain();
            }
        }
    
        /// <summary>
        /// Override initialization to make it idempotent
        /// </summary>
        protected override void Initialization()
        {
            // Only run expensive initialization once per GameObject
            if (_isFullyInitialized)
            {
                return;
            }
        
            base.Initialization();
        }
    
        /// <summary>
        /// Clean up static references when disabled (when returned to pool)
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();
        
            // Note: We don't remove from _initializationFlags here because
            // we want to remember that this GameObject was initialized
            // Only clean up if the GameObject is actually being destroyed
        }
    
        /// <summary>
        /// Clean up static references when the object is actually destroyed
        /// </summary>
        void OnDestroy()
        {
            // Clean up static cache to prevent memory leaks
            if (_initializationFlags.ContainsKey(gameObject))
            {
                _initializationFlags.Remove(gameObject);
            }
        }
    }
}