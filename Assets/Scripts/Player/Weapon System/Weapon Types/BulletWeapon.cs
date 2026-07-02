using UnityEngine;

public class BulletWeapon : BaseWeapon
{
    [SerializeField] private bool enableBulletSparks;
    protected override void Shoot()
    {
        for (int i = 0; i < weaponData.bulletsPerShot; i++)
        {
            if (currentAmmo <= 0)
            {
                break;
            }
            GameObject bulletObj = Instantiate(weaponData.bulletPrefab, muzzle.position, muzzle.rotation);
            if (bulletObj.TryGetComponent<Bullet>(out var bullet))
            {
                bullet.Initialize(weaponData.bulletSpeed, weaponData.baseDamage, weaponData.bulletSpread, muzzle, enableBulletSparks);
            }
            else
            {
                Debug.LogError("Bullet prefab has no bullet script attached.");
            }
            currentAmmo--;
        }

    }
}
