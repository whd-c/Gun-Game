using UnityEngine;

public struct CameraInput
{
    public Vector2 Look;
}

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;

    [Space]
    [SerializeField] private float sensitivity = 0.1f;
    [Range(0f, 90f)]
    [SerializeField] private float clampedPitch = 85f;
    [Range(0f, 180f)]


    [Space]
    [SerializeField] private float minFieldOfView = 60f;
    [Range(0f, 180f)]
    [SerializeField] private float maxFieldOfView = 90f;

    [SerializeField] private float velocityMin = 15f;
    [SerializeField] private float velocityPeak = 60f;
    [SerializeField] private float fieldOfViewResponse = 5f;
    private Vector3 _eulerAngles;

    public void Initialize(Transform target)
    {
        transform.position = target.position;
        transform.eulerAngles = _eulerAngles = target.eulerAngles;
    }

    public void UpdateRotation(CameraInput input)
    {
        _eulerAngles += new Vector3(-input.Look.y, input.Look.x) * sensitivity;
        _eulerAngles.x = Mathf.Clamp(_eulerAngles.x, -clampedPitch, clampedPitch);
        transform.eulerAngles = _eulerAngles;
    }

    public void UpdatePosition(Transform target)
    {
        transform.position = target.position;
    }

    public void UpdateCamera(float deltaTime, Vector3 velocity, Vector3 up)
    {
        //get rid of vertical movement
        var planarVelocity = Vector3.ProjectOnPlane(velocity, up);

        var velocityRatio = Mathf.InverseLerp(velocityMin, velocityPeak, planarVelocity.magnitude);
        var targetFieldOfView = Mathf.Lerp(minFieldOfView, maxFieldOfView, velocityRatio);

        mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFieldOfView, 1f - Mathf.Exp(-fieldOfViewResponse * deltaTime));
    }
}
