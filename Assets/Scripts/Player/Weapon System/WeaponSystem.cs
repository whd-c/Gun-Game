using System.Collections.Generic;
using UnityEngine;

public struct WeaponInput
{
    public int ChoosenWeapon;
}

public class WeaponSystem : MonoBehaviour
{
    [SerializeField] private List<BaseWeapon> availableWeapons;
    [SerializeField] private WeaponHolder weaponHolder;

    private int _requestedWeaponIndex;
    private int _currentWeaponIndex;

    public void Initialize()
    {
        _requestedWeaponIndex = 0;
        weaponHolder.SetWeapon(availableWeapons[_requestedWeaponIndex]);
    }

    public void UpdateInput(WeaponInput weaponInput)
    {
        _requestedWeaponIndex = weaponInput.ChoosenWeapon;
        EquipWeapon(_requestedWeaponIndex);
    }

    public void EquipWeapon(int index)
    {
        if (index < 0 || index >= availableWeapons.Count || index == _currentWeaponIndex) return;

        var choosenWeapon = availableWeapons[index];
        _currentWeaponIndex = index;
        weaponHolder.SetWeapon(choosenWeapon);
    }
}
