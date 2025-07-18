using System.Collections;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using Pathfinding;
using UnityEngine;

namespace HoldTheLine.Scripts.AI.AIActions
{
    /// <summary>
    /// An Action that performs melee attacks using DamageOnTouch for reliable damage dealing
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AI Action Melee Attack 3D")]
    public class AIActionMeleeAttack3D : AIAction
    {
        [Header("Targeting")]
        /// if true the Character will face the target when attacking
        [Tooltip("if true the Character will face the target when attacking")]
        public bool FaceTarget = true;
        /// the maximum distance at which the zombie can perform a melee attack
        [Tooltip("the maximum distance at which the zombie can perform a melee attack")]
        public float AttackRange = 1.5f;
        /// if this is set to true, vertical aim will be locked to remain horizontal
        [Tooltip("if this is set to true, vertical aim will be locked to remain horizontal")]
        public bool LockVerticalAim = true;

        [Header("Attack Settings")]
        /// the time between attacks
        [Tooltip("the time between attacks")]
        public float TimeBetweenAttacks = 1f;
        /// the name of the attack animation trigger parameter
        [Tooltip("the name of the attack animation trigger parameter")]
        public string AttackAnimationTrigger = "Attack";

        [Header("Damage Hitbox")]
        /// the damage hitbox gameobject (should have DamageOnTouch component)
        [Tooltip("the damage hitbox gameobject (should have DamageOnTouch component)")]
        public GameObject DamageHitbox;
        /// delay before activating damage hitbox (sync with animation)
        [Tooltip("delay before activating damage hitbox (sync with animation)")]
        public float DamageDelay = 0.2f;
        /// how long the damage hitbox stays active
        [Tooltip("how long the damage hitbox stays active")]
        public float DamageActiveDuration = 0.3f;

        [Header("Effects")]
        /// feedbacks to play when attacking
        [Tooltip("feedbacks to play when attacking")]
        public MMF_Player AttackFeedbacks;

        protected CharacterOrientation3D _orientation3D;
        protected Character _character;
        protected Animator _animator;
        protected Vector3 _directionToTarget;
        protected int _numberOfAttacks = 0;
        protected bool _attacking = false;
        protected float _lastAttackTime;
        protected int _attackAnimationTriggerID;
        protected int _withinAttackRadiusAnimationParameter;

        /// <summary>
        /// On init we grab components and setup animator parameters
        /// </summary>
        public override void Initialization()
        {
            if(!ShouldInitialize) return;
            base.Initialization();
            
            _character = GetComponentInParent<Character>();
            _orientation3D = _character?.FindAbility<CharacterOrientation3D>();
            _animator = _character?.CharacterAnimator;
            
            // Setup animator parameters
            if (_animator != null && !string.IsNullOrEmpty(AttackAnimationTrigger))
            {
                _attackAnimationTriggerID = Animator.StringToHash(AttackAnimationTrigger);
            }
            
            if (_animator != null)
            {
                MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, "WithinAttackRadius", 
                    out _withinAttackRadiusAnimationParameter, AnimatorControllerParameterType.Bool, 
                    _character._animatorParameters);
            }

            // Find damage hitbox if not assigned
            if (DamageHitbox == null)
            {
                DamageHitbox = transform.Find("AttackHitbox")?.gameObject;
            }

            // Make sure damage hitbox starts disabled
            if (DamageHitbox != null)
            {
                DamageHitbox.SetActive(false);
            }
        }

        /// <summary>
        /// On PerformAction we check range, face target, and attack
        /// </summary>
        public override void PerformAction()
        {
            // Ensure A* stops trying to move during attack
            var follower = GetComponent<FollowerEntity>();
            if (follower != null)
            {
                follower.isStopped = true;
            }
    
            // Rest of your attack logic...
            if (!IsTargetInRange())
            {
                return;
            }
    
            TestFaceTarget();
            Attack();
        }

        /// <summary>
        /// Update facing direction and animator parameters
        /// </summary>
        protected virtual void Update()
        {
            // Handle facing
            if (_attacking && FaceTarget && _orientation3D != null)
            {
                if (LockVerticalAim)
                {
                    _directionToTarget.y = 0;
                }
                _orientation3D.ForcedRotation = true;
                _orientation3D.ForcedRotationDirection = _directionToTarget;
            }

            // Update animator bool parameter
            bool inRange = IsTargetInRange();
            if (_animator != null)
            {
                MMAnimatorExtensions.UpdateAnimatorBool(_animator, _withinAttackRadiusAnimationParameter, 
                    inRange, _character._animatorParameters, _character.RunAnimatorSanityChecks);
            }
        }

        /// <summary>
        /// Checks if the target is within attack range
        /// </summary>
        protected virtual bool IsTargetInRange()
        {
            if (_brain.Target == null)
            {
                return false;
            }

            Vector3 targetPosition = _brain.Target.position;
            float distance = Vector3.Distance(_character.transform.position, targetPosition);
            _directionToTarget = (targetPosition - _character.transform.position).normalized;
            
            return distance <= AttackRange;
        }

        /// <summary>
        /// Sets up facing direction
        /// </summary>
        protected virtual void TestFaceTarget()
        {
            if (!FaceTarget || (_brain.Target == null))
            {
                return;
            }

            if (LockVerticalAim)
            {
                _directionToTarget.y = 0;
            }

            _attacking = true;
        }

        /// <summary>
        /// Triggers the attack if cooldown allows
        /// </summary>
        protected virtual void Attack()
        {
            if (_numberOfAttacks < 1)
            {
                if (Time.time >= _lastAttackTime + TimeBetweenAttacks)
                {
                    StartAttack();
                    _numberOfAttacks++;
                }
                else
                {
                    Debug.Log("[AIActionMeleeAttack3D] Attack on cooldown");
                }
            }
        }

        /// <summary>
        /// Starts the attack sequence
        /// </summary>
        protected virtual void StartAttack()
        {
            _lastAttackTime = Time.time;

            // Trigger attack animation
            if (_animator != null && _attackAnimationTriggerID != 0)
            {
                _animator.SetTrigger(_attackAnimationTriggerID);
            }
            else
            {
                Debug.LogWarning($"[AIActionMeleeAttack3D] Cannot fire animation trigger! Animator: {_animator != null}, TriggerID: {_attackAnimationTriggerID}");
            }

            // Play attack feedbacks
            AttackFeedbacks?.PlayFeedbacks();

            // Handle damage hitbox timing
            StartCoroutine(AttackSequence());
        }

        /// <summary>
        /// Handles the attack sequence with DamageOnTouch activation
        /// </summary>
        protected virtual IEnumerator AttackSequence()
        {
            // Wait for damage delay
            yield return new WaitForSeconds(DamageDelay);

            // Activate damage hitbox
            if (DamageHitbox != null)
            {
                DamageHitbox.SetActive(true);
            }
            else
            {
                Debug.LogError("[AIActionMeleeAttack3D] DamageHitbox is null!");
            }

            // Keep hitbox active for specified duration
            yield return new WaitForSeconds(DamageActiveDuration);

            // Deactivate damage hitbox
            if (DamageHitbox != null)
            {
                DamageHitbox.SetActive(false);
            }
        }

        /// <summary>
        /// Reset attack counter when entering state
        /// </summary>
        public override void OnEnterState()
        {
            base.OnEnterState();
            _numberOfAttacks = 0;
        }

        /// <summary>
        /// Clean up when exiting state
        /// </summary>
        public override void OnExitState()
        {
            base.OnExitState();
            
            var follower = GetComponent<FollowerEntity>();
            if (follower != null)
            {
                follower.isStopped = false;
            }
            _attacking = false;
            StopAllCoroutines();
            
            // Disable forced rotation
            if (_orientation3D != null)
            {
                _orientation3D.ForcedRotation = false;
            }

            // Make sure damage hitbox is disabled
            if (DamageHitbox != null)
            {
                DamageHitbox.SetActive(false);
            }
        }

        /// <summary>
        /// Draw attack range gizmo
        /// </summary>
        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, AttackRange);
            
            if (_brain != null && _brain.Target != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, _brain.Target.position);
            }
        }
    }
}