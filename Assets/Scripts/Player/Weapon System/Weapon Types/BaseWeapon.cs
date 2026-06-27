using UnityEngine;

public abstract class BaseWeapon : MonoBehaviour
{
    public WeaponData weaponData;
    protected float _timeToFire;

    public void Initialize()
    {
        _timeToFire = weaponData.drawTime;
    }

    public void UpdateWeapon(float deltaTime)
    {
        if (_timeToFire > 0f)
        {
            _timeToFire -= deltaTime;
        }
    }

    public void TryToShoot()
    {
        if (_timeToFire <= 0f)
        {
            _timeToFire = weaponData.fireRate;
            Shoot();
        }
    }

    protected abstract void Shoot();
}
