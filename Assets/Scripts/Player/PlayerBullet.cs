/*******************************************************
 * NOMBRE DEL ARCHIVO: PlayerBullet.cs
 * AUTOR: Gael, Steve y David
 * CURSO: Desarrollo de videojuego de Acción 2D con Unity
 * 
 * DESCRIPCIÓN:
 * Script encargado del comportamiento de las balas del jugador.
 * Controla su movimiento, daño y destrucción al impactar con enemigos
 * o con paredes. Este script se asocia al prefab de la bala.
 * 
 * FUENTES CONSULTADAS:
 * - Consultas menores a IA (ChatGPT) para resolución de errores específicos
 *   en temas de Rigidbody2D y detección de colisiones con OnTriggerEnter2D.
 * - No se usaron fragmentos directos de código externo.
 * 
 * FECHA: 05/10
 *******************************************************/

using UnityEngine;

// Requiere que el objeto tenga Rigidbody2D y Collider2D para funcionar correctamente.
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerBullet : MonoBehaviour
{
    [Header("Config")]
    public float lifeTime = 5f; // Tiempo que la bala permanecerá activa antes de destruirse automáticamente.
    public int damage = 1;      // Daño que la bala inflige a los enemigos.

    private Rigidbody2D rb;     // Referencia al Rigidbody2D de la bala.

    private void Awake()
    {
        // Se obtiene el componente Rigidbody2D y se desactiva la gravedad para evitar que caiga.
        rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.gravityScale = 0f;
    }

    /// <summary>
    /// Inicializa la bala con dirección, velocidad y daño definidos.
    /// </summary>
    /// <param name="direction">Dirección del disparo.</param>
    /// <param name="speed">Velocidad de la bala.</param>
    /// <param name="damageAmount">Cantidad de daño que infligirá.</param>
    public void Init(Vector2 direction, float speed, int damageAmount)
    {
        damage = damageAmount; // Se asigna el daño recibido al crear la bala.

        // Verifica y obtiene el Rigidbody2D si aún no está asignado.
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        // Aplica movimiento en la dirección normalizada multiplicada por la velocidad.
        if (rb != null)
        {
            rb.linearVelocity = direction.normalized * speed;
        }

        // Mantiene la posición en el eje Z = 0 para evitar que desaparezca del plano 2D.
        transform.position = new Vector3(transform.position.x, transform.position.y, 0f);

        // Rota el sprite de la bala para que apunte hacia la dirección del disparo.
        float ang = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, ang);

        // Destruye la bala después del tiempo de vida definido.
        Destroy(gameObject, lifeTime);
    }

    /// <summary>
    /// Detecta colisiones con enemigos o paredes y aplica las acciones correspondientes.
    /// </summary>
    /// <param name="other">El collider del objeto con el que colisiona la bala.</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Busca un componente BaseEnemy en el objeto o en sus padres.
        BaseEnemy enemy = other.GetComponentInParent<BaseEnemy>();
        if (enemy != null)
        {
            // Si encuentra un enemigo, aplica daño y destruye la bala.
            enemy.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // Si la bala impacta con una pared (por layer o tag), se destruye.
        if (other.gameObject.layer == LayerMask.NameToLayer("Walls") || other.CompareTag("Wall"))
        {
            Destroy(gameObject);
            return;
        }
    }
}
