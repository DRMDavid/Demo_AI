using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Bullet : MonoBehaviour
{
    // Tipos de balas disponibles para el Boss
    public enum TipoBala { Normal, Veneno, Hielo, Explosiva }

    [Header("Configuración Base")]
    public float lifeTime = 3f;
    public int damage = 1;

    [Header("Configuración Especial")]
    [Tooltip("Elige el comportamiento de esta bala.")]
    [SerializeField] public TipoBala tipo = TipoBala.Normal;

    [Header("Explosivos")]
    [SerializeField] private float radioExplosion = 2.5f;
    [SerializeField] private GameObject vfxExplosion; // Arrastra tu prefab de explosión aquí

    [Header("Estados Alterados")]
    [SerializeField] private int dañoVeneno = 5;
    [SerializeField] private float duracionVeneno = 3f;
    [SerializeField] private float factorHielo = 0.5f; // 50% velocidad
    [SerializeField] private float duracionHielo = 2f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>(); // Para cambiar color si es necesario
        if (rb != null) rb.gravityScale = 0f;
    }

    // Llamar inmediatamente después de Instantiate
    public void Init(Vector2 direction, float speed)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = direction.normalized * speed;
        
        // Opcional: Cambiar color según tipo para debug visual rápido
        if (sr != null)
        {
            switch (tipo)
            {
                case TipoBala.Veneno: sr.color = Color.green; break;
                case TipoBala.Hielo: sr.color = Color.cyan; break;
                case TipoBala.Explosiva: sr.color = new Color(1f, 0.5f, 0f); break; // Naranja
            }
        }

        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Lógica para aplicar efectos al jugador
            AplicarEfectos(collision.gameObject);

            // Si es explosiva, explota; si no, se destruye normal
            if (tipo == TipoBala.Explosiva) Explotar();
            else Destroy(gameObject);
        }
        else if (collision.CompareTag("Wall") || collision.CompareTag("Suelo"))
        {
            if (tipo == TipoBala.Explosiva) Explotar();
            Destroy(gameObject);
        }
    }

    private void AplicarEfectos(GameObject target)
    {
        PlayerSalud ps = target.GetComponent<PlayerSalud>();
        PlayerStatusManager status = target.GetComponent<PlayerStatusManager>();

        if (ps != null)
        {
            // Daño base
            ps.RecibirDamage(damage);
        }

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
        Debug.Log("BOOM! Bala explosiva.");
        if (vfxExplosion != null) Instantiate(vfxExplosion, transform.position, Quaternion.identity);

        Collider2D[] afectados = Physics2D.OverlapCircleAll(transform.position, radioExplosion);
        foreach (var col in afectados)
        {
            if (col.CompareTag("Player"))
            {
                // Daño extra por explosión
                col.GetComponent<PlayerSalud>().RecibirDamage(damage * 2);
            }
        }
        Destroy(gameObject);
    }
}