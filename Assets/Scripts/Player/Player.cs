using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class Player : MonoBehaviour
{
    [SerializeField] private PlayerCharacter playerCharacter;

    [Space]
    [SerializeField] private PlayerCamera playerCamera;
    [SerializeField] private CameraSpring cameraSpring;
    [SerializeField] private CameraLean cameraLean;
    [SerializeField] private CameraVignette cameraVignette;

    [Space]
    [SerializeField] private Volume volume;


    [Space]
    [SerializeField] private WeaponSystem weaponSystem;

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

        cameraVignette.Initialize(volume.profile);

        weaponSystem.Initialize();
    }

    void OnDestroy()
    {
        _inputActions.Dispose();
    }

    void Update()
    {
        var input = _inputActions.Gameplay;
        var deltaTime = Time.deltaTime;

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

        //get camera input and update it
        var cameraInput = new CameraInput { Look = input.Look.ReadValue<Vector2>() };
        playerCamera.UpdateRotation(cameraInput);
        playerCamera.UpdatePosition(cameraTarget);
        playerCamera.UpdateCamera(deltaTime, state.Velocity, cameraTarget.up);
        cameraSpring.UpdateSpring(deltaTime, cameraTarget.up);
        cameraLean.UpdateLean(deltaTime, state, cameraTarget.up);
        cameraVignette.UpdateVignette(deltaTime, state.Velocity);

        // Getting 1 - 9 input for the weapon
        // this is shit code, im sorry for pushing this to the repo
        // swear im gonna refactor this

        var choosenWeapon = -1;

        if (Keyboard.current.anyKey.IsPressed())
        {
            for (int i = 1; i <= 9; i++)
            {
                var pressedKey = (Key)((int)Key.Digit1 + (i - 1));
                if (Keyboard.current[pressedKey].IsPressed())
                {
                    choosenWeapon = i - 1;
                    break;
                }
            }
        }

        var weaponInput = new WeaponInput
        {
            ChoosenWeapon = choosenWeapon,
            Shoot = input.Shoot.WasPressedThisFrame()
        };
        weaponSystem.UpdateInput(weaponInput);
        weaponSystem.UpdateWeapons(deltaTime);

    }
}
