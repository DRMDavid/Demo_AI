using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    [Header("Configuración del Proyectil")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public int proyectilDamage = 2;
    public float proyectilSpeed = 12f;
    public float fireRate = 0.25f;

    [Header("Referencias Visuales")]
    public Transform armaSprite; // El sprite del arma que rota

    [Header("Sonido de Disparo")]
    public AudioClip shootSound; // El clip de audio para el disparo

    // --- Variables Privadas ---
    private float lastFireTime;
    private AudioSource audioSource; // El componente que reproduce el sonido

    // Awake se ejecuta antes que Start, asegurando que el AudioSource esté listo
    private void Awake()
    {
        // Busca el componente AudioSource en este objeto
        audioSource = GetComponent<AudioSource>();
        
        // Si no encuentra uno, lo añade automáticamente para evitar errores
        if (audioSource == null) 
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void Update()
    {
        // Mantener presionado el click izquierdo para disparar
        if (Input.GetMouseButton(0)) 
        {
            TryShoot();
        }

        // Apuntar el arma hacia el mouse
        ApuntarArma();
    }

    private void TryShoot()
    {
        // Revisa si ha pasado suficiente tiempo desde el último disparo (controla el fire rate)
        if (Time.time < lastFireTime + fireRate)
        {
            return;
        } 
        
        // Revisa si las referencias necesarias existen
        if (bulletPrefab == null || firePoint == null)
        {
            return;
        }

        // Actualiza el tiempo del último disparo
        lastFireTime = Time.time;

        // --- Instanciar la Bala ---
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        Vector2 dir = (mouseWorld - firePoint.position).normalized;

        GameObject go = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        PlayerBullet bullet = go.GetComponent<PlayerBullet>();
        
        if (bullet != null)
        {
            bullet.Init(dir, proyectilSpeed, proyectilDamage);
        }
        else
        {
            Debug.LogError("El prefab de la bala NO tiene el script PlayerBullet!");
        }

        // --- Reproducir Sonido de Disparo ---
        if (shootSound != null)
        {
            // Usa PlayOneShot para que los sonidos no se corten entre sí si disparas rápido
            audioSource.PlayOneShot(shootSound);
        }
    }

    private void ApuntarArma()
    {
        if (armaSprite == null) return;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;
        Vector2 direccion = mouseWorld - armaSprite.position;
        float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;

        armaSprite.rotation = Quaternion.Euler(0f, 0f, angulo);
    }

    private void OnDrawGizmos()
    {
        if (firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(firePoint.position, 0.05f);

            if (Application.isPlaying && Camera.main != null)
            {
                Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mouseWorld.z = 0;
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(firePoint.position, mouseWorld);
            }
        }
    }
}