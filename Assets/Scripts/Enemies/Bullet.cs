using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Bullet : MonoBehaviour
{
    [Header("Config")]
    public float lifeTime = 10f;    // Tiempo antes de autodestruir la bala
    public float damage = 10f;     // Daño que hará la bala

    private void Start()
    {
        // Destruye la bala después de lifeTime segundos
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Si choca con el jugador
        if (collision.CompareTag("Player"))
        {
            PlayerSalud playerSalud = collision.GetComponent<PlayerSalud>();
            if (playerSalud != null)
            {
                // Aplica daño al player
                playerSalud.RecibirDaño(damage);
            }

            Destroy(gameObject); // Destruye la bala al impactar
        }
        // Si choca con pared u otro obstáculo
        else if (collision.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
