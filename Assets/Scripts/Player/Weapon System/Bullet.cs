using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    private float _damage;
    public void Initialize(float bulletSpeed, float damage, Transform muzzle)
    {
        transform.SetPositionAndRotation(muzzle.position, muzzle.rotation);
        rb.linearVelocity = muzzle.forward * bulletSpeed;

        _damage = damage;
    }
    public void OnTriggerEnter(Collider other)
    {
        Destroy(transform.gameObject);
    }
}