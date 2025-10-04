using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Bullet : MonoBehaviour
{
    [Header("Config")]
    public float lifeTime = 3f;
    public int damage = 1;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.gravityScale = 0f;
    }

    // Llamar inmediatamente después de Instantiate
    public void Init(Vector2 direction, float speed)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = direction.normalized * speed;
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerSalud ps = collision.GetComponent<PlayerSalud>();
            if (ps != null)
            {
                // Ajusta el método según tu PlayerSalud (RecibirDamage o RecibirDaño)
                ps.RecibirDamage(damage);
            }
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}