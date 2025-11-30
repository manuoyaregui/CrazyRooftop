using UnityEngine;
using System.Collections.Generic;

namespace CrazyRooftop.Player
{
    public enum PlayerAction
    {
        Jump,
        Kick,
        Slide,
        Land
    }

    /// <summary>
    /// Detects sequences of player actions within a time window
    /// </summary>
    [CreateAssetMenu(fileName = "InputSequenceCombo", menuName = "CrazyRooftop/Combos/Input Sequence Detector")]
    public class InputSequenceDetector : ComboDetector
    {
        [Header("Sequence Settings")]
        [Tooltip("The sequence of actions to detect")]
        public PlayerAction[] RequiredSequence = new PlayerAction[] { PlayerAction.Jump, PlayerAction.Kick };
        
        [Tooltip("Maximum time between actions in seconds")]
        public float TimeWindow = 2f;
        
        [Tooltip("If true, player velocity must increase with each action")]
        public bool RequireVelocityIncrease = false;

        // Circular buffer for action history
        private const int MAX_BUFFER_SIZE = 20;
        private PlayerAction[] _actionBuffer = new PlayerAction[MAX_BUFFER_SIZE];
        private float[] _timeBuffer = new float[MAX_BUFFER_SIZE];
        private int _bufferIndex = 0;
        private int _bufferCount = 0;

        // Tracking state
        private CharacterStateEnum _lastState = CharacterStateEnum.Default;
        private bool _wasGrounded = true;
        private float _lastVelocityMagnitude = 0f;

        public override void OnUpdate(PlayerController player, float deltaTime)
        {
            base.OnUpdate(player, deltaTime);

            // Track state changes to detect actions
            CharacterStateEnum currentState = player.CurrentCharacterStateEnum;
            bool isGrounded = player.Motor.GroundingStatus.IsStableOnGround;

            // Detect Jump (transition from grounded to not grounded in Default state)
            if (_wasGrounded && !isGrounded && currentState == CharacterStateEnum.Default)
            {
                RecordAction(PlayerAction.Jump);
            }

            // Detect Kick (entering Kick state)
            if (currentState == CharacterStateEnum.Kick && _lastState != CharacterStateEnum.Kick)
            {
                RecordAction(PlayerAction.Kick);
            }

            // Detect Slide (entering Sliding state)
            if (currentState == CharacterStateEnum.Sliding && _lastState != CharacterStateEnum.Sliding)
            {
                RecordAction(PlayerAction.Slide);
            }

            _lastState = currentState;
            _wasGrounded = isGrounded;
        }

        public override void OnLanded(PlayerController player)
        {
            RecordAction(PlayerAction.Land);
        }

        private void RecordAction(PlayerAction action)
        {
            // Add to circular buffer
            _actionBuffer[_bufferIndex] = action;
            _timeBuffer[_bufferIndex] = Time.time;
            _bufferIndex = (_bufferIndex + 1) % MAX_BUFFER_SIZE;
            if (_bufferCount < MAX_BUFFER_SIZE)
            {
                _bufferCount++;
            }

            // Check if sequence matches
            CheckSequence();
        }

        private void CheckSequence()
        {
            if (IsOnCooldown() || RequiredSequence.Length == 0 || _bufferCount < RequiredSequence.Length)
            {
                return;
            }

            // Get the most recent N actions (where N = sequence length)
            int sequenceLength = RequiredSequence.Length;
            PlayerAction[] recentActions = new PlayerAction[sequenceLength];
            float[] recentTimes = new float[sequenceLength];

            for (int i = 0; i < sequenceLength; i++)
            {
                int bufferPos = (_bufferIndex - sequenceLength + i + MAX_BUFFER_SIZE) % MAX_BUFFER_SIZE;
                recentActions[i] = _actionBuffer[bufferPos];
                recentTimes[i] = _timeBuffer[bufferPos];
            }

            // Check if actions match the required sequence
            bool sequenceMatches = true;
            for (int i = 0; i < sequenceLength; i++)
            {
                if (recentActions[i] != RequiredSequence[i])
                {
                    sequenceMatches = false;
                    break;
                }
            }

            if (!sequenceMatches)
            {
                return;
            }

            // Check time window
            float timeDelta = recentTimes[sequenceLength - 1] - recentTimes[0];
            if (timeDelta > TimeWindow)
            {
                return;
            }

            // Optional: Check velocity increase
            if (RequireVelocityIncrease)
            {
                // For now, we'll just check if velocity is generally increasing
                // A more sophisticated check could track velocity at each action
                // This is a simplified version
                float currentVelocity = GameObject.FindObjectOfType<PlayerController>()?.Motor.Velocity.magnitude ?? 0f;
                if (currentVelocity <= _lastVelocityMagnitude)
                {
                    return;
                }
                _lastVelocityMagnitude = currentVelocity;
            }

            // Sequence detected!
            TriggerCombo();
        }

        public override void Reset()
        {
            base.Reset();
            _bufferCount = 0;
            _bufferIndex = 0;
            _lastVelocityMagnitude = 0f;
        }
    }
}
