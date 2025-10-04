using UnityEngine;
using System.Collections;

public class LightEnemy : BaseEnemy
{
    [Header("Light Enemy Settings")]
    public float acceleration = 5f;
    public float maxSpeed = 3f;
    public float fleeDuration = 3f;
    public float restDuration = 2f;
    public float shootingCooldown = 2f;
    public float predictionTime = 1f;
    public GameObject bulletPrefab;
    public float bulletSpeed = 6f;

    private Rigidbody2D _rb;
    private GameObject _player;
    private bool isFleeing = false;
    private bool canShoot = true;

    // 👇 para el parpadeo
    private SpriteRenderer spriteRenderer;
    private Coroutine blinkCoroutine;

    // --- START ---
    protected override void Start()
    {
        base.Start();
        _rb = GetComponent<Rigidbody2D>();
        if (_rb == null)
            Debug.LogError("LightEnemy requires a Rigidbody2D on the prefab.");

        if (_steeringBehaviors == null)
            Debug.LogError("LightEnemy requires RigidbodySteeringBehaviours on the prefab.");

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            Debug.LogWarning("LightEnemy: No SpriteRenderer found, blink effect won’t work.");

        StartCoroutine(FleeCycle());
    }

    // --- FLEE/REST CYCLE ---
    private IEnumerator FleeCycle()
    {
        while (true)
        {
            // Flee
            isFleeing = true;
            if (blinkCoroutine != null) StopCoroutine(blinkCoroutine); // parar parpadeo si estaba
            if (spriteRenderer != null) spriteRenderer.color = Color.white; // reset color
            yield return new WaitForSeconds(fleeDuration);

            // Rest
            isFleeing = false;
            if (_rb != null) _rb.linearVelocity = Vector2.zero;
            if (_steeringBehaviors != null)
                _steeringBehaviors.currentBehavior = ESteeringBehaviors.DontMove;

            // 👉 inicia el parpadeo azul durante el descanso
            if (spriteRenderer != null)
                blinkCoroutine = StartCoroutine(BlinkBlue(restDuration));

            yield return new WaitForSeconds(restDuration);
        }
    }

    // --- BLINK BLUE ---
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

        spriteRenderer.color = Color.white; // reset color al terminar
    }

    // --- UPDATE ---
    void Update()
    {
        if (_steeringBehaviors == null) return;

        // Find player
        if (_player == null)
        {
            var players = GameObject.FindGameObjectsWithTag("Player");
            if (players.Length > 0)
                _player = players[0];
        }

        if (_player == null) return;

        Rigidbody2D playerRb = _player.GetComponent<Rigidbody2D>();
        if (playerRb == null) return;

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

        if (canShoot)
        {
            StartCoroutine(ShootAtPredictedPosition(playerRb));
        }

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

    // --- PREDICTIVE SHOOTING ---
    private IEnumerator ShootAtPredictedPosition(Rigidbody2D playerRb)
    {
        canShoot = false;

        if (bulletPrefab != null && playerRb != null)
        {
            Vector2 predictedPosition = (Vector2)_player.transform.position + playerRb.linearVelocity * predictionTime;
            Vector2 direction = (predictedPosition - (Vector2)transform.position).normalized;

            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
                bulletRb.linearVelocity = direction * bulletSpeed;
        }

        yield return new WaitForSeconds(shootingCooldown);
        canShoot = true;
    }
}
