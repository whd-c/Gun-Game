using UnityEngine;
using KinematicCharacterController;
using System;
using UnityEngine.TextCore.Text;

public enum CrouchInput
{
    None, Toggle, Hold, UnHold
}

public enum Stance
{
    Stand, Crouch, Slide
}

public struct CharacterState
{
    public bool Grounded;
    public Stance Stance;
    public Vector3 Velocity;
    public Vector3 Acceleration;
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
    public bool holdCrouch = true;

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
    [SerializeField] private float coyoteTime = 0.075f;
    [SerializeField] private float jumpBufferTime = 0.05f;
    [Range(0f, 1f)]
    [SerializeField] private float jumpSustainGravityMultiplier = 0.4f;
    [SerializeField] private float gravity = -90f;

    [Space]
    [SerializeField] private float speedNeededToSlide = 10f;
    [SerializeField] private float slideStartSpeed = 25f;
    [SerializeField] private float slideEndSpeed = 15f;
    [SerializeField] private float slideFriction = 0.8f;
    [SerializeField] private float slideSteerAcceleration = 5f;
    [SerializeField] private float slideGravity = -90f;

    [Space]
    [SerializeField] private float standHeight = 2f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float crouchHeightResponse = 15f;
    [Range(0f, 1f)]
    [SerializeField] private float standCameraTargetHeight = 0.9f;
    [Range(0f, 1f)]
    [SerializeField] private float crouchCameraTargetHeight = 0.7f;

    private CharacterState _state;
    private CharacterState _lastState;
    private CharacterState _tempState;
    private Quaternion _requestedRotation;
    private Vector3 _requestedMovement;
    private bool _requestedJump;
    private bool _requestedSustainedJump;
    private bool _requestedCrouch;
    private bool _requestedCrouchInAir;
    private float _timeSinceUngrounded;
    private float _timeSinceJumpRequest;
    private bool _ungroundedDueToJump;
    private Collider[] _uncrouchOverlapResults;
    private float _standYOffset;
    private float _crouchYOffset;

    public void Initialize()
    {
        _state.Stance = Stance.Stand;
        _lastState = _state;

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

        var wasRequestingJump = _requestedJump;
        _requestedJump = _requestedJump || input.Jump;
        if (_requestedJump && !wasRequestingJump)
        {
            _timeSinceJumpRequest = 0f;
        }
        _requestedSustainedJump = input.JumpSustain;


        var wasRequestingCrouch = _requestedCrouch;
        _requestedCrouch = input.Crouch switch
        {
            CrouchInput.Toggle => !_requestedCrouch,
            CrouchInput.Hold => true,
            CrouchInput.UnHold => false,
            CrouchInput.None => _requestedCrouch,
            _ => _requestedCrouch
        };
        if (_requestedCrouch && !wasRequestingCrouch)
        {
            _requestedCrouchInAir = !_state.Grounded;
        }
        else if (!_requestedCrouch && wasRequestingCrouch)
        {
            _requestedCrouchInAir = false;
        }
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

        var cameraTargetHeight = currentHeight * (_state.Stance is Stance.Stand ? standCameraTargetHeight : crouchCameraTargetHeight);
        var rootTargetScale = new Vector3(1f, normalizedHeight, 1f);

        cameraTarget.localPosition = Vector3.Lerp(
            a: cameraTarget.localPosition,
            b: new Vector3(0f, cameraTargetHeight, 0f),
            t: 1f - Mathf.Exp(-crouchHeightResponse * deltaTime)
            );

        //make the root move up or down with the collider
        var rootTargetHeight = _state.Stance is Stance.Stand ? _standYOffset : _crouchYOffset;

        root.localPosition = new Vector3(0f, rootTargetHeight, 0f);
        root.localScale = Vector3.Lerp(
            a: root.localScale,
            b: rootTargetScale,
            t: 1f - Mathf.Exp(-crouchHeightResponse * deltaTime)
            );
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        _state.Acceleration = Vector3.zero;
        if (motor.GroundingStatus.IsStableOnGround)
        {
            _timeSinceUngrounded = 0f;
            _ungroundedDueToJump = false;
            //snap the movement to the ground to prevent going slightly up or down
            var groundedMovement = motor.GetDirectionTangentToSurface(
                direction: _requestedMovement,
                surfaceNormal: motor.GroundingStatus.GroundNormal
            ) * _requestedMovement.magnitude;

            {
                var isMoving = currentVelocity.magnitude >= speedNeededToSlide - Mathf.Epsilon;
                var isCrouching = _state.Stance is Stance.Crouch;
                var wasStanding = _lastState.Stance is Stance.Stand;
                var wasInAir = !_lastState.Grounded;

                //initial slide
                if (isMoving && isCrouching && (wasStanding || wasInAir))
                {
                    _state.Stance = Stance.Slide;

                    if (wasInAir)
                    {
                        //start sliding naturally after landing (slide along the slope)
                        currentVelocity = Vector3.ProjectOnPlane(_lastState.Velocity, motor.GroundingStatus.GroundNormal);
                    }

                    var effectiveSlideStartSpeed = slideStartSpeed;
                    if (!_lastState.Grounded && !_requestedCrouchInAir)
                    {
                        effectiveSlideStartSpeed = 0f;
                        _requestedCrouchInAir = false;
                    }
                    var slideSpeed = Mathf.Max(effectiveSlideStartSpeed, currentVelocity.magnitude);
                    currentVelocity = motor.GetDirectionTangentToSurface(
                        direction: currentVelocity,
                        surfaceNormal: motor.GroundingStatus.GroundNormal
                    ) * slideSpeed;
                }
            }

            if (_state.Stance is Stance.Stand or Stance.Crouch)
            {
                var speed = _state.Stance is Stance.Stand ? walkSpeed : crouchSpeed;
                var response = _state.Stance is Stance.Stand ? walkResponse : crouchResponse;

                var targetVelocity = groundedMovement * speed;
                var moveVelocity = Vector3.Lerp(
                    a: currentVelocity,
                    b: targetVelocity,
                    t: 1f - Mathf.Exp(-response * deltaTime)
                );
                _state.Acceleration = (moveVelocity - currentVelocity) / deltaTime;
                currentVelocity = moveVelocity;
            }
            else //continue sliding
            {
                currentVelocity -= currentVelocity * (slideFriction * deltaTime);

                //on slopes
                {
                    var force = Vector3.ProjectOnPlane(-motor.CharacterUp, motor.GroundingStatus.GroundNormal
                    ) * slideGravity;
                    currentVelocity -= force * deltaTime;
                }

                //steering
                {
                    var originalSpeed = currentVelocity.magnitude;
                    var targetVelocity = groundedMovement * originalSpeed;
                    var steerVelocity = currentVelocity;
                    var steerForce = deltaTime * slideSteerAcceleration * (targetVelocity - steerVelocity);

                    steerVelocity += steerForce;
                    steerVelocity = Vector3.ClampMagnitude(steerVelocity, originalSpeed);

                    _state.Acceleration = (steerVelocity - currentVelocity) / deltaTime;
                    currentVelocity = steerVelocity;
                }

                if (currentVelocity.magnitude < slideEndSpeed)
                    _state.Stance = Stance.Crouch;
            }
        }
        else
        {
            _timeSinceUngrounded += deltaTime;
            //gravity & air movement
            if (_requestedMovement.sqrMagnitude > 0f)
            {
                //movement on the xz plane
                var planarMovement = Vector3.ProjectOnPlane(_requestedMovement, motor.CharacterUp) * _requestedMovement.magnitude;

                var currentPlanarVelocity = Vector3.ProjectOnPlane(currentVelocity, motor.CharacterUp);

                var movementForce = airAcceleration * deltaTime * planarMovement;

                if (currentPlanarVelocity.magnitude < airSpeed)
                {
                    var targetPlanarVelocity = currentPlanarVelocity + movementForce;

                    targetPlanarVelocity = Vector3.ClampMagnitude(targetPlanarVelocity, airSpeed);

                    movementForce = targetPlanarVelocity - currentPlanarVelocity;
                }

                else if (Vector3.Dot(currentPlanarVelocity, movementForce) > 0f)
                {
                    var constrainedMovementForce = Vector3.ProjectOnPlane(movementForce, currentPlanarVelocity.normalized);

                    movementForce = constrainedMovementForce;
                }

                //anti airwalking fix
                if (motor.GroundingStatus.FoundAnyGround)
                {
                    if (Vector3.Dot(movementForce, currentVelocity + movementForce) > 0f)
                    {
                        //calculate horizontal direction of the slope we are trying to airwalk on
                        var obstructionNormal = Vector3.Cross(
                            motor.CharacterUp,
                            Vector3.Cross(
                                motor.CharacterUp,
                                motor.GroundingStatus.GroundNormal
                            )).normalized;

                        //remove the direction from our movement                    
                        movementForce = Vector3.ProjectOnPlane(movementForce, obstructionNormal);

                    }
                }

                currentVelocity += movementForce;
            }
            var effectiveGravity = gravity;
            var verticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
            if (_requestedSustainedJump && verticalSpeed > 0)
                effectiveGravity *= jumpSustainGravityMultiplier;
            currentVelocity += motor.CharacterUp * effectiveGravity * deltaTime;
        }

        if (_requestedJump)
        {
            var canCoyoteJump = _timeSinceUngrounded < coyoteTime && !_ungroundedDueToJump;
            if ((_state.Grounded || canCoyoteJump) && _state.Stance is not Stance.Crouch)
            {
                _requestedJump = false;
                _requestedCrouch = false;
                _requestedCrouchInAir = false;
                _ungroundedDueToJump = true;

                motor.ForceUnground(time: 0.1f);
                var currentVerticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
                var targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, jumpSpeed);
                currentVelocity += motor.CharacterUp * (targetVerticalSpeed - currentVerticalSpeed);
            }
            else
            {
                _timeSinceJumpRequest += deltaTime;

                var canJumpLater = _timeSinceJumpRequest < jumpBufferTime;
                _requestedJump = canJumpLater;
            }

        }

    }

    public void AfterCharacterUpdate(float deltaTime)
    {
        if (!_requestedCrouch && _state.Stance is not Stance.Stand)
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
                _state.Stance = Stance.Stand;
            }
        }
        _state.Grounded = motor.GroundingStatus.IsStableOnGround;
        _state.Velocity = motor.Velocity;
        _lastState = _tempState;

        var totalAcceleration = (_state.Velocity - _lastState.Velocity) / deltaTime;
        _state.Acceleration = Vector3.ClampMagnitude(_state.Acceleration, totalAcceleration.magnitude);
    }

    public void BeforeCharacterUpdate(float deltaTime)
    {
        _tempState = _state;
        if (_requestedCrouch && _state.Stance is Stance.Stand)
        {
            _state.Stance = Stance.Crouch;
            motor.SetCapsuleDimensions(
                radius: motor.Capsule.radius,
                height: crouchHeight,
                yOffset: _crouchYOffset
            );
        }
    }

    public void PostGroundingUpdate(float deltaTime)
    {
        if (!motor.GroundingStatus.IsStableOnGround && _state.Stance is Stance.Slide)
        {
            _state.Stance = Stance.Crouch;
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
        _state.Acceleration = Vector3.ProjectOnPlane(_state.Acceleration, hitNormal);
    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
    }

    public Transform GetCameraTarget() => cameraTarget;
    public CharacterState GetState() => _state;
    public CharacterState GetLastState() => _lastState;
}
