using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    private BaseWeapon _currentWeapon;

    public void SetWeapon(BaseWeapon weapon)
    {
        if (_currentWeapon != null)
        {
            _currentWeapon.gameObject.SetActive(false);
        }

        _currentWeapon = weapon;
        _currentWeapon.Switch();

        if (_currentWeapon != null)
        {
            _currentWeapon.gameObject.SetActive(true);
        }
    }
}