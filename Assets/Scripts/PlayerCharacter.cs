using UnityEngine;
using KinematicCharacterController;
using System;
using UnityEngine.TextCore.Text;

public enum CrouchInput
{
    None, Toggle
}

public enum Stance
{
    Stand, Crouch
}
public struct CharacterInput
{
    public Quaternion Rotation;
    public Vector2 Move;
    public bool Jump;
    public bool JumpSustain;
    public CrouchInput Crouch;
}

public class PlayerCharacter : MonoBehaviour, ICharacterController
{
    [SerializeField] private KinematicCharacterMotor motor;
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private Transform root;

    [Space]
    [SerializeField] private float walkSpeed = 20f;
    [SerializeField] private float crouchSpeed = 10f;

    [SerializeField] private float walkResponse = 25f;
    [SerializeField] private float crouchResponse = 25f;

    [Space]
    [SerializeField] private float airSpeed = 15f;
    [SerializeField] private float airAcceleration = 70f;
    [Space]
    [SerializeField] private float jumpSpeed = 20f;
    [Range(0f, 1f)]
    [SerializeField] private float jumpSustainGravityMultiplier = 0.4f;
    [SerializeField] private float gravity = -90f;

    [Space]
    [SerializeField] private float standHeight = 2f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float crouchHeightResponse = 15f;

    [Range(0f, 1f)]
    [SerializeField] private float standCameraTargetHeight = 0.9f;
    [Range(0f, 1f)]
    [SerializeField] private float crouchCameraTargetHeight = 0.7f;

    private Stance _stance;
    private Quaternion _requestedRotation;
    private Vector3 _requestedMovement;
    private bool _requestedJump;
    private bool _requestedSutainedJump;
    private bool _requestedCrouch;
    private Collider[] _uncrouchOverlapResults;
    private float _standYOffset;
    private float _crouchYOffset;

    public void Initialize()
    {
        _stance = Stance.Stand;
        _uncrouchOverlapResults = new Collider[8];

        _standYOffset = standHeight * 0.5f;
        _crouchYOffset = crouchHeight * 0.5f;

        motor.CharacterController = this;
    }

    public void UpdateInput(CharacterInput input)
    {
        _requestedRotation = input.Rotation;

        _requestedMovement = new Vector3(input.Move.x, 0f, input.Move.y);
        //clamp it to 1 to prevent it going faster diagonally
        _requestedMovement = Vector3.ClampMagnitude(_requestedMovement, 1f);
        //orient the movement to face the same direction as the camera
        _requestedMovement = input.Rotation * _requestedMovement;

        _requestedJump = _requestedJump || input.Jump;

        _requestedSutainedJump = input.JumpSustain;

        _requestedCrouch = input.Crouch switch
        {
            CrouchInput.Toggle => !_requestedCrouch,
            CrouchInput.None => _requestedCrouch,
            _ => _requestedCrouch
        };
    }

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        //project the vector on plane to prevent rotating in any other axis than up
        var forward = Vector3.ProjectOnPlane(
            _requestedRotation * Vector3.forward,
            motor.CharacterUp
        );
        //fix for the stupid "Look rotation viewing vector is zero" log
        if (forward != Vector3.zero)
            currentRotation = Quaternion.LookRotation(forward, motor.CharacterUp);
    }

    public void UpdateBody(float deltaTime)
    {
        var currentHeight = motor.Capsule.height;
        var normalizedHeight = currentHeight / standHeight;

        var cameraTargetHeight = currentHeight * (_stance is Stance.Stand ? standCameraTargetHeight : crouchCameraTargetHeight);
        var rootTargetScale = new Vector3(1f, normalizedHeight, 1f);

        cameraTarget.localPosition = Vector3.Lerp(
            a: cameraTarget.localPosition,
            b: new Vector3(0f, cameraTargetHeight, 0f),
            t: 1f - Mathf.Exp(-crouchHeightResponse * deltaTime)
            );

        //make the root move up or down with the collider
        var rootTargetHeight = _stance is Stance.Stand ? _standYOffset : _crouchYOffset;

        root.localPosition = new Vector3(0f, rootTargetHeight, 0f);
        root.localScale = Vector3.Lerp(
            a: root.localScale,
            b: rootTargetScale,
            t: 1f - Mathf.Exp(-crouchHeightResponse * deltaTime)
            );
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        if (motor.GroundingStatus.IsStableOnGround)
        {
            //snap the movement to the ground to prevent going slightly up or down
            var groundedMovement = motor.GetDirectionTangentToSurface(
                direction: _requestedMovement,
                surfaceNormal: motor.GroundingStatus.GroundNormal
            ) * _requestedMovement.magnitude;

            var speed = _stance is Stance.Stand ? walkSpeed : crouchSpeed;
            var response = _stance is Stance.Stand ? walkResponse : crouchResponse;

            var targetVelocity = groundedMovement * speed;
            currentVelocity = Vector3.Lerp(
                a: currentVelocity,
                b: targetVelocity,
                t: 1f - Mathf.Exp(-response * deltaTime)
            );
        }
        else
        {
            //gravity & air movement
            if (_requestedMovement.sqrMagnitude > 0f)
            {
                //movement on the xz plane
                var planarMovement = Vector3.ProjectOnPlane(_requestedMovement, motor.CharacterUp) * _requestedMovement.magnitude;

                var currentPlanarVelocity = Vector3.ProjectOnPlane(currentVelocity, motor.CharacterUp);

                var movementForce = planarMovement * airAcceleration * deltaTime;

                var targetPlanarVelocity = currentPlanarVelocity + movementForce;

                targetPlanarVelocity = Vector3.ClampMagnitude(targetPlanarVelocity, airSpeed);

                currentVelocity += targetPlanarVelocity - currentPlanarVelocity;
            }
            var effectiveGravity = gravity;
            var verticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
            if (_requestedSutainedJump && verticalSpeed > 0)
                effectiveGravity *= jumpSustainGravityMultiplier;
            currentVelocity += motor.CharacterUp * effectiveGravity * deltaTime;
        }

        if (_requestedJump)
        {
            _requestedJump = false;
            motor.ForceUnground(time: 0f);
            var currentVerticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
            var targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, jumpSpeed);
            currentVelocity += motor.CharacterUp * (targetVerticalSpeed - currentVerticalSpeed);
        }

    }

    public void AfterCharacterUpdate(float deltaTime)
    {
        if (!_requestedCrouch && _stance is not Stance.Stand)
        {
            motor.SetCapsuleDimensions(
                radius: motor.Capsule.radius,
                height: standHeight,
                yOffset: _standYOffset
            );

            //check for overlap

            if (motor.CharacterOverlap(
                motor.TransientPosition,
                motor.TransientRotation,
                _uncrouchOverlapResults,
                motor.CollidableLayers,
                QueryTriggerInteraction.Ignore) > 0)
            {
                _requestedCrouch = true;
                motor.SetCapsuleDimensions(
                    radius: motor.Capsule.radius,
                    height: crouchHeight,
                    yOffset: _crouchYOffset
                );
            }
            else
            {
                _stance = Stance.Stand;
            }
        }
    }

    public void BeforeCharacterUpdate(float deltaTime)
    {
        if (_requestedCrouch && _stance is Stance.Stand)
        {
            _stance = Stance.Crouch;
            motor.SetCapsuleDimensions(
                radius: motor.Capsule.radius,
                height: crouchHeight,
                yOffset: _crouchYOffset
            );
        }
    }


    public bool IsColliderValidForCollisions(Collider coll) => true;

    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {
    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
    }

    public void PostGroundingUpdate(float deltaTime)
    {
    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
    }

    public Transform GetCameraTarget() => cameraTarget;
}
