using UnityEngine;

public abstract class BaseWeapon : MonoBehaviour
{
    public WeaponData weaponData;

    [SerializeField] protected Transform muzzle;
    protected int _currentAmmo;
    private float _timeToFire;
    private float _drawingTime;

    public void Initialize()
    {
        _currentAmmo = weaponData.maxAmmo;
    }

    public void Switch()
    {
        _timeToFire = 0f;
        _drawingTime = weaponData.drawTime;
    }

    public void UpdateWeapon(float deltaTime)
    {
        if (_drawingTime >= 0f)
            _drawingTime -= deltaTime;
        if (_timeToFire >= 0f)
            _timeToFire -= deltaTime;
    }

    public void TryToShoot()
    {
        var canFire = _timeToFire <= 0f && _drawingTime <= 0f;
        if (canFire && _currentAmmo > 0)
        {
            _timeToFire = weaponData.fireRate;
            Shoot();
            _currentAmmo--;
        }
    }

    protected abstract void Shoot();
}
