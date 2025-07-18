using MoreMountains.TopDownEngine;
using Pathfinding;
using Pathfinding.RVO;
using UnityEngine;

namespace HoldTheLine.Scripts.AI.Pathfinding 
{
    /// <summary>
    /// A TDE CharacterPathfinder ability that uses the A* Pathfinding Project to move.
    /// It requires a Seeker and an RVOController on the same GameObject.
    /// This version is optimized for performance, stability, and clarity.
    /// </summary>
    public class CharacterPathfinder3DAStar : CharacterPathfinder3D
    {
        [Header("A* Pathfinder Settings")]
        [Tooltip("How often (in seconds) the agent will request a new path to the target. Lower is more responsive but less performant.")]
        public float PathUpdateFrequency = 0.25f;
        [Tooltip("The radius from the final destination at which the agent will begin to slow down.")]
        public float SlowRadius = 3f;
        [Tooltip("The radius from the final destination at which the agent will stop completely.")]
        public float StopRadius = 1f;
        [Tooltip("Should debug messages be logged to the console?")]
        public bool ShowDebugMessages;

        private RVOController _rvocontroller;
        private Seeker _seeker;
        
        private float _timeOfNextPathUpdate;
        private float _slowRadiusSqr;
        private float _maxMovementSpeed;

        private void CacheComponents()
        {
            _seeker = GetComponent<Seeker>();
            _rvocontroller = GetComponent<RVOController>();
            _slowRadiusSqr = SlowRadius * SlowRadius;
            UpdateMaxMovementSpeed();
        }

        protected override void Awake()
        {
            base.Awake();
            CacheComponents();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            CacheComponents();
        }

        public Vector3[] GetWaypoints()
        {
            return Waypoints;
        }

        /// <summary>
        /// Requests a new path from the Seeker on a set frequency.
        /// </summary>
        protected override void DeterminePath(Vector3 start, Vector3 end, bool ignoreDelay = false)
        {
            if (_seeker == null || Target == null) return;              // guard
            if (Time.time < _timeOfNextPathUpdate && !ignoreDelay) return;

            _timeOfNextPathUpdate = Time.time + PathUpdateFrequency;
            _seeker.StartPath(start, end, OnPathComplete);
        }

        /// <summary>
        /// Called when the Seeker has calculated a path.
        /// </summary>
        public void OnPathComplete(Path p)
        {
            if (p.error)
            {
                if (ShowDebugMessages) Debug.LogWarning("No Valid Path Found for " + this.name, this.gameObject);
                Waypoints = null; // Clear the old path to stop movement
                return;
            }

            Waypoints = p.vectorPath.ToArray();
            _waypoints = Waypoints.Length;
            // Start moving towards the second waypoint, as the first is the agent's current position.
            NextWaypointIndex = (_waypoints >= 2) ? 1 : 0;
        }

        /// <summary>
        /// Uses the RVOController to calculate movement and passes it to the CharacterMovement ability.
        /// </summary>
        protected override void MoveController()
        {
            if ((Waypoints == null) || (NextWaypointIndex < 1) || (NextWaypointIndex >= _waypoints))
            {
                DeterminePath(transform.position, Target.position, true);  // force new path now
                return;
            }

            // --- FIX: More robust checks for a valid state before attempting to move ---
            if (Target == null || Waypoints == null || NextWaypointIndex <= 0 || NextWaypointIndex >= _waypoints)
            {
                _characterMovement.SetMovement(Vector2.zero);
                // Set a zero velocity target for the RVO controller to signal it has stopped.
                if (_rvocontroller != null)
                {
                    _rvocontroller.SetTarget(transform.position, 0f, 0f, Vector3.positiveInfinity);
                }
                return;
            }

            UpdateMaxMovementSpeed();

            // --- FIX: Clearer speed calculation and target setting for the RVOController ---
            var distanceToFinalDestinationSqr = (transform.position - Target.position).sqrMagnitude;
            float targetSpeed;

            if (distanceToFinalDestinationSqr <= StopRadius * StopRadius)
            {
                targetSpeed = 0.0f;
            }
            else if (distanceToFinalDestinationSqr > _slowRadiusSqr)
            {
                targetSpeed = _maxMovementSpeed;
            }
            else
            {
                // Smoothly interpolate speed inside the slow radius
                targetSpeed = _maxMovementSpeed * Mathf.Sqrt(distanceToFinalDestinationSqr) / SlowRadius;
            }

            var nextWaypointPosition = Waypoints[NextWaypointIndex];
            _rvocontroller.SetTarget(nextWaypointPosition, targetSpeed, _maxMovementSpeed, Vector3.positiveInfinity);
            
            var movementDelta = _rvocontroller.CalculateMovementDelta(transform.position, Time.deltaTime);

            // --- FIX: Added safety checks and clarity for calculating final movement direction ---
            if (Time.deltaTime > 0 && _maxMovementSpeed > 0)
            {
                // We derive a normalized direction vector to feed into the TopDownController,
                // which is what CharacterOrientation and other abilities expect.
                _direction = movementDelta / (Time.deltaTime * _maxMovementSpeed);
                _newMovement.x = _direction.x;
                _newMovement.y = _direction.z;
                _characterMovement.SetMovement(_newMovement);
            }
            else
            {
                _characterMovement.SetMovement(Vector2.zero);
            }
        }
        
        /// <summary>
        /// Updates the max movement speed value from the CharacterMovement component.
        /// This should be called regularly in case speed modifiers change.
        /// </summary>
        private void UpdateMaxMovementSpeed()
        {
            if (_characterMovement == null) return;
            // Use the base MovementSpeed, as multipliers are handled by the TopDownController
            _maxMovementSpeed = _characterMovement.MovementSpeed;
        }
    }
}