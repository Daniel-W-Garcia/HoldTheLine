using MoreMountains.Tools;
using UnityEngine;

namespace HoldTheLine.Scripts.AI
{
    public class AIAnimatorAttackRadius : MonoBehaviour
    {
        [Tooltip("Match this to your AIDecisionDistanceToTarget Distance value")]
        public float AttackRadius = 3f;

        protected AIBrain _brain;
        protected int _withinRadiusToHash;
        protected int _distanceToTargetToHash;
        [SerializeField] Animator _animator;


        void Awake()
        {
            _brain = GetComponent<AIBrain>();

            // find Animator on this object or any child
        
            if (_animator == null)
            {
                Debug.LogError($"{name} couldn’t find an Animator in children.");
                enabled = false;
                return;
            }
            _withinRadiusToHash = Animator.StringToHash("WithinAttackRadius");
            _distanceToTargetToHash = Animator.StringToHash("DistanceToTarget");
        }

        void Update()
        {
            if (_brain.Target == null)
            {
                _animator.SetBool(_withinRadiusToHash, false);
                return;
            }

            float sqrDist = (transform.position - _brain.Target.position).sqrMagnitude;
            bool within = sqrDist <= AttackRadius * AttackRadius;
            _animator.SetBool(_withinRadiusToHash, within);
            _animator.SetFloat(_distanceToTargetToHash, sqrDist);
        }
    }
}