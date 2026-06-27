using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponData", menuName = "Weapons/Data")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public float fireRate;
    public float baseDamage;
    public float drawTime;
}
