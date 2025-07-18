using HoldTheLine.Scripts.AI.AIActions;
using UnityEngine;

// This script acts as a proxy to forward Animation Events from the model
// to the logic scripts on the parent/root object.
namespace HoldTheLine.Scripts.AI
{
    public class AnimationEventProxy : MonoBehaviour
    {
        // We will find the AIActionRoar script on the parent GameObject
        private AIActionRoar _roarAction;

        void Awake()
        {
            // Find the script in the parent hierarchy when the game starts
            _roarAction = GetComponentInParent<AIActionRoar>();
        }

        public void OnRoarAnimationStart()
        {
            
        }

        // THIS is the public function that your Animation Event will call.
        public void OnRoarAnimationComplete()
        {
            // If we found the roar action script, call its public method.
            if (_roarAction != null)
            {
                _roarAction.OnRoarComplete();
            }
            else
            {
                // A helpful error in case the script can't be found.
                Debug.LogWarning("AnimationEventProxy could not find AIActionRoar in parent.", this.gameObject);
            }
        }
    }
}