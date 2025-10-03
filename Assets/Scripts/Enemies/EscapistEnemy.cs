using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class EscapistEnemy : BaseEnemy
{
    [Header("Movement")]
    public float maxSpeed = 3f;           // Velocidad máxima (no muy alta)
    public float maxForce = 5f;           // Fuerza de aceleración (rápida)
    public float fleeDuration = 2f;       // Tiempo que huye
    public float restDuration = 1f;       // Tiempo que descansa
    public float obstacleAvoidDistance = 1f; // Distancia para evitar obstáculos
    public LayerMask obstacleMask;        // Capa de obstáculos

    [Header("Shoot")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 6f;
    public float predictionTime = 0.5f;   // Tiempo para predecir posición futura del player
    public float fireRate = 1f;

    private Rigidbody2D rb;
    private Transform player;
    private Vector2 velocity;             // Para suavizar movimiento
    private bool isFleeing = true;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Start()
    {
        StartCoroutine(FleeCycle());
        StartCoroutine(ShootCycle());
    }

    IEnumerator FleeCycle()
    {
        while (true)
        {
            // Huye
            isFleeing = true;
            float timer = 0f;
            while (timer < fleeDuration)
            {
                if (player != null)
                {
                    Vector2 desiredDir = ((Vector2)transform.position - (Vector2)player.position).normalized;
                    desiredDir = AvoidObstacles(desiredDir);

                    Vector2 desiredVelocity = desiredDir * maxSpeed;
                    Vector2 steering = desiredVelocity - velocity;
                    steering = Vector2.ClampMagnitude(steering, maxForce);

                    velocity = Vector2.ClampMagnitude(velocity + steering * Time.deltaTime, maxSpeed);
                    rb.linearVelocity = velocity;
                }

                timer += Time.deltaTime;
                yield return null;
            }

            // Descansa
            isFleeing = false;
            rb.linearVelocity = Vector2.zero;
            velocity = Vector2.zero;
            yield return new WaitForSeconds(restDuration);
        }
    }

    IEnumerator ShootCycle()
    {
        while (true)
        {
            if (player != null)
            {
                // Predecir posición futura del jugador
                Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
                Vector2 futurePos = (Vector2)player.position;
                if (playerRb != null)
                    futurePos += playerRb.linearVelocity * predictionTime;

                Vector2 dir = (futurePos - (Vector2)firePoint.position).normalized;

                // Instanciar bala
                GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
                Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
                if (bulletRb != null)
                {
                    bulletRb.gravityScale = 0f;
                    bulletRb.linearVelocity = dir * bulletSpeed;
                }

                // Asignar daño
                Bullet bulletScript = bullet.GetComponent<Bullet>();
                if (bulletScript != null)
                    bulletScript.damage = contactDamage;
            }

            yield return new WaitForSeconds(1f / fireRate);
        }
    }

    // Evita obstáculos usando raycast
    Vector2 AvoidObstacles(Vector2 desiredDir)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, desiredDir, obstacleAvoidDistance, obstacleMask);
        if (hit.collider != null)
        {
            // Cambia dirección ligeramente hacia la derecha o izquierda
            Vector2 perp = Vector2.Perpendicular(desiredDir).normalized;
            desiredDir = (desiredDir + perp * 0.5f).normalized;
        }
        return desiredDir;
    }
}
