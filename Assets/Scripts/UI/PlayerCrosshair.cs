using TMPro;
using UnityEngine;

public class PlayerCrosshair : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform crosshairImage;
    [SerializeField] private TextMeshProUGUI hpText;

    [Space]
    [SerializeField] private float originalSize = 1f;
    [SerializeField] private float maxSize = 2f;
    [SerializeField] private float maxSizeMargin = 0.1f;
    [SerializeField] private float shrinkRate = 5f;


    public void Initialize()
    {
        crosshairImage.localScale = new Vector3(originalSize, originalSize, 0f);
    }
    public void UpdateCrosshair(float deltaTime)
    {
        var originalVector = new Vector3(originalSize, originalSize, 0f);
        if (crosshairImage.localScale.x > originalSize)
            crosshairImage.localScale = Vector3.Lerp(crosshairImage.localScale, originalVector, 1f - Mathf.Exp(-shrinkRate * deltaTime));

        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out var hit, 100f))
        {
            if (hit.collider.gameObject.TryGetComponent<Enemy>(out var enemy))
            {
                hpText.text = $"HP: {Mathf.CeilToInt(enemy.health)}";
            }
            else
            {
                hpText.text = "";
            }
        }
    }
    public void Grow(float amount)
    {
        if (crosshairImage.localScale.x < maxSize - maxSizeMargin)
            crosshairImage.localScale += new Vector3(amount, amount, 0f);
        crosshairImage.localScale = new Vector3(Mathf.Clamp(crosshairImage.localScale.x, originalSize, maxSize), Mathf.Clamp(crosshairImage.localScale.y, originalSize, maxSize), 0f);
    }
}
