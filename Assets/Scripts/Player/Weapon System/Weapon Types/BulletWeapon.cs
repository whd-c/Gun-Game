using UnityEngine;

public class BulletWeapon : BaseWeapon
{
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
                bullet.Initialize(weaponData.bulletSpeed, weaponData.baseDamage, weaponData.bulletSpread, muzzle);
            }
            else
            {
                Debug.LogError("Bullet prefab has no bullet script attached.");
            }
            currentAmmo--;
        }

    }
}
