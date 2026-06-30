using System.Collections.Generic;
using UnityEngine;

public struct WeaponInput
{
    public int ChoosenWeapon;
    public bool Shoot;
    public bool Reload;
}

public class WeaponSystem : MonoBehaviour
{
    [SerializeField] private List<BaseWeapon> weaponPrefabs;
    [SerializeField] private WeaponHolder weaponHolder;

    private List<BaseWeapon> _spawnedWeapons = new();
    private int _requestedWeaponIndex = -1;
    private bool _requestedToShoot = false;
    private bool _requestedToReload = false;
    private int _currentWeaponIndex = -1;

    public void Initialize()
    {
        foreach (var prefab in weaponPrefabs)
        {
            BaseWeapon spawnedWeapon = Instantiate(prefab, weaponHolder.transform);
            spawnedWeapon.gameObject.SetActive(false);
            spawnedWeapon.Initialize();
            _spawnedWeapons.Add(spawnedWeapon);
        }

        EquipWeapon(0);
    }

    public void UpdateInput(WeaponInput weaponInput)
    {
        _requestedWeaponIndex = weaponInput.ChoosenWeapon;
        _requestedToShoot = weaponInput.Shoot;
        _requestedToReload = weaponInput.Reload;

        if (_requestedToShoot)
        {
            _spawnedWeapons[_currentWeaponIndex].TryToShoot();
        }
        else if (_requestedToReload)
        {
            weaponHolder.Reload();
        }
        EquipWeapon(_requestedWeaponIndex);
    }

    public void UpdateWeapons(float deltaTime)
    {
        _spawnedWeapons[_currentWeaponIndex].UpdateWeapon(deltaTime);
    }

    public void EquipWeapon(int index)
    {
        if (index < 0 || index >= _spawnedWeapons.Count || index == _currentWeaponIndex) return;

        _currentWeaponIndex = index;
        weaponHolder.SetWeapon(_spawnedWeapons[_currentWeaponIndex]);
    }
}
