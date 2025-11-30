using UnityEngine;
using KinematicCharacterController;
using CrazyRooftop.Player;

namespace CrazyRooftop.Environment
{
    /// <summary>
    /// Platform that launches the player in a specific direction after a delay when stepped on.
    /// Uses KCC's PhysicsMover system for proper character interaction.
    /// </summary>
    public class LaunchPlatform : MonoBehaviour, IMoverController
    {
        [Header("Launch Settings")]
        [Tooltip("Time in seconds before the platform launches")]
        [SerializeField] private float launchDelay = 1f;
        
        [Tooltip("Speed at which the platform moves to target (Normal mode only)")]
        [SerializeField] private float launchSpeed = 20f;
        
        [Tooltip("Target position that defines where the platform will move")]
        [SerializeField] private Transform targetDirection;
        
        [Header("Crazy Mode")]
        [Tooltip("If true, platform launches with physics force instead of smooth interpolation")]
        [SerializeField] private bool isCrazyMode = false;
        
        [Tooltip("Force applied when launching in crazy mode")]
        [SerializeField] private float crazyLaunchForce = 50f;
        
        [Tooltip("If true, ejects the player upward before destroying the platform")]
        [SerializeField] private bool ejectBeforeDestroy = true;
        
        [Tooltip("Upward force applied to player when ejecting (only if ejectBeforeDestroy is true)")]
        [SerializeField] private float ejectForce = 30f;
        
        [Tooltip("Time in seconds before destroying the platform after crazy launch")]
        [SerializeField] private float destroyDelay = 5f;

        [Tooltip("Maximum distance the player can be from the platform to receive the ejection force")]
        [SerializeField] private float maxEjectDistance = 5f;
        
        [Header("Detection Settings")]
        [Tooltip("Tag of the player GameObject (default: 'Player')")]
        [SerializeField] private string playerTag = "Player";
        
        [Header("Visual Feedback")]
        [Tooltip("Optional: Material to apply when player is on platform")]
        [SerializeField] private Material activeMaterial;
        
        [Tooltip("Optional: Particle system to play on launch")]
        [SerializeField] private ParticleSystem launchParticles;
        
        [Header("References")]
        [Tooltip("PhysicsMover component (auto-assigned)")]
        public PhysicsMover Mover;
        
        private MeshRenderer meshRenderer;
        private Material originalMaterial;
        private bool isPlayerOnPlatform = false;
        private bool hasLaunched = false;
        private float timeOnPlatform = 0f;
        private Vector3 targetPosition;
        private Vector3 launchDirection;
        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private Collider platformCollider;
        private PlayerController playerController; // Reference to player controller for crazy launch
        private Vector3 currentVelocity; // Current velocity for crazy mode physics
        
        private void Awake()
        {
            // Get or add PhysicsMover
            Mover = GetComponent<PhysicsMover>();
            if (Mover == null)
            {
                Mover = gameObject.AddComponent<PhysicsMover>();
            }
            
            // Set this as the mover controller
            Mover.MoverController = this;
            
            // Get collider
            platformCollider = GetComponent<Collider>();
            if (platformCollider == null)
            {
                Debug.LogError($"LaunchPlatform on {gameObject.name} needs a Collider component!", this);
            }
            
            // Get renderer for visual feedback
            meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                originalMaterial = meshRenderer.material;
            }
            
            // Validate target direction
            if (targetDirection == null)
            {
                Debug.LogWarning($"LaunchPlatform on {gameObject.name} has no target direction set!", this);
            }
            
            // Store initial transform
            initialPosition = transform.position;
            initialRotation = transform.rotation;
        }
        
        private void Update()
        {
            if (isPlayerOnPlatform && !hasLaunched)
            {
                timeOnPlatform += Time.deltaTime;
                
                if (timeOnPlatform >= launchDelay)
                {
                    Launch();
                }
            }
        }
        
        private void FixedUpdate()
        {
            // Check if player is standing on the platform using overlap
            if (!hasLaunched && platformCollider != null)
            {
                CheckPlayerOnPlatform();
            }
        }
        
        private void CheckPlayerOnPlatform()
        {
            // Get the top surface of the platform
            Bounds bounds = platformCollider.bounds;
            Vector3 checkPosition = new Vector3(bounds.center.x, bounds.max.y + 0.1f, bounds.center.z);
            
            // Check for player above the platform
            Collider[] colliders = Physics.OverlapBox(
                checkPosition,
                new Vector3(bounds.extents.x * 0.9f, 0.2f, bounds.extents.z * 0.9f),
                transform.rotation
            );
            
            bool playerFound = false;
            foreach (Collider col in colliders)
            {
                PlayerController controller = col.GetComponent<PlayerController>();
                if (col.CompareTag(playerTag) || controller != null)
                {
                    playerFound = true;
                    if (!isPlayerOnPlatform)
                    {
                        playerController = controller; // Store reference for crazy launch
                        OnPlayerEnter();
                    }
                    break;
                }
            }
            
            if (!playerFound && isPlayerOnPlatform)
            {
                OnPlayerExit();
            }
        }
        
        private void OnPlayerEnter()
        {
            if (hasLaunched) return;
            
            isPlayerOnPlatform = true;
            timeOnPlatform = 0f;
            
            // Calculate target position and direction
            if (targetDirection != null)
            {
                targetPosition = targetDirection.position;
                launchDirection = (targetPosition - transform.position).normalized;
            }
            else
            {
                // Default to forward and up if no target is set
                launchDirection = (transform.forward + Vector3.up).normalized;
                targetPosition = transform.position + launchDirection * 10f;
            }
            
            // Visual feedback
            if (meshRenderer != null && activeMaterial != null)
            {
                meshRenderer.material = activeMaterial;
            }
        }
        
        private void OnPlayerExit()
        {
            if (hasLaunched) return;
            
            isPlayerOnPlatform = false;
            timeOnPlatform = 0f;
            
            // Reset visual feedback
            if (meshRenderer != null && originalMaterial != null)
            {
                meshRenderer.material = originalMaterial;
            }
        }
        
        private void Launch()
        {
            hasLaunched = true;
            
            if (isCrazyMode)
            {
                // CRAZY MODE: Rocket-like physics launch (player stays on platform)
                // CRAZY MODE: Rocket-like physics launch (player stays on platform)
                
                // Initialize velocity for physics simulation
                currentVelocity = launchDirection * crazyLaunchForce;
                
                // Don't make rigidbody dynamic - keep it kinematic so player stays attached
                // The velocity will be applied in UpdateMovement
                
                // Play particles if available
                if (launchParticles != null)
                {
                    launchParticles.Play();
                }
                
                // Start coroutine to eject player and destroy platform
                StartCoroutine(EjectAndDestroy());
            }
            else
            {
                // NORMAL MODE: Smooth interpolation (handled in UpdateMovement)
                // NORMAL MODE: Smooth interpolation (handled in UpdateMovement)
                
                // Play particles if available
                if (launchParticles != null)
                {
                    launchParticles.Play();
                }
            }
        }
        
        /// <summary>
        /// Coroutine that waits, ejects the player, then destroys the platform
        /// </summary>
        private System.Collections.IEnumerator EjectAndDestroy()
        {
            // Wait for the destroy delay
            yield return new WaitForSeconds(destroyDelay);
            
            // Eject player if enabled and we have a reference
            if (ejectBeforeDestroy && playerController != null)
            {
                // Check distance to player
                float distance = Vector3.Distance(transform.position, playerController.transform.position);
                
                if (distance <= maxEjectDistance)
                {
                    // Eject in the same direction as the platform's current movement
                    Vector3 ejectVelocity = currentVelocity.normalized * ejectForce;
                    playerController.AddVelocity(ejectVelocity);
                }
            }
            
            // Destroy the platform
            Destroy(gameObject);
        }
        
        /// <summary>
        /// Called by PhysicsMover to update the platform's movement.
        /// This is where we define the platform's goal position and rotation.
        /// </summary>
        public void UpdateMovement(out Vector3 goalPosition, out Quaternion goalRotation, float deltaTime)
        {
            goalRotation = initialRotation; // Always keep original rotation
            
            if (!hasLaunched)
            {
                // Not launched yet, stay at initial position
                goalPosition = initialPosition;
                return;
            }
            
            if (isCrazyMode)
            {
                // CRAZY MODE: Physics simulation with gravity (rocket-like movement)
                // Apply gravity to velocity
                currentVelocity += Physics.gravity * deltaTime;
                
                // Calculate new position based on velocity
                goalPosition = Mover.Rigidbody.position + (currentVelocity * deltaTime);
            }
            else
            {
                // NORMAL MODE: Smooth interpolation to target
                goalPosition = Vector3.MoveTowards(
                    Mover.Rigidbody.position,
                    targetPosition,
                    launchSpeed * deltaTime
                );
            }
        }
        
        /// <summary>
        /// Reset the platform to its initial state (useful for object pooling or respawning)
        /// </summary>
        public void ResetPlatform()
        {
            hasLaunched = false;
            isPlayerOnPlatform = false;
            timeOnPlatform = 0f;
            
            // Reset position using PhysicsMover
            Mover.SetPosition(initialPosition);
            Mover.SetRotation(initialRotation);
            
            // Reset visual feedback
            if (meshRenderer != null && originalMaterial != null)
            {
                meshRenderer.material = originalMaterial;
            }
        }
        
        private void OnDrawGizmos()
        {
            // Visualize launch direction in editor
            if (targetDirection != null)
            {
                Gizmos.color = Color.red;
                Vector3 direction = (targetDirection.position - transform.position).normalized;
                Gizmos.DrawRay(transform.position, direction * 5f);
                Gizmos.DrawWireSphere(targetDirection.position, 0.5f);
                
                // Draw line to target
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, targetDirection.position);
            }
        }
    }
}
