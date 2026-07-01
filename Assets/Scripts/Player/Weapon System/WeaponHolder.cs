using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    [HideInInspector] public BaseWeapon currentWeapon;
    private Coroutine _reloadingCoroutine;

    public void SetWeapon(BaseWeapon weapon)
    {
        if (currentWeapon != null)
        {
            currentWeapon.gameObject.SetActive(false);
            if (_reloadingCoroutine != null)
            {
                StopCoroutine(_reloadingCoroutine);
                _reloadingCoroutine = null;
            }

        }

        currentWeapon = weapon;

        if (currentWeapon != null)
        {
            currentWeapon.Switch();
            currentWeapon.gameObject.SetActive(true);
        }
    }

    public void Reload()
    {
        if ((currentWeapon.currentAmmo < currentWeapon.weaponData.maxAmmo) && !currentWeapon.reloading)
        {
            _reloadingCoroutine = StartCoroutine(currentWeapon.Reloading());

        }

    }
}