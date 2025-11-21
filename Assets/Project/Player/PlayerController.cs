using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using System;


namespace CrazyRooftop.Player
{
    public enum CharacterStateEnum
    {
        Default,
        Sliding,
    }

    public enum OrientationMethod
    {
        TowardsCamera,
        TowardsMovement,
    }

    public struct PlayerCharacterInputs
    {
        public float MoveAxisForward;
        public float MoveAxisRight;
        public Quaternion CameraRotation;
        public bool JumpDown;
        public bool CrouchDown;
        public bool CrouchUp;
    }

    public struct AICharacterInputs
    {
        public Vector3 MoveVector;
        public Vector3 LookVector;
    }

    public enum BonusOrientationMethod
    {
        None,
        TowardsGravity,
        TowardsGroundSlopeAndGravity,
    }

    public class PlayerController : MonoBehaviour, ICharacterController
    {
        public KinematicCharacterMotor Motor;

        [Header("Stable Movement")]
        public float MaxStableMoveSpeed = 10f;
        public float MinStableMoveSpeed = 2f; // Starting speed
        [Tooltip("Time in seconds to reach max speed from min speed")]
        public float TimeToMaxSpeed = 2f;
        public float DecelerationRate = 10f; // How fast to slow down when stopping
        [Range(0f, 1f)]
        [Tooltip("Percentage of MinStableMoveSpeed that the player moves at when crouching")]
        public float MaxCrouchSpeedPercent = 0.8f;
        public float StableMovementSharpness = 15f;
        public float OrientationSharpness = 10f;
        public OrientationMethod OrientationMethod = OrientationMethod.TowardsCamera;

        [Header("Air Movement")]
        public float MaxAirMoveSpeed = 15f;
        public float AirAccelerationSpeed = 15f;
        public float Drag = 0.1f;

        [Header("Jumping")]
        public bool AllowJumpingWhenSliding = false;
        public float JumpUpSpeed = 10f;
        public float JumpScalableForwardSpeed = 10f;
        public float JumpPreGroundingGraceTime = 0f;
        public float JumpPostGroundingGraceTime = 0f;

        [Header("Sliding")]
        [Range(0f, 1f)]
        [Tooltip("Percentage of MaxStableMoveSpeed required to initiate a slide")]
        public float MinSlideSpeedPercent = 0.5f;
        [Tooltip("Velocity multiplier applied when starting a slide")]
        public float SlideBoostMultiplier = 1.3f;
        [Tooltip("Target duration of the slide in seconds")]
        public float SlideDuration = 1.0f;
        [Tooltip("Minimum speed before slide automatically ends")]
        public float MinSlideEndSpeed = 2f;
        [Tooltip("Gravity acceleration applied when sliding down a slope")]
        public float SlideGravity = 20f;
        [Tooltip("Multiplier for gravity when sliding down a slope to make it feel stronger")]
        public float SlopeSlideGravityMultiplier = 2.0f;
        public float SlideCapsuleHeight = 0.8f;
        public float CrouchedCameraHeight = 1.0f;

        [Header("Misc")]
        public List<Collider> IgnoredColliders = new List<Collider>();
        public BonusOrientationMethod BonusOrientationMethod = BonusOrientationMethod.None;
        public float BonusOrientationSharpness = 10f;
        public Vector3 Gravity = new Vector3(0, -30f, 0);
        public Transform MeshRoot;
        public Transform CameraFollowPoint;
        public Camera PlayerCamera;
        public float SlideCameraTilt = 15f;
        public float CameraTiltSharpness = 10f;
        public float SlideFOV = 80f;
        public float FOVSpeed = 5f;
        public float CrouchScaleSharpness = 10f;
        public float CrouchedCapsuleHeight = 1f;

        // Constants
        public const float SlopeThreshold = 0.99f;
        public const float UphillThreshold = 0.1f;
        public const float MinVelocityForSlideDir = 0.1f;
        public const float MinDuration = 0.01f;
        public const float SlideDownSlopeThreshold = -0.01f;

        public CharacterStateEnum CurrentCharacterStateEnum { get; private set; }
        public BaseCharacterState CurrentState { get; private set; }

        // Exposed Properties for States
        public Vector3 MoveInputVector { get; private set; }
        public Vector3 LookInputVector { get; private set; }
        public bool JumpRequested { get; set; }
        public bool JumpConsumed { get; set; }
        public bool JumpedThisFrame { get; set; }
        public float TimeSinceJumpRequested { get; set; } = Mathf.Infinity;
        public float TimeSinceLastAbleToJump { get; set; }
        public Vector3 InternalVelocityAdd { get; set; }
        public bool ShouldBeCrouching { get; private set; }
        public bool IsCrouching { get; set; }
        public bool IsSliding { get; set; }
        public Vector3 TargetMeshScale { get; set; } = Vector3.one;
        public Collider[] ProbedColliders { get; private set; } = new Collider[8];

        private RaycastHit[] _probedHits = new RaycastHit[8];
        private float _standingCameraHeight;
        private float _defaultFOV;

        // States
        private DefaultState _defaultState;
        private SlidingState _slidingState;

        private void Awake()
        {
            // Initialize States
            _defaultState = new DefaultState(this);
            _slidingState = new SlidingState(this);

            // Handle initial state
            TransitionToState(CharacterStateEnum.Default);

            // Assign the characterController to the motor
            Motor.CharacterController = this;

            // Store initial camera height
            if (CameraFollowPoint)
            {
                _standingCameraHeight = CameraFollowPoint.localPosition.y;
            }

            if (PlayerCamera)
            {
                _defaultFOV = PlayerCamera.fieldOfView;
            }
            else if (CameraFollowPoint)
            {
                 PlayerCamera = CameraFollowPoint.GetComponent<Camera>();
                 if (PlayerCamera) _defaultFOV = PlayerCamera.fieldOfView;
            }
        }

        /// <summary>
        /// Handles movement state transitions and enter/exit callbacks
        /// </summary>
        public void TransitionToState(CharacterStateEnum newStateEnum)
        {
            CharacterStateEnum tmpInitialStateEnum = CurrentCharacterStateEnum;
            CurrentCharacterStateEnum = newStateEnum;

            BaseCharacterState newState = null;
            switch (newStateEnum)
            {
                case CharacterStateEnum.Default:
                    newState = _defaultState;
                    break;
                case CharacterStateEnum.Sliding:
                    newState = _slidingState;
                    break;
            }

            if (CurrentState != null)
            {
                CurrentState.Exit();
            }

            CurrentState = newState;

            if (CurrentState != null)
            {
                CurrentState.Enter();
            }
        }

        public BaseCharacterState GetState(CharacterStateEnum stateEnum)
        {
            switch (stateEnum)
            {
                case CharacterStateEnum.Default:
                    return _defaultState;
                case CharacterStateEnum.Sliding:
                    return _slidingState;
                default:
                    return null;
            }
        }

        /// <summary>
        /// This is called every frame by Player in order to tell the character what its inputs are
        /// </summary>
        public void SetInputs(ref PlayerCharacterInputs inputs)
        {
            // Clamp input
            Vector3 moveInputVector = Vector3.ClampMagnitude(new Vector3(inputs.MoveAxisRight, 0f, inputs.MoveAxisForward), 1f);

            // Calculate camera direction and rotation on the character plane
            Vector3 cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.forward, Motor.CharacterUp).normalized;
            if (cameraPlanarDirection.sqrMagnitude == 0f)
            {
                cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.up, Motor.CharacterUp).normalized;
            }
            Quaternion cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, Motor.CharacterUp);

            // Update crouching state (Global)
            if (inputs.CrouchDown)
            {
                ShouldBeCrouching = true;
            }
            else if (inputs.CrouchUp)
            {
                ShouldBeCrouching = false;
            }

            // Update jump state (Global)
            if (inputs.JumpDown)
            {
                TimeSinceJumpRequested = 0f;
                JumpRequested = true;
            }

            // Move and look inputs
            MoveInputVector = cameraPlanarRotation * moveInputVector;

            switch (OrientationMethod)
            {
                case OrientationMethod.TowardsCamera:
                    LookInputVector = cameraPlanarDirection;
                    break;
                case OrientationMethod.TowardsMovement:
                    LookInputVector = MoveInputVector.normalized;
                    break;
            }
        }

        /// <summary>
        /// This is called every frame by the AI script in order to tell the character what its inputs are
        /// </summary>
        public void SetInputs(ref AICharacterInputs inputs)
        {
            MoveInputVector = inputs.MoveVector;
            LookInputVector = inputs.LookVector;
        }

        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is called before the character begins its movement update
        /// </summary>
        public void BeforeCharacterUpdate(float deltaTime)
        {
        }

        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is where you tell your character what its rotation should be right now. 
        /// This is the ONLY place where you should set the character's rotation
        /// </summary>
        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            if (CurrentState != null)
            {
                CurrentState.UpdateRotation(ref currentRotation, deltaTime);
            }
        }

        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is where you tell your character what its velocity should be right now. 
        /// This is the ONLY place where you can set the character's velocity
        /// </summary>
        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            if (CurrentState != null)
            {
                CurrentState.UpdateVelocity(ref currentVelocity, deltaTime);
            }
        }

        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is called after the character has finished its movement update
        /// </summary>
        public void AfterCharacterUpdate(float deltaTime)
        {
            if (CurrentState != null)
            {
                CurrentState.AfterCharacterUpdate(deltaTime);
            }

            UpdateCameraPosition(deltaTime);
            UpdateMeshScale(deltaTime);
        }

        private void UpdateCameraPosition(float deltaTime)
        {
            // Handle Camera Height and Tilt
            if (CameraFollowPoint)
            {
                float targetHeight = _standingCameraHeight;
                Quaternion targetRotation = Quaternion.identity;
                float targetFOV = _defaultFOV;

                if (IsSliding)
                {
                    targetHeight = CrouchedCameraHeight;
                    targetRotation = Quaternion.Euler(0, 0, SlideCameraTilt);
                    targetFOV = SlideFOV;
                }
                else if (IsCrouching)
                {
                    targetHeight = CrouchedCameraHeight;
                }

                Vector3 targetPos = CameraFollowPoint.localPosition;
                targetPos.y = targetHeight;
                CameraFollowPoint.localPosition = Vector3.Lerp(CameraFollowPoint.localPosition, targetPos, 10f * deltaTime);

                CameraFollowPoint.localRotation = Quaternion.Slerp(CameraFollowPoint.localRotation, targetRotation, CameraTiltSharpness * deltaTime);

                if (PlayerCamera)
                {
                    PlayerCamera.fieldOfView = Mathf.Lerp(PlayerCamera.fieldOfView, targetFOV, FOVSpeed * deltaTime);
                }
            }
        }

        private void UpdateMeshScale(float deltaTime)
        {
            // Handle Body Scale Interpolation
            if (MeshRoot)
            {
                MeshRoot.localScale = Vector3.Lerp(MeshRoot.localScale, TargetMeshScale, CrouchScaleSharpness * deltaTime);
            }
        }

        public void PostGroundingUpdate(float deltaTime)
        {
            // Handle landing and leaving ground
            if (Motor.GroundingStatus.IsStableOnGround && !Motor.LastGroundingStatus.IsStableOnGround)
            {
                OnLanded();
            }
            else if (!Motor.GroundingStatus.IsStableOnGround && Motor.LastGroundingStatus.IsStableOnGround)
            {
                OnLeaveStableGround();
            }
        }

        public bool IsColliderValidForCollisions(Collider coll)
        {
            if (IgnoredColliders.Count == 0)
            {
                return true;
            }

            if (IgnoredColliders.Contains(coll))
            {
                return false;
            }

            return true;
        }

        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
        }

        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
        }

        public void AddVelocity(Vector3 velocity)
        {
            switch (CurrentCharacterStateEnum)
            {
                case CharacterStateEnum.Default:
                    {
                        InternalVelocityAdd += velocity;
                        break;
                    }
            }
        }

        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
        {
        }

        protected void OnLanded()
        {
        }

        protected void OnLeaveStableGround()
        {
        }

        public void OnDiscreteCollisionDetected(Collider hitCollider)
        {
        }
    }
}
