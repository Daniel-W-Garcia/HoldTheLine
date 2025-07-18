using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using UnityEngine;

namespace HoldTheLine.Scripts.AI.AIDecisions
{
    /// <summary>
    /// This decision returns true if the Brain's current target has a Health component and is not dead.
    /// It's optimized to cache the Character component for performance.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AI Decision Target Is Alive (Cached ref)")]
    public class AIDecisionTargetIsAlive : AIDecision
    {
        protected Character _targetCharacter;
        protected Health _targetHealth; // Even better, we can check the Health component directly.
        protected Transform _lastTarget;

        /// <summary>
        /// On Decide we check whether the Target is alive or dead
        /// </summary>
        public override bool Decide()
        {
            return CheckIfTargetIsAlive();
        }

        /// <summary>
        /// Returns true if the Brain's Target is alive, false otherwise.
        /// Caches the Character/Health component to avoid repeated GetComponent calls on the same target.
        /// </summary>
        protected virtual bool CheckIfTargetIsAlive()
        {
            if (_brain.Target == null)
            {
                return false;
            }

            // If the target has changed since the last check, we need to find its components again.
            if (_brain.Target != _lastTarget)
            {
                _lastTarget = _brain.Target;
                _targetHealth = _brain.Target.GetComponent<Health>();
            }

            // If we have a cached reference to the target's health, check its state.
            if (_targetHealth != null)
            {
                // This is a direct and clear check. If CurrentHealth is above 0, they're alive.
                return _targetHealth.CurrentHealth > 0;
            }

            // If the target doesn't have a Health component, we can't determine if it's alive.
            return false;
        }

        /// <summary>
        /// When the brain's target is reset, we should clear our cached components.
        /// </summary>
        public override void OnExitState()
        {
            base.OnExitState();
            _lastTarget = null;
            _targetHealth = null;
        }
    }
}