using MoreMountains.TopDownEngine;
using Pathfinding;
using UnityEngine;

namespace HoldTheLine.Scripts.AI
{
    [AddComponentMenu("TopDown Engine/Character/AI/Abilities/ECS Pathfinder Movement")]
    public class ECSPathfinderMovement : CharacterAbility
    {
        [Header("A* Settings")]
        [Tooltip("The A* FollowerEntity component that drives this character's pathfinding.")]
        public FollowerEntity Follower;
        
        protected IAstarAI _agent;

        /// <summary>
        /// On Initialization, we get the IAstarAI agent from the FollowerEntity.
        /// </summary>
        protected override void Initialization()
        {
            base.Initialization();
            if (Follower == null)
            {
                Debug.LogError("[ECSPathfinderMovement] The FollowerEntity field has not been assigned in the inspector.", gameObject);
                return;
            }
            _agent = Follower as IAstarAI;
        }

        /// <summary>
        /// Every frame, we process the ability to move the character based on the A* agent's final, avoidance-adjusted velocity.
        /// </summary>
        public override void ProcessAbility()
        {
            // We perform safety checks to ensure everything is set up correctly.
            if (!AbilityAuthorized || _agent == null || _controller == null)
            {
                return;
            }

            // If the A* agent has reached its destination or is manually stopped, we tell the character to stop moving.
            if (_agent.isStopped || _agent.reachedEndOfPath)
            {
                _controller.SetMovement(Vector3.zero);
                return;
            }

            // --- THIS IS THE KEY CHANGE ---
            // We now read _agent.velocity instead of _agent.desiredVelocity.
            // .desiredVelocity is the raw path direction.
            // .velocity is the final, calculated velocity after the RVOSimulator has adjusted it for local avoidance.
            // This gives us the "swerving" motion needed to avoid other agents.
            Vector3 worldDirection = _agent.velocity;
            
            // We still ensure movement is on the horizontal plane. This is correct.
            worldDirection.y = 0f;

            // We feed this final, avoidance-adjusted direction into the TopDownController.
            _controller.SetMovement(worldDirection.normalized);
        }
    }
}