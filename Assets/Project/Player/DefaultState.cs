using UnityEngine;
using KinematicCharacterController;
using CrazyRooftop.Player;

namespace CrazyRooftop.Player
{
    public class DefaultState : BaseCharacterState
    {
        private float _currentTargetSpeed = 0f;

        public DefaultState(PlayerController controller) : base(controller)
        {
        }

        public override void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            if (Controller.LookInputVector.sqrMagnitude > 0f && Controller.OrientationSharpness > 0f)
            {
                Vector3 smoothedLookInputDirection = Vector3.Slerp(Motor.CharacterForward, Controller.LookInputVector, 1 - Mathf.Exp(-Controller.OrientationSharpness * deltaTime)).normalized;
                currentRotation = Quaternion.LookRotation(smoothedLookInputDirection, Motor.CharacterUp);
            }

            Vector3 currentUp = (currentRotation * Vector3.up);
            if (Controller.BonusOrientationMethod == BonusOrientationMethod.TowardsGravity)
            {
                Vector3 smoothedGravityDir = Vector3.Slerp(currentUp, -Controller.Gravity.normalized, 1 - Mathf.Exp(-Controller.BonusOrientationSharpness * deltaTime));
                currentRotation = Quaternion.FromToRotation(currentUp, smoothedGravityDir) * currentRotation;
            }
            else if (Controller.BonusOrientationMethod == BonusOrientationMethod.TowardsGroundSlopeAndGravity)
            {
                if (Motor.GroundingStatus.IsStableOnGround)
                {
                    Vector3 initialCharacterBottomHemiCenter = Motor.TransientPosition + (currentUp * Motor.Capsule.radius);
                    Vector3 smoothedGroundNormal = Vector3.Slerp(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal, 1 - Mathf.Exp(-Controller.BonusOrientationSharpness * deltaTime));
                    currentRotation = Quaternion.FromToRotation(currentUp, smoothedGroundNormal) * currentRotation;
                    Motor.SetTransientPosition(initialCharacterBottomHemiCenter + (currentRotation * Vector3.down * Motor.Capsule.radius));
                }
                else
                {
                    Vector3 smoothedGravityDir = Vector3.Slerp(currentUp, -Controller.Gravity.normalized, 1 - Mathf.Exp(-Controller.BonusOrientationSharpness * deltaTime));
                    currentRotation = Quaternion.FromToRotation(currentUp, smoothedGravityDir) * currentRotation;
                }
            }
            else
            {
                Vector3 smoothedGravityDir = Vector3.Slerp(currentUp, Vector3.up, 1 - Mathf.Exp(-Controller.BonusOrientationSharpness * deltaTime));
                currentRotation = Quaternion.FromToRotation(currentUp, smoothedGravityDir) * currentRotation;
            }
        }

        public override void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            if (Motor.GroundingStatus.IsStableOnGround)
            {
                float currentVelocityMagnitude = currentVelocity.magnitude;
                Vector3 effectiveGroundNormal = Motor.GroundingStatus.GroundNormal;
                currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) * currentVelocityMagnitude;

                if (Controller.MoveInputVector.sqrMagnitude > 0f)
                {
                    float accelerationRate = (Controller.MaxStableMoveSpeed - Controller.MinStableMoveSpeed) / Mathf.Max(Controller.TimeToMaxSpeed, 0.01f);
                    
                    if (Controller.IsCrouching && !Controller.IsSliding)
                    {
                        float maxCrouchSpeed = Controller.MinStableMoveSpeed * Controller.MaxCrouchSpeedPercent;
                        _currentTargetSpeed = maxCrouchSpeed;
                    }
                    else
                    {
                        _currentTargetSpeed += accelerationRate * deltaTime;
                        _currentTargetSpeed = Mathf.Clamp(_currentTargetSpeed, Controller.MinStableMoveSpeed, Controller.MaxStableMoveSpeed);
                    }
                }
                else
                {
                    _currentTargetSpeed -= Controller.DecelerationRate * deltaTime;
                    _currentTargetSpeed = Mathf.Max(_currentTargetSpeed, 0f);
                }

                Vector3 inputRight = Vector3.Cross(Controller.MoveInputVector, Motor.CharacterUp);
                Vector3 reorientedInput = Vector3.Cross(effectiveGroundNormal, inputRight).normalized * Controller.MoveInputVector.magnitude;
                Vector3 targetMovementVelocity = reorientedInput * _currentTargetSpeed;

                currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1f - Mathf.Exp(-Controller.StableMovementSharpness * deltaTime));
            }
            else
            {
                if (Controller.MoveInputVector.sqrMagnitude > 0f)
                {
                    Vector3 addedVelocity = Controller.MoveInputVector * Controller.AirAccelerationSpeed * deltaTime;
                    Vector3 currentVelocityOnInputsPlane = Vector3.ProjectOnPlane(currentVelocity, Motor.CharacterUp);

                    if (currentVelocityOnInputsPlane.magnitude < Controller.MaxAirMoveSpeed)
                    {
                        Vector3 newTotal = Vector3.ClampMagnitude(currentVelocityOnInputsPlane + addedVelocity, Controller.MaxAirMoveSpeed);
                        addedVelocity = newTotal - currentVelocityOnInputsPlane;
                    }
                    else
                    {
                        if (Vector3.Dot(currentVelocityOnInputsPlane, addedVelocity) > 0f)
                        {
                            addedVelocity = Vector3.ProjectOnPlane(addedVelocity, currentVelocityOnInputsPlane.normalized);
                        }
                    }

                    if (Motor.GroundingStatus.FoundAnyGround)
                    {
                        if (Vector3.Dot(currentVelocity + addedVelocity, addedVelocity) > 0f)
                        {
                            Vector3 perpenticularObstructionNormal = Vector3.Cross(Vector3.Cross(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal), Motor.CharacterUp).normalized;
                            addedVelocity = Vector3.ProjectOnPlane(addedVelocity, perpenticularObstructionNormal);
                        }
                    }

                    currentVelocity += addedVelocity;
                }

                currentVelocity += Controller.Gravity * deltaTime;
                currentVelocity *= (1f / (1f + (Controller.Drag * deltaTime)));
            }

            // Handle jumping
            Controller.JumpedThisFrame = false;
            Controller.TimeSinceJumpRequested += deltaTime;
            if (Controller.JumpRequested)
            {
                if (!Controller.JumpConsumed && ((Controller.AllowJumpingWhenSliding ? Motor.GroundingStatus.FoundAnyGround : Motor.GroundingStatus.IsStableOnGround) || Controller.TimeSinceLastAbleToJump <= Controller.JumpPostGroundingGraceTime))
                {
                    Vector3 jumpDirection = Motor.CharacterUp;
                    if (Motor.GroundingStatus.FoundAnyGround && !Motor.GroundingStatus.IsStableOnGround)
                    {
                        jumpDirection = Motor.GroundingStatus.GroundNormal;
                    }

                    Motor.ForceUnground();
                    currentVelocity += (jumpDirection * Controller.JumpUpSpeed) - Vector3.Project(currentVelocity, Motor.CharacterUp);
                    currentVelocity += (Controller.MoveInputVector * Controller.JumpScalableForwardSpeed);
                    Controller.JumpRequested = false;
                    Controller.JumpConsumed = true;
                    Controller.JumpedThisFrame = true;
                }
            }

            if (Controller.InternalVelocityAdd.sqrMagnitude > 0f)
            {
                currentVelocity += Controller.InternalVelocityAdd;
                Controller.InternalVelocityAdd = Vector3.zero;
            }
        }

        public override void AfterCharacterUpdate(float deltaTime)
        {
            // Handle jump-related values
            if (Controller.JumpRequested && Controller.TimeSinceJumpRequested > Controller.JumpPreGroundingGraceTime)
            {
                Controller.JumpRequested = false;
            }

            if (Controller.AllowJumpingWhenSliding ? Motor.GroundingStatus.FoundAnyGround : Motor.GroundingStatus.IsStableOnGround)
            {
                if (!Controller.JumpedThisFrame)
                {
                    Controller.JumpConsumed = false;
                }
                Controller.TimeSinceLastAbleToJump = 0f;
            }
            else
            {
                Controller.TimeSinceLastAbleToJump += deltaTime;
            }

            // Handle uncrouching
            if (Controller.IsCrouching && !Controller.ShouldBeCrouching)
            {
                Motor.SetCapsuleDimensions(0.5f, 2f, 1f);
                if (Motor.CharacterOverlap(
                    Motor.TransientPosition,
                    Motor.TransientRotation,
                    Controller.ProbedColliders,
                    Motor.CollidableLayers,
                    QueryTriggerInteraction.Ignore) > 0)
                {
                    Motor.SetCapsuleDimensions(0.5f, Controller.CrouchedCapsuleHeight, Controller.CrouchedCapsuleHeight * 0.5f);
                }
                else
                {
                    Controller.TargetMeshScale = new Vector3(1f, 1f, 1f);
                    Controller.IsCrouching = false;
                }
            }

            // Check for slide initiation
            if (Controller.ShouldBeCrouching)
            {
                if (!Controller.IsSliding && Motor.GroundingStatus.IsStableOnGround)
                {
                    float currentSpeed = Motor.Velocity.magnitude;
                    float minSlideSpeed = Controller.MaxStableMoveSpeed * Controller.MinSlideSpeedPercent;
                    
                    bool isMovingUphill = Vector3.Dot(Motor.Velocity.normalized, Motor.CharacterUp) > PlayerController.UphillThreshold;
                    bool isOnSlope = Vector3.Dot(Motor.GroundingStatus.GroundNormal, Motor.CharacterUp) < PlayerController.SlopeThreshold;

                    if ((currentSpeed >= minSlideSpeed && !isMovingUphill) || isOnSlope)
                    {
                        Controller.TransitionToState(CharacterStateEnum.Sliding);
                    }
                    else
                    {
                        if (!Controller.IsCrouching)
                        {
                            Controller.IsCrouching = true;
                            Motor.SetCapsuleDimensions(0.5f, Controller.CrouchedCapsuleHeight, Controller.CrouchedCapsuleHeight * 0.5f);
                            Controller.TargetMeshScale = new Vector3(1f, 0.5f, 1f);
                        }
                    }
                }
                else if (!Controller.IsCrouching)
                {
                    Controller.IsCrouching = true;
                    Motor.SetCapsuleDimensions(0.5f, Controller.CrouchedCapsuleHeight, Controller.CrouchedCapsuleHeight * 0.5f);
                    Controller.TargetMeshScale = new Vector3(1f, 0.5f, 1f);
                }
            }
        }
        
        public void SetTargetSpeed(float speed)
        {
            _currentTargetSpeed = speed;
        }
    }
}
