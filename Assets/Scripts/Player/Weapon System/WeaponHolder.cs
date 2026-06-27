using UnityEngine;

public class WeaponHolder : MonoBehaviour
{

    private BaseWeapon _currentWeapon;

    public void SetWeapon(BaseWeapon weapon)
    {
        if (weapon == null) return;
        ClearCurrentWeapon();

        _currentWeapon = Instantiate(weapon, transform);

        _currentWeapon.transform.localPosition = Vector3.zero;
        _currentWeapon.transform.localRotation = Quaternion.identity;
    }
    public void ClearCurrentWeapon()
    {
        if (_currentWeapon != null)
        {
            Destroy(_currentWeapon.gameObject);
            _currentWeapon = null;
        }
    }
}