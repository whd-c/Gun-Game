using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private PlayerCharacter playerCharacter;

    [Space]
    [SerializeField] private PlayerCamera playerCamera;
    [SerializeField] private CameraSpring cameraSpring;
    [SerializeField] private CameraLean cameraLean;

    [Space]
    [SerializeField] private WeaponSystem weaponSystem;
    [SerializeField] private WeaponHolder weaponHolder;

    private PlayerInputActions _inputActions;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        _inputActions = new PlayerInputActions();
        _inputActions.Enable();

        playerCharacter.Initialize();

        playerCamera.Initialize(playerCharacter.GetCameraTarget());
        cameraSpring.Initialize();
        cameraLean.Initialize();

        weaponSystem.Initialize();
        weaponHolder.Initialize();
    }

    void OnDestroy()
    {
        _inputActions.Dispose();
    }

    void Update()
    {
        var input = _inputActions.Gameplay;
        var deltaTime = Time.deltaTime;

        //get camera input and update it
        var cameraInput = new CameraInput { Look = input.Look.ReadValue<Vector2>() };
        playerCamera.UpdateRotation(cameraInput);


        //get character input and update it
        var crouchState = CrouchInput.None;
        if (playerCharacter.holdCrouch)
        {
            if (input.Crouch.IsPressed())
            {
                crouchState = CrouchInput.Hold;
            }
            else if (input.Crouch.WasReleasedThisFrame())
            {
                crouchState = CrouchInput.UnHold;
            }
        }
        else
        {
            if (input.Crouch.WasPressedThisFrame())
            {
                crouchState = CrouchInput.Toggle;
            }

        }

        var characterInput = new CharacterInput
        {
            Rotation = playerCamera.transform.rotation,
            Move = input.Move.ReadValue<Vector2>(),
            Jump = input.Jump.WasPressedThisFrame(),
            JumpSustain = input.Jump.IsPressed(),
            Crouch = crouchState
        };
        playerCharacter.UpdateInput(characterInput);
        playerCharacter.UpdateBody(deltaTime);

        var cameraTarget = playerCharacter.GetCameraTarget();
        var state = playerCharacter.GetState();
        var isSliding = state.Stance is Stance.Slide;

        playerCamera.UpdatePosition(cameraTarget);
        cameraSpring.UpdateSpring(deltaTime, cameraTarget.up);
        cameraLean.UpdateLean(deltaTime, isSliding, state.Acceleration, cameraTarget.up);

        weaponSystem.UpdateInput();
        weaponHolder.UpdateWeapon();
    }
}
