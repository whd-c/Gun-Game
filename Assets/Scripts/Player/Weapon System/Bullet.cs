using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float _damage;
    private float _bulletSpeed;
    public void Initialize(float bulletSpeed, float damage, Transform muzzle)
    {
        transform.SetPositionAndRotation(muzzle.position, muzzle.rotation);
        _damage = damage;
        _bulletSpeed = bulletSpeed;
        Destroy(transform.gameObject, 5f);
    }

    void Update()
    {
        var moveDistance = _bulletSpeed * Time.deltaTime;

        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, moveDistance))
        {
            OnBulletHit(hit);
        }
        else
        {
            transform.Translate(Vector3.forward * moveDistance);
        }
    }
    private void OnBulletHit(RaycastHit hit)
    {
        transform.position = hit.point;
        if (hit.collider.gameObject.TryGetComponent<Enemy>(out var enemy))
        {
            enemy.TakeDamage(_damage);
        }
        Destroy(transform.gameObject);
    }
}


