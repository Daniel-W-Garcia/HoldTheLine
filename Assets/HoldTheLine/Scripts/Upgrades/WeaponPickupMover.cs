using System.Collections;
using DG.Tweening;
using MoreMountains.Tools;
using UnityEngine;

// Add this for MMPoolableObject

namespace HoldTheLine.Scripts.Upgrades
{
    public class WeaponPickupMover : MonoBehaviour
    {
        [Header("Movement Settings")]
        [Tooltip("Speed of movement towards destination")]
        public float moveSpeed = 5f;
        [Tooltip("Only move on -Z axis towards player")]
        public bool constrainToZAxis = true;

        [Header("Collision Detection")]
        [Tooltip("Distance to check for walls ahead")]
        public float wallCheckDistance = 0.5f;
        [Tooltip("Layer mask for walls/obstacles")]
        public LayerMask wallLayerMask = -1;
        [Tooltip("How often to check for walls (in seconds)")]
        public float wallCheckFrequency = 0.1f;

        [Header("Destination")]
        [Tooltip("Target destination object/collider to detect when reached")]
        public GameObject targetDestination;
        [Tooltip("Tag to identify destination objects")]
        public string destinationTag = "Player";
        [Tooltip("Distance threshold for destination detection")]
        public float destinationThreshold = 0.5f;

        [Header("Self Destruct")]
        [Tooltip("Time after reaching destination before self-destruct")]
        public float selfDestructDelay = 3f;
        [Tooltip("Enable self-destruct functionality")]
        public bool enableSelfDestruct = true;

        private bool pickedUp = false;
        private bool hasReachedDestination = false;
        private bool isBlocked = false;
        private bool shouldMove = false;
        private float lastWallCheckTime = 0f;
        private MMPoolableObject poolableObject; // Add reference to poolable object

        void OnEnable()
        {
            // Reset state when object is reactivated from pool
            pickedUp = false;
            hasReachedDestination = false;
            isBlocked = false;
            shouldMove = false;
            lastWallCheckTime = 0f;
        
            // Get the poolable object component
            if (poolableObject == null)
            {
                poolableObject = GetComponent<MMPoolableObject>();
            }
        
            StartCoroutine(InitializeMovement());
        }

        IEnumerator InitializeMovement()
        {
            // Wait one frame to ensure everything is initialized
            yield return null;
        
            SetupMovementTarget();
            shouldMove = true;
        }

        void SetupMovementTarget()
        {
            if (targetDestination != null)
            {
                return;
            }
        
            // Find destination by tag
            if (!string.IsNullOrEmpty(destinationTag))
            {
                GameObject destination = GameObject.FindGameObjectWithTag(destinationTag);
                if (destination != null)
                {
                    targetDestination = destination;
                }
                else
                {
                    Debug.LogWarning($"No destination found with tag '{destinationTag}'");
                }
            }
        
            if (targetDestination == null)
            {
                Debug.LogError($"No target destination set for {gameObject.name}!");
            }
        }

        void Update()
        {
            if (!shouldMove || hasReachedDestination || pickedUp || targetDestination == null)
                return;

            // Check for walls periodically
            if (Time.time - lastWallCheckTime > wallCheckFrequency)
            {
                CheckForWalls();
                lastWallCheckTime = Time.time;
            }

            // Only move if not blocked
            if (!isBlocked)
            {
                MoveTowardsTarget();
            }

            // Check if we've reached the destination
            CheckDestinationReached();
        }

        void CheckForWalls()
        {
            Vector3 direction = GetMovementDirection();
        
            // Cast a ray in the movement direction to check for walls
            RaycastHit hit;
            Vector3 rayOrigin = transform.position + Vector3.up * 0.1f; // Slightly above ground
        
            isBlocked = Physics.Raycast(rayOrigin, direction, out hit, wallCheckDistance, wallLayerMask);
        
            // Debug visualization
            Color rayColor = isBlocked ? Color.red : Color.green;
            Debug.DrawRay(rayOrigin, direction * wallCheckDistance, rayColor, wallCheckFrequency);
        
            if (isBlocked)
            {
                Debug.Log($"DestroyableWall detected: {hit.collider.name} at distance {hit.distance}");
            }
        }

        Vector3 GetMovementDirection()
        {
            Vector3 direction = (targetDestination.transform.position - transform.position).normalized;
        
            // Constrain to Z-axis only if enabled
            if (constrainToZAxis)
            {
                direction = new Vector3(0, 0, direction.z);
            }
        
            return direction;
        }

        void MoveTowardsTarget()
        {
            Vector3 direction = GetMovementDirection();
        
            // Move using transform
            transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);
        }

        void CheckDestinationReached()
        {
            float distanceToTarget = Vector3.Distance(transform.position, targetDestination.transform.position);
            if (distanceToTarget <= destinationThreshold)
            {
                OnDestinationReached();
            }
        }

        void OnDestinationReached()
        {
            if (hasReachedDestination) return;
        
            hasReachedDestination = true;
            shouldMove = false;
        
            // Start self-destruct timer
            if (enableSelfDestruct && !pickedUp)
            {
                StartCoroutine(SelfDestructTimer());
            }
        }

        // Public method that can be called when item is picked up
        public void OnItemPickedUp()
        {
            pickedUp = true;
            shouldMove = false;
            StopAllCoroutines();
        }

        // Method to call when a wall is destroyed - allows movement to resume
        public void OnWallDestroyed()
        {
            isBlocked = false;
            Debug.Log($"{gameObject.name} - DestroyableWall destroyed, resuming movement");
        }

        // Collision detection for trigger-based destinations
        void OnTriggerEnter(Collider other)
        {
            if (hasReachedDestination) return;
        
            if (targetDestination != null && other.gameObject == targetDestination)
            {
                OnDestinationReached();
            }
            else if (!string.IsNullOrEmpty(destinationTag) && other.CompareTag(destinationTag))
            {
                OnDestinationReached();
            }
        }

        IEnumerator SelfDestructTimer()
        {
            yield return new WaitForSeconds(selfDestructDelay);
        
            if (!pickedUp)
            {
                StartCoroutine(SelfDestructSequence());
            }
        }

        IEnumerator SelfDestructSequence()
        {
            transform.DOScale(Vector3.zero, 0.5f)
                .SetEase(Ease.InQuad)
                .OnComplete(() => ReturnToPool()); // Changed from Destroy to ReturnToPool
        
            yield return new WaitForSeconds(0.5f);
        }

        // New method to properly return object to pool
        void ReturnToPool()
        {
            // Reset the scale back to original before returning to pool
            transform.localScale = Vector3.one;
        
            if (poolableObject != null)
            {
                // Use the poolable object's destroy method which returns it to the pool
                poolableObject.Destroy();
            }
            else
            {
                // Fallback - just deactivate the object
                gameObject.SetActive(false);
            }
        }

        void OnDisable()
        {
            transform.DOKill();
            StopAllCoroutines();
            shouldMove = false;
        
            // Reset scale when disabled (in case it was scaled down)
            transform.localScale = Vector3.one;
        }

        void OnDestroy()
        {
            transform.DOKill();
        }

        // Debug visualization
        void OnDrawGizmosSelected()
        {
            if (targetDestination != null)
            {
                // Draw line to target
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, targetDestination.transform.position);
                Gizmos.DrawWireSphere(targetDestination.transform.position, destinationThreshold);
            
                // Show movement direction
                Vector3 direction = GetMovementDirection();
                Gizmos.color = isBlocked ? Color.red : Color.blue;
                Gizmos.DrawRay(transform.position, direction * 2f);
            }
        
            // Show wall detection ray
            Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
            Vector3 direction2 = GetMovementDirection();
            Gizmos.color = isBlocked ? Color.red : Color.yellow;
            Gizmos.DrawRay(rayOrigin, direction2 * wallCheckDistance);
        }
    }
}