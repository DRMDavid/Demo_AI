/*******************************************************
 * NOMBRE DEL ARCHIVO: Bullet.cs
 * AUTORES: Hannin Abarca, Gael Jimenez, David Sanchez
 * * DESCRIPCIÓN:
 * Controla la lógica de los proyectiles enemigos.
 * Soporta múltiples tipos de munición: Normal, Veneno, Hielo y Explosiva.
 * * REFERENCIAS:
 * - Sistema de Proyectiles Básico: https://www.udemy.com/course/aprende-a-crear-un-videojuego-de-accion-2d-con-unity/
 *******************************************************/

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Bullet : MonoBehaviour
{
    // Enum para definir el comportamiento en el Inspector
    public enum TipoBala { Normal, Veneno, Hielo, Explosiva }

    [Header("Configuración Base")]
    public float lifeTime = 3f;
    public int damage = 1;

    [Header("Configuración Especial")]
    [Tooltip("Define qué efecto aplicará esta bala.")]
    [SerializeField] public TipoBala tipo = TipoBala.Normal;

    [Header("Explosivos")]
    [SerializeField] private float radioExplosion = 2.5f;
    [SerializeField] private GameObject vfxExplosion; 

    [Header("Estados Alterados")]
    [SerializeField] private int dañoVeneno = 5;
    [SerializeField] private float duracionVeneno = 3f;
    [SerializeField] private float factorHielo = 0.5f; // 0.5 = 50% lentitud
    [SerializeField] private float duracionHielo = 2f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        if (rb != null) rb.gravityScale = 0f; // Asegurar que no caiga
    }

    /// <summary>
    /// Inicializa la bala con dirección y velocidad. Cambia el color según el tipo.
    /// </summary>
    public void Init(Vector2 direction, float speed)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = direction.normalized * speed;
        
        // Feedback visual rápido para debug
        if (sr != null)
        {
            switch (tipo)
            {
                case TipoBala.Veneno: sr.color = Color.green; break;
                case TipoBala.Hielo: sr.color = Color.cyan; break;
                case TipoBala.Explosiva: sr.color = new Color(1f, 0.5f, 0f); break; // Naranja
            }
        }

        Destroy(gameObject, lifeTime); // Auto-destrucción
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Colisión con Jugador
        if (collision.CompareTag("Player"))
        {
            AplicarEfectos(collision.gameObject);

            // Explosivas explotan, el resto se destruye
            if (tipo == TipoBala.Explosiva) Explotar();
            else Destroy(gameObject);
        }
        // Colisión con Muros (Tag corregido a "Wall")
        else if (collision.CompareTag("Wall"))
        {
            if (tipo == TipoBala.Explosiva) Explotar();
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Aplica daño y efectos de estado al objetivo.
    /// </summary>
    private void AplicarEfectos(GameObject target)
    {
        PlayerSalud ps = target.GetComponent<PlayerSalud>();
        PlayerStatusManager status = target.GetComponent<PlayerStatusManager>();

        // Daño base inmediato
        if (ps != null)
        {
            ps.RecibirDamage(damage);
        }

        // Efectos secundarios
        if (status != null)
        {
            switch (tipo)
            {
                case TipoBala.Veneno:
                    status.AplicarVeneno(dañoVeneno, duracionVeneno);
                    break;
                case TipoBala.Hielo:
                    status.AplicarCongelacion(factorHielo, duracionHielo);
                    break;
            }
        }
    }

    private void Explotar()
    {
        // Instanciar efecto visual
        if (vfxExplosion != null) Instantiate(vfxExplosion, transform.position, Quaternion.identity);

        // Daño en área
        Collider2D[] afectados = Physics2D.OverlapCircleAll(transform.position, radioExplosion);
        foreach (var col in afectados)
        {
            if (col.CompareTag("Player"))
            {
                // Doble daño por explosión
                col.GetComponent<PlayerSalud>().RecibirDamage(damage * 2);
            }
        }
        Destroy(gameObject);
    }
}