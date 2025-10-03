using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BaseEnemy : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    public float currentHealth;
    public int contactDamage = 10;

    [Header("Death Effect")]
    public GameObject deathEffectPrefab;

    protected Rigidbody2D rb;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
    }

    public virtual void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }

    protected void DealContactDamage(GameObject player)
    {
        PlayerSalud salud = player.GetComponent<PlayerSalud>();
        if (salud != null)
        {
            salud.RecibirDaño(contactDamage);
        }
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            DealContactDamage(collision.gameObject);
        }
    }
}
