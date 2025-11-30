using UnityEngine;

namespace CrazyRooftop.Player
{
    /// <summary>
    /// Base class for all combo detectors. Extend this to create custom combo detection logic.
    /// </summary>
    public abstract class ComboDetector : ScriptableObject
    {
        [Header("Combo Settings")]
        [Tooltip("Message to log when combo is detected")]
        public string ComboMessage = "Combo detected!";
        
        [Tooltip("Cooldown in seconds before this combo can trigger again")]
        public float Cooldown = 3f;

        protected float _cooldownTimer = 0f;

        /// <summary>
        /// Called every frame by ComboManager. Override to implement detection logic.
        /// </summary>
        public virtual void OnUpdate(PlayerController player, float deltaTime)
        {
            // Update cooldown
            if (_cooldownTimer > 0f)
            {
                _cooldownTimer -= deltaTime;
            }
        }

        /// <summary>
        /// Called when player lands on ground
        /// </summary>
        public virtual void OnLanded(PlayerController player) { }

        /// <summary>
        /// Called when player state changes
        /// </summary>
        public virtual void OnStateChanged(PlayerController player, CharacterStateEnum newState) { }

        /// <summary>
        /// Reset detector state
        /// </summary>
        public virtual void Reset()
        {
            _cooldownTimer = 0f;
        }

        /// <summary>
        /// Trigger the combo (logs message and starts cooldown)
        /// </summary>
        protected void TriggerCombo()
        {
            if (_cooldownTimer <= 0f)
            {
                Debug.Log($"<color=cyan>[COMBO]</color> {ComboMessage}");
                _cooldownTimer = Cooldown;
            }
        }

        /// <summary>
        /// Check if detector is currently on cooldown
        /// </summary>
        protected bool IsOnCooldown()
        {
            return _cooldownTimer > 0f;
        }
    }
}
