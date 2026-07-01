using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponData", menuName = "Weapons/Data")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public float fireRate;
    public bool isAutomatic;
    public int bulletsPerShot;
    public float baseDamage;
    public int maxAmmo;
    public float bulletSpeed;
    public float bulletSpread;
    public float drawTime;
    public float reloadTime;
    public GameObject bulletPrefab;
}
