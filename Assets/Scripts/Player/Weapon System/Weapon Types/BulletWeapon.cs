using UnityEngine;

public class BulletWeapon : BaseWeapon
{
    protected override void Shoot()
    {
        GameObject bulletObj = Instantiate(weaponData.bulletPrefab, muzzle.position, muzzle.rotation);


        if (bulletObj.TryGetComponent<Bullet>(out var bullet))
        {
            bullet.Initialize(weaponData.bulletSpeed, weaponData.baseDamage, muzzle);
        }
        else
        {
            Debug.LogError("Bullet prefab has no bullet script attached.");
        }
    }
}
