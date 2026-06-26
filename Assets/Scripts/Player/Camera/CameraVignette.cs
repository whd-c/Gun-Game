using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraVignette : MonoBehaviour
{
    [SerializeField] private float minVignette = 0.3f;
    [SerializeField] private float maxVignette = 0.6f;
    [SerializeField] private float velocityPeak = 80f;
    [SerializeField] private float vignetteResponse = 5f;

    private VolumeProfile _volumeProfile;
    private Vignette _vignette;
    public void Initialize(VolumeProfile volumeProfile)
    {
        _volumeProfile = volumeProfile;

        if (!_volumeProfile.TryGet(out _vignette))
        {
            _vignette = _volumeProfile.Add<Vignette>();
        }
        _vignette.intensity.Override(minVignette);
    }

    public void UpdateVignette(float deltaTime, Vector3 velocity)
    {
        var velocityRatio = Mathf.InverseLerp(0, velocityPeak, velocity.magnitude);
        var targetVignette = Mathf.Lerp(minVignette, maxVignette, velocityRatio);

        _vignette.intensity.value = Mathf.Lerp(_vignette.intensity.value, targetVignette, 1f - Mathf.Exp(-vignetteResponse * deltaTime));
    }
}
