using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerBullet : MonoBehaviour
{
    [Header("Config")]
    public float lifeTime = 5f;
    public int damage = 1;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.gravityScale = 0f;
    }

    // Inicializa la bala del jugador
    public void Init(Vector2 direction, float speed, int damageAmount)
    {
        damage = damageAmount;

        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction.normalized * speed;
        }

        // Mantener en Z=0 para que se vea en GameView
        transform.position = new Vector3(transform.position.x, transform.position.y, 0f);

        // Rotar sprite hacia la dirección de disparo
        float ang = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, ang);

        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // ✅ Buscar el BaseEnemy en el objeto o en sus padres
        BaseEnemy enemy = other.GetComponentInParent<BaseEnemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // ✅ Destruir en colisión con paredes
        if (other.gameObject.layer == LayerMask.NameToLayer("Walls") || other.CompareTag("Wall"))
        {
            Destroy(gameObject);
            return;
        }
    }

}