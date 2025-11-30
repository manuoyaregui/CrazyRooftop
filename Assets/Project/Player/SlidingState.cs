using UnityEngine;
using KinematicCharacterController;
using CrazyRooftop.Player;

namespace CrazyRooftop.Player
{
    public class SlidingState : BaseCharacterState
    {
        private Vector3 _slideDirection = Vector3.zero;
        private float _slideSpeed = 0f;
        private float _currentSlideDecelerationRate = 0f;

        public SlidingState(PlayerController controller) : base(controller)
        {
        }

        public override void Enter()
        {
            Controller.IsSliding = true;
            
            float currentSpeed = Motor.Velocity.magnitude;
            
            // Store slide direction
            if (currentSpeed > PlayerController.MinVelocityForSlideDir)
            {
                _slideDirection = Motor.Velocity.normalized;
            }
            else
            {
                _slideDirection = Vector3.ProjectOnPlane(Vector3.down, Motor.GroundingStatus.GroundNormal).normalized;
            }
            
            // Apply boost
            _slideSpeed = Mathf.Max(currentSpeed * Controller.SlideBoostMultiplier, Controller.MinSlideEndSpeed);
            
            // Calculate deceleration
            _currentSlideDecelerationRate = (_slideSpeed - Controller.MinSlideEndSpeed) / Mathf.Max(Controller.SlideDuration, PlayerController.MinDuration);
            
            // Set capsule
            Motor.SetCapsuleDimensions(0.5f, Controller.SlideCapsuleHeight, Controller.SlideCapsuleHeight * 0.5f);
            Controller.TargetMeshScale = new Vector3(1f, 0.4f, 1f);
        }

        public override void Exit()
        {
            Controller.IsSliding = false;
            // Pass the slide speed back to default state so we don't lose momentum instantly
            // We need a way to access DefaultState or set the speed on Controller
            // For now, we can assume the Controller handles this or we set a property
            if (Controller.GetState(CharacterStateEnum.Default) is DefaultState defaultState)
            {
                defaultState.SetTargetSpeed(_slideSpeed);
            }
        }

        public override void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            if (_slideDirection.sqrMagnitude > 0f)
            {
                currentRotation = Quaternion.LookRotation(_slideDirection, Motor.CharacterUp);
            }

            Vector3 currentUp = (currentRotation * Vector3.up);
            Vector3 smoothedGravityDir = Vector3.Slerp(currentUp, Vector3.up, 1 - Mathf.Exp(-Controller.BonusOrientationSharpness * deltaTime));
            currentRotation = Quaternion.FromToRotation(currentUp, smoothedGravityDir) * currentRotation;
        }

        public override void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            if (Motor.GroundingStatus.IsStableOnGround)
            {
                HandleGroundMovement(deltaTime, ref currentVelocity);
            }
            else
            {
                HandleAirMovement(deltaTime, ref currentVelocity);
            }
            
            HandleJumping(deltaTime, ref currentVelocity);
            
            if (Controller.InternalVelocityAdd.sqrMagnitude > 0f)
            {
                currentVelocity += Controller.InternalVelocityAdd;
                Controller.InternalVelocityAdd = Vector3.zero;
            }
        }

        private void HandleGroundMovement(float deltaTime, ref Vector3 currentVelocity)
        {
            Vector3 effectiveGroundNormal = Motor.GroundingStatus.GroundNormal;
            Vector3 slideDirectionOnSlope = Motor.GetDirectionTangentToSurface(_slideDirection, effectiveGroundNormal).normalized;
            
            float slopeDot = Vector3.Dot(slideDirectionOnSlope, Motor.CharacterUp);
            
            if (slopeDot < PlayerController.SlideDownSlopeThreshold)
            {
                _slideSpeed += Controller.SlideGravity * Controller.SlopeSlideGravityMultiplier * -slopeDot * deltaTime;
            }
            else
            {
                _slideSpeed -= _currentSlideDecelerationRate * deltaTime;
            }
            
            if (CheckSlideEndConditions())
            {
                EndSlide();
            }
            
            currentVelocity = slideDirectionOnSlope * _slideSpeed;
        }

        private void HandleAirMovement(float deltaTime, ref Vector3 currentVelocity)
        {
            // Airborne logic
            currentVelocity += Controller.Gravity * deltaTime;
            currentVelocity *= (1f / (1f + (Controller.Drag * deltaTime)));

            // If user releases the slide key, transition to default
            if (!Controller.ShouldBeCrouching)
            {
                Controller.TransitionToState(CharacterStateEnum.Default);
            }
        }

        private bool CheckSlideEndConditions()
        {
            if (_slideSpeed <= Controller.MinSlideEndSpeed)
            {
                return true;
            }
            
            if (!Controller.ShouldBeCrouching)
            {
                return true;
            }

            return false;
        }

        private void EndSlide()
        {
            Controller.TransitionToState(CharacterStateEnum.Default);
            
            if (Controller.ShouldBeCrouching)
            {
                Controller.IsCrouching = true;
                Motor.SetCapsuleDimensions(0.5f, Controller.CrouchedCapsuleHeight, Controller.CrouchedCapsuleHeight * 0.5f);
                Controller.TargetMeshScale = new Vector3(1f, 0.5f, 1f);
            }
            else
            {
                Motor.SetCapsuleDimensions(0.5f, 2f, 1f);
                if (Motor.CharacterOverlap(
                    Motor.TransientPosition,
                    Motor.TransientRotation,
                    Controller.ProbedColliders,
                    Motor.CollidableLayers,
                    QueryTriggerInteraction.Ignore) > 0)
                {
                    Controller.IsCrouching = true;
                    Motor.SetCapsuleDimensions(0.5f, Controller.CrouchedCapsuleHeight, Controller.CrouchedCapsuleHeight * 0.5f);
                    Controller.TargetMeshScale = new Vector3(1f, 0.5f, 1f);
                }
                else
                {
                    Controller.TargetMeshScale = new Vector3(1f, 1f, 1f);
                    Controller.IsCrouching = false;
                }
            }
        }

        private void HandleJumping(float deltaTime, ref Vector3 currentVelocity)
        {
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

                    Controller.TransitionToState(CharacterStateEnum.Default);
                    Motor.SetCapsuleDimensions(0.5f, 2f, 1f);
                    Controller.TargetMeshScale = new Vector3(1f, 1f, 1f);
                    Controller.IsCrouching = false;

                    Motor.ForceUnground();

                    currentVelocity += (jumpDirection * Controller.JumpUpSpeed) - Vector3.Project(currentVelocity, Motor.CharacterUp);
                    currentVelocity += (_slideDirection * Controller.JumpScalableForwardSpeed);
                    Controller.JumpRequested = false;
                    Controller.JumpConsumed = true;
                    Controller.JumpedThisFrame = true;
                }
            }
        }

        public override void AfterCharacterUpdate(float deltaTime)
        {
             // Handle jump-related values during slide
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
        }
    }
}
