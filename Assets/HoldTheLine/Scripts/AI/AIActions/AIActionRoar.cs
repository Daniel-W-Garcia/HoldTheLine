using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using UnityEngine;

namespace HoldTheLine.Scripts.AI.AIActions
{
    [AddComponentMenu("TopDown Engine/Custom/AIActionRoar")]
    public class AIActionRoar : AIAction
    {
        [Tooltip("Name of the Animator trigger parameter for Roar")]
        public string RoarTrigger = "Roar";

        [Tooltip("The name of the state to transition to after the roar is complete")]
        public string ChaseState = "Chase";
        
        [Header("Feedbacks")]
        [Tooltip("The feedback to play when entering the roar state")]
        public MMFeedbacks RoarEnterFeedback; 

        protected Animator _animator;
        protected CharacterMovement _movement;
        protected bool _hasRoared;

        public override void Initialization()
        {
            base.Initialization();
            _animator = this.gameObject.GetComponentInChildren<Animator>();
            _movement = this.gameObject.GetComponent<CharacterMovement>();
        }
        public override void OnEnterState()
        {
            base.OnEnterState();
            _hasRoared = false;
            RoarEnterFeedback?.PlayFeedbacks();
        }

        public override void PerformAction()
        {
            if (_animator == null) return;
            if (_hasRoared) return;
            
            // Stop movement while roaring
            if (_movement != null)
            {
                _movement.MovementForbidden = true;
            }
            
            // Trigger the roar animation
            _animator.SetTrigger(RoarTrigger);
            _hasRoared = true;
            Debug.Log($"Roar Trigger flipped {RoarTrigger}");
        }

        /// <summary>
        /// This method should be called by an AnimationEvent on the last frame of the roar animation clip.
        /// </summary>
        public void OnRoarComplete()
        {
            // Re-enable movement
            if (_movement != null)
            {
                _movement.MovementForbidden = false;
            }
    
            // Switch back to the specified state
            _brain.TransitionToState(ChaseState);
        }
    }
}