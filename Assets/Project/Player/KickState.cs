using UnityEngine;
using KinematicCharacterController;

namespace CrazyRooftop.Player
{
    public class KickState : BaseCharacterState
    {
        public KickState(PlayerController controller) : base(controller)
        {
        }

        public override void Enter()
        {
            Controller.IsCrouching = true;
            Controller.HasKickedInAir = true;

            // Set capsule dimensions for kick (crouched)
            Motor.SetCapsuleDimensions(0.5f, Controller.CrouchedCapsuleHeight, Controller.CrouchedCapsuleHeight * 0.5f);
            Controller.TargetMeshScale = new Vector3(1f, 0.5f, 1f);

            // Apply Kick Impulse
            Vector3 forwardDir = Vector3.zero;
            
            // If we have movement input, use it
            if (Controller.MoveInputVector.sqrMagnitude > 0f)
            {
                forwardDir = Controller.MoveInputVector.normalized;
            }
            else
            {
                // Use Camera Forward projected on plane
                if (Controller.PlayerCamera)
                {
                    forwardDir = Vector3.ProjectOnPlane(Controller.PlayerCamera.transform.forward, Motor.CharacterUp).normalized;
                }
                else
                {
                    forwardDir = Motor.CharacterForward;
                }
            }

            if (forwardDir.sqrMagnitude == 0f) forwardDir = Motor.CharacterForward;

            // Calculate Kick Speed based on momentum
            float currentHorizontalSpeed = Vector3.ProjectOnPlane(Motor.Velocity, Motor.CharacterUp).magnitude;
            float kickSpeed = Controller.KickForwardSpeed + (currentHorizontalSpeed * Controller.KickMomentumMultiplier);

            Vector3 kickVelocity = forwardDir * kickSpeed;
            
            float minSpeedForUpward = Controller.MaxStableMoveSpeed * Controller.MinKickUpwardSpeedPercent;
            
            if (currentHorizontalSpeed > minSpeedForUpward)
            {
                kickVelocity.y = Controller.KickUpwardSpeed;
            }
            else
            {
                kickVelocity.y = 0f;
            }

            // Reset vertical velocity to ensure consistent upward kick, 
            // BUT preserve falling momentum as requested
            Vector3 currentVel = Motor.Velocity;
            if (currentVel.y > 0f)
            {
                currentVel.y = 0f;
            }
            
            Motor.BaseVelocity = currentVel + kickVelocity;
        }

        public override void Exit()
        {
            // Reset crouching if we are not transitioning to sliding
            // But usually we transition to Default or Sliding. 
            // If we transition to Default, it will handle uncrouching if needed.
        }

        public override void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            // Rotate towards velocity
            if (Motor.Velocity.sqrMagnitude > 0f)
            {
                Vector3 velocityPlanar = Vector3.ProjectOnPlane(Motor.Velocity, Motor.CharacterUp).normalized;
                if (velocityPlanar.sqrMagnitude > 0f)
                {
                    currentRotation = Quaternion.LookRotation(velocityPlanar, Motor.CharacterUp);
                }
            }
        }

        public override void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            // Apply Gravity and Drag
            currentVelocity += Controller.Gravity * deltaTime;
            currentVelocity *= (1f / (1f + (Controller.Drag * deltaTime)));

            // Check for landing
            if (Motor.GroundingStatus.IsStableOnGround)
            {
                // If holding crouch/slide, transition to Sliding
                if (Controller.ShouldBeCrouching)
                {
                    Controller.TransitionToState(CharacterStateEnum.Sliding);
                }
                else
                {
                    Controller.TransitionToState(CharacterStateEnum.Default);
                }
            }
        }

        public override void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
            // Handle pushing objects
            if (hitCollider.attachedRigidbody != null)
            {
                Rigidbody rb = hitCollider.attachedRigidbody;
                if (rb.mass <= Controller.SlidePushMassLimit) // Reusing slide push limit for now
                {
                    Vector3 pushDirection = Motor.Velocity.normalized;
                    // Add some upward force
                    pushDirection += Vector3.up * 0.5f;
                    pushDirection.Normalize();

                    float forceMagnitude = Controller.KickPushForceMultiplier;
                    rb.AddForce(pushDirection * forceMagnitude, ForceMode.Impulse);
                }
            }
        }
    }
}
