using UnityEngine;
using KinematicCharacterController;
using CrazyRooftop.Player;

namespace CrazyRooftop.Player
{
    public abstract class BaseCharacterState
    {
        protected PlayerController Controller;
        protected KinematicCharacterMotor Motor;

        public BaseCharacterState(PlayerController controller)
        {
            Controller = controller;
            Motor = controller.Motor;
        }

        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void UpdateRotation(ref Quaternion currentRotation, float deltaTime) { }
        public virtual void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) { }
        public virtual void AfterCharacterUpdate(float deltaTime) { }
        public virtual void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }
    }
}
