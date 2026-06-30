using TMPro;
using UnityEngine;

public class WeaponInfo : MonoBehaviour
{
    [SerializeField] private WeaponHolder weaponHolder;

    [Space]
    [SerializeField] private TextMeshProUGUI weaponName;
    [SerializeField] private TextMeshProUGUI weaponAmmo;
    [SerializeField] private TextMeshProUGUI weaponReloading;

    void Update()
    {
        weaponName.text = weaponHolder.currentWeapon.weaponData.name;
        weaponAmmo.text = $"Ammo: {weaponHolder.currentWeapon.currentAmmo}/{weaponHolder.currentWeapon.weaponData.maxAmmo}";
        if (weaponHolder.currentWeapon.reloading)
        {
            weaponReloading.text = "Reloading...";
        }
        else
        {
            weaponReloading.text = "";
        }
    }
}
