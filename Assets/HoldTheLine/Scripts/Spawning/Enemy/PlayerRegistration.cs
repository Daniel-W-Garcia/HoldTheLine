using HoldTheLine.Scripts.AI.Pathfinding;
using UnityEngine;

namespace HoldTheLine.Scripts.Spawning.Enemy
{
    /// <summary>
    /// Helper component to register the player with the AI targeting system.
    /// Attach this to your player character or call the static methods from your spawn system.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Player Registration")]
    public class PlayerRegistration : MonoBehaviour
    {
        
        [Header("Auto-Registration")]
        [Tooltip("Whether to automatically register this player on Start.")]
        public bool AutoRegisterOnStart = true;
    
        [Tooltip("Whether to automatically unregister this player on destruction.")]
        public bool AutoUnregisterOnDestroy = true;

        private void Start()
        {
            if (AutoRegisterOnStart)
            {
                AIFindPayerTargetOnEnable.RegisterPlayer(this.transform);
            }
        }

        private void OnDestroy()
        {
            if (AutoUnregisterOnDestroy)
            {
                AIFindPayerTargetOnEnable.UnregisterPlayer();
            }
        }

        /// <summary>
        /// Manually register this player
        /// </summary>
        [ContextMenu("Register Player")]
        public void RegisterPlayer()
        {
            AIFindPayerTargetOnEnable.RegisterPlayer(this.transform);
        }

        /// <summary>
        /// Manually unregister the player
        /// </summary>
        [ContextMenu("Unregister Player")]
        public void UnregisterPlayer()
        {
            AIFindPayerTargetOnEnable.UnregisterPlayer();
        }
    }
}