using UnityEngine;
using System.Collections;

/*
Integrantes del equipo:
- Hannin Abarca
- David Sánchez
- Gael Jiménez
*/

/// <summary>
/// Enemigo ligero especializado en huir y disparar proyectiles prediciendo la posición del jugador.
/// Hereda de BaseEnemy, por lo que ya tiene salud, daño y efectos visuales de recibir daño.
/// </summary>
public class LightEnemy : BaseEnemy
{
    [Header("Light Enemy Settings")]
    public float acceleration = 5f;        // Aceleración al moverse
    public float maxSpeed = 3f;           // Velocidad máxima
    public float fleeDuration = 3f;       // Tiempo que pasa huyendo
    public float restDuration = 2f;       // Tiempo que pasa descansando
    public float shootingCooldown = 2f;   // Tiempo entre disparos
    public float predictionTime = 1f;     // Tiempo usado para predecir la posición del jugador
    public GameObject bulletPrefab;       // Prefab del proyectil
    public float bulletSpeed = 6f;        // Velocidad del proyectil

    // --- REFERENCIAS ---
    private Rigidbody2D _rb;              // Rigidbody2D del enemigo
    private GameObject _player;           // Referencia al jugador
    private bool isFleeing = false;       // Flag que indica si está huyendo
    private bool canShoot = true;         // Flag para controlar cooldown de disparo

    // Para el parpadeo visual durante el descanso
    private SpriteRenderer spriteRenderer;
    private Coroutine blinkCoroutine;

    // --- START ---
    protected override void Start()
    {
        base.Start(); // Llama a Start() de BaseEnemy para inicializar salud y componentes
        _rb = GetComponent<Rigidbody2D>();
        if (_rb == null)
            Debug.LogError("LightEnemy requires a Rigidbody2D on the prefab.");

        if (_steeringBehaviors == null)
            Debug.LogError("LightEnemy requires RigidbodySteeringBehaviours on the prefab.");

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            Debug.LogWarning("LightEnemy: No SpriteRenderer found, blink effect won’t work.");

        // Iniciamos el ciclo Flee/Rest
        StartCoroutine(FleeCycle());
    }

    // --- CICLO DE HUIR Y DESCANSAR ---
    private IEnumerator FleeCycle()
    {
        while (true)
        {
            // --- HUIR ---
            isFleeing = true;
            if (blinkCoroutine != null) StopCoroutine(blinkCoroutine); // detener parpadeo si estaba activo
            if (spriteRenderer != null) spriteRenderer.color = Color.white; // reset color al blanco
            yield return new WaitForSeconds(fleeDuration);

            // --- DESCANSAR ---
            isFleeing = false;
            if (_rb != null) _rb.linearVelocity = Vector2.zero; // detener movimiento
            if (_steeringBehaviors != null)
                _steeringBehaviors.currentBehavior = ESteeringBehaviors.DontMove;

            // Inicia parpadeo azul durante el descanso
            if (spriteRenderer != null)
                blinkCoroutine = StartCoroutine(BlinkBlue(restDuration));

            yield return new WaitForSeconds(restDuration);
        }
    }

    // --- PARPADEO AZUL DURANTE EL DESCANSO ---
    private IEnumerator BlinkBlue(float duration)
    {
        float elapsed = 0f;
        bool toggle = false;

        while (elapsed < duration)
        {
            toggle = !toggle;
            spriteRenderer.color = toggle ? Color.blue : Color.white;
            yield return new WaitForSeconds(0.3f); // velocidad del parpadeo
            elapsed += 0.3f;
        }

        spriteRenderer.color = Color.white; // reset al terminar
    }

    // --- UPDATE ---
    void Update()
    {
        if (_steeringBehaviors == null) return;

        // --- BUSCAR JUGADOR ---
        if (_player == null)
        {
            var players = GameObject.FindGameObjectsWithTag("Player");
            if (players.Length > 0)
                _player = players[0];
        }

        if (_player == null) return; // si no hay jugador, no hacer nada

        Rigidbody2D playerRb = _player.GetComponent<Rigidbody2D>();
        if (playerRb == null) return;

        // --- COMPORTAMIENTO DE HUIR ---
        if (isFleeing)
        {
            _steeringBehaviors.currentBehavior = ESteeringBehaviors.Flee;
            _steeringBehaviors.useObstacleAvoidance = true;
            _steeringBehaviors.SetTarget(_player.transform.position, playerRb);
        }
        else
        {
            _steeringBehaviors.useObstacleAvoidance = false;
        }

        // --- DISPARO PREDICTIVO ---
        if (canShoot)
        {
            StartCoroutine(ShootAtPredictedPosition(playerRb));
        }

        // --- LIMITAR VELOCIDAD Y APLICAR ACELERACIÓN ---
        if (_rb != null)
        {
            _rb.linearVelocity = Vector2.ClampMagnitude(_rb.linearVelocity, maxSpeed);
            _rb.linearVelocity = Vector2.Lerp(
                _rb.linearVelocity,
                _rb.linearVelocity.normalized * maxSpeed,
                acceleration * Time.deltaTime
            );
        }
    }

    // --- DISPARO PREDICTIVO AL JUGADOR ---
    private IEnumerator ShootAtPredictedPosition(Rigidbody2D playerRb)
    {
        canShoot = false;

        if (bulletPrefab != null && playerRb != null)
        {
            // Calculamos posición futura del jugador
            Vector2 predictedPosition = (Vector2)_player.transform.position + playerRb.linearVelocity * predictionTime;
            Vector2 direction = (predictedPosition - (Vector2)transform.position).normalized;

            // Instanciamos proyectil y le aplicamos velocidad
            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
                bulletRb.linearVelocity = direction * bulletSpeed;
        }

        yield return new WaitForSeconds(shootingCooldown);
        canShoot = true;
    }
}
