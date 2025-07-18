using MoreMountains.Tools;
using Pathfinding;
using UnityEngine;

namespace HoldTheLine.Scripts.AI.AIDecisions
{
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AI Decision Agent Stopped")]
    public class AIDecisionAgentStopped : AIDecision
    {
        [Header("Settings")]
        [Tooltip("Optional: validate we're close enough to target")]
        public bool ValidateDistance = true;
        [Tooltip("Max distance to consider valid (should match A* stopDistance)")]
        public float MaxDistance = 2f;
        
        protected IAstarAI _agent;
        protected FollowerEntity _followerEntity;
        
        public override void Initialization()
        {
            base.Initialization();
            _agent = this.gameObject.GetComponent<IAstarAI>();
            _followerEntity = _agent as FollowerEntity;
            
            if (_agent == null)
            {
                Debug.LogError($"[AIDecisionAgentStopped] No IAstarAI found on {gameObject.name}");
            }
        }

        public override bool Decide()
        {
            if (_agent == null) return false;
            
            // First check: A* says we're stopped
            bool stopped = _agent.isStopped;
            
            if (!stopped || !ValidateDistance) return stopped;
            
            // Optional validation: ensure target is actually in range
            if (_brain.Target != null)
            {
                float dist = Vector3.Distance(transform.position, _brain.Target.position);
                return dist <= MaxDistance;
            }
            
            return stopped;
        }
    }
}