using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    [SerializeField] private Renderer enemyRenderer;
    [SerializeField] private Collider enemyCollider;

    [Space]
    [SerializeField] private float respawnTime = 2f;
    [SerializeField] private float maxHealth = 100f;
    private float _health;
    private Coroutine _changeColorCoroutine;

    public void Start()
    {
        enemyRenderer.material.color = Color.green;
        _health = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        _health -= amount;
        if (_changeColorCoroutine != null)
            StopCoroutine(_changeColorCoroutine);
        _changeColorCoroutine = StartCoroutine(ChangeColor());
        if (_health <= 0f)
            StartCoroutine(Deactivate());
    }

    private IEnumerator ChangeColor()
    {
        enemyRenderer.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        enemyRenderer.material.color = Color.green;
    }

    private IEnumerator Deactivate()
    {
        enemyRenderer.enabled = false;
        enemyCollider.enabled = false;
        yield return new WaitForSeconds(respawnTime);
        enemyRenderer.enabled = true;
        enemyCollider.enabled = true;
        _health = maxHealth;
    }

}
