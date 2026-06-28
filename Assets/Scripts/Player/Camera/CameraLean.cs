using UnityEngine;

public class CameraLean : MonoBehaviour
{
    [SerializeField] private float attackDamping = 0.5f;
    [SerializeField] private float decayDamping = 0.3f;

    [Space]
    [SerializeField] private float walkStrength = 0.075f;
    [SerializeField] private float slideStrength = 0.25f;
    [SerializeField] private float jumpStrength = 0.2f;
    [SerializeField] private float strengthResponse = 5f;
    private Vector3 _dampedAcceleration;
    private Vector3 _dampedAccelerationVel;
    private float _smoothStrength;

    public void Initialize()
    {
        _smoothStrength = walkStrength;
    }

    public void UpdateLean(float deltaTime, CharacterState state, Vector3 up)
    {
        var planarAcceleration = Vector3.ProjectOnPlane(state.Acceleration, up);
        var damping = planarAcceleration.magnitude > _dampedAcceleration.magnitude
        ? attackDamping : decayDamping;

        _dampedAcceleration = Vector3.SmoothDamp(
            current: _dampedAcceleration,
            target: planarAcceleration,
            currentVelocity: ref _dampedAccelerationVel,
            smoothTime: damping,
            maxSpeed: float.PositiveInfinity,
            deltaTime: deltaTime
        );

        var leanAxis = Vector3.Cross(_dampedAcceleration.normalized, up).normalized;
        transform.localRotation = Quaternion.identity;

        var sliding = state.Stance is Stance.Slide;
        var jumping = !state.Grounded;

        var targetStrength = walkStrength;

        if (sliding)
        {
            targetStrength = slideStrength;
        }
        else if (jumping)
        {
            targetStrength = jumpStrength;
        }

        _smoothStrength = Mathf.Lerp(_smoothStrength, targetStrength, 1f - Mathf.Exp(-strengthResponse * deltaTime));

        transform.rotation = Quaternion.AngleAxis(_dampedAcceleration.magnitude * _smoothStrength, leanAxis) * transform.rotation;
    }
}
