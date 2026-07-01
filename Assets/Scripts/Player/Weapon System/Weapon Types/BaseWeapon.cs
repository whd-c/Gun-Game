using System.Collections;
using UnityEngine;

public abstract class BaseWeapon : MonoBehaviour
{
    public WeaponData weaponData;
    public int currentAmmo;
    [HideInInspector] public bool reloading;
    [SerializeField] protected Transform muzzle;
    private float _timeToFire;
    private float _drawingTime;


    public void Initialize()
    {
        reloading = false;
        currentAmmo = weaponData.maxAmmo;
    }

    public void Switch()
    {
        reloading = false;
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
        var canFire = _timeToFire <= 0f && _drawingTime <= 0f && !reloading;
        if (canFire && currentAmmo > 0)
        {
            _timeToFire = weaponData.fireRate;
            Shoot();
        }
    }

    public IEnumerator Reloading()
    {
        reloading = true;
        yield return new WaitForSeconds(weaponData.reloadTime);
        currentAmmo = weaponData.maxAmmo;
        reloading = false;
    }

    protected abstract void Shoot();
}
