using MoreMountains.Tools;
using UnityEngine;

namespace HoldTheLine.Scripts.AI.AIDecisions
{
    [AddComponentMenu("TopDown Engine/Custom/AIDecisionCrossLine")]
    public class AIDecisionCrossLine : AIDecision
    {
        [Tooltip("Defines the plane: position = origin, forward = normal")]
        public Transform PlaneTransform;
        [Tooltip("Tag of the scene object to auto-find if no transform is assigned")]
        public string PlaneTag = "RoarPlane";

        protected bool _hasCrossed;

        public override void OnEnterState()
        {
            base.OnEnterState();
            _hasCrossed = false;
            if (PlaneTransform == null && !string.IsNullOrEmpty(PlaneTag))
            {
                var go = GameObject.FindWithTag(PlaneTag);
                if (go != null) PlaneTransform = go.transform;
            }
        }

        public override bool Decide()
        {
            if (PlaneTransform == null) return false;

            // signed distance from agent to plane
            float d = Vector3.Dot(transform.position - PlaneTransform.position, PlaneTransform.forward);

            if (!_hasCrossed && d > 0f)
            {
                _hasCrossed = true;   // only fire once per crossing
                return true;
            }
            return false;
        }
    }
}