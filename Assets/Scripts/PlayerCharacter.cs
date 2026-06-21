using UnityEngine;
using KinematicCharacterController;

public struct CharacterInput
{
    public Quaternion Rotation;
    public Vector2 Move;
}

public class PlayerCharacter : MonoBehaviour, ICharacterController
{
    [SerializeField] private KinematicCharacterMotor motor;
    [SerializeField] private Transform cameraTarget;
    [Space]
    [SerializeField] private float walkSpeed = 20f;

    private Quaternion _requestedRotation;
    private Vector3 _requestedMovement;

    public void Initialize()
    {
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

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        //snap the movement to the ground to prevent going slightly up or down
        var groundedMovement = motor.GetDirectionTangentToSurface(
            direction: _requestedMovement,
            surfaceNormal: motor.GroundingStatus.GroundNormal
        ) * _requestedMovement.magnitude;
        currentVelocity = groundedMovement * walkSpeed;
    }

    public void AfterCharacterUpdate(float deltaTime)
    {
    }

    public void BeforeCharacterUpdate(float deltaTime)
    {
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
