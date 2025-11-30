using UnityEngine;

namespace CrazyRooftop.Player
{
    public enum VelocityAxis
    {
        Vertical,
        Horizontal,
        Both
    }

    /// <summary>
    /// Detects when player velocity spikes above a threshold
    /// </summary>
    [CreateAssetMenu(fileName = "VelocitySpikeCombo", menuName = "CrazyRooftop/Combos/Velocity Spike Detector")]
    public class VelocitySpikeDetector : ComboDetector
    {
        [Header("Velocity Spike Settings")]
        [Tooltip("Which velocity axis to monitor")]
        public VelocityAxis Axis = VelocityAxis.Vertical;
        
        [Tooltip("Multiplier of max speed to trigger (1.0 = 100% of max speed, 1.2 = 120%)")]
        public float ThresholdMultiplier = 1.0f;

        private bool _wasAboveThreshold = false;

        public override void OnUpdate(PlayerController player, float deltaTime)
        {
            base.OnUpdate(player, deltaTime);

            // Early exit if on cooldown
            if (IsOnCooldown())
            {
                _wasAboveThreshold = false;
                return;
            }

            float currentSpeed = 0f;
            float maxSpeed = 0f;

            switch (Axis)
            {
                case VelocityAxis.Vertical:
                    currentSpeed = Mathf.Abs(player.Motor.Velocity.y);
                    maxSpeed = player.GetMaxPossibleVerticalSpeed();
                    break;

                case VelocityAxis.Horizontal:
                    Vector3 horizontalVel = player.Motor.Velocity;
                    horizontalVel.y = 0f;
                    currentSpeed = horizontalVel.magnitude;
                    maxSpeed = player.GetMaxPossibleHorizontalSpeed();
                    break;

                case VelocityAxis.Both:
                    currentSpeed = player.Motor.Velocity.magnitude;
                    float maxH = player.GetMaxPossibleHorizontalSpeed();
                    float maxV = player.GetMaxPossibleVerticalSpeed();
                    maxSpeed = Mathf.Sqrt(maxH * maxH + maxV * maxV);
                    break;
            }

            float threshold = maxSpeed * ThresholdMultiplier;
            bool isAboveThreshold = currentSpeed >= threshold;

            // Trigger on rising edge (wasn't above, now is)
            if (isAboveThreshold && !_wasAboveThreshold)
            {
                TriggerCombo();
            }

            _wasAboveThreshold = isAboveThreshold;
        }

        public override void Reset()
        {
            base.Reset();
            _wasAboveThreshold = false;
        }
    }
}
