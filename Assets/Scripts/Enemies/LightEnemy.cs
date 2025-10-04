using UnityEngine;
using System.Collections;

public class LightEnemy : BaseEnemy
{
    [Header("Light Enemy Settings")]
    public float acceleration = 5f;        // Fast acceleration
    public float maxSpeed = 3f;            // Lower max speed (feels light)
    public float fleeDuration = 3f;        // How long it flees (X seconds)
    public float restDuration = 2f;        // How long it rests (Y seconds)
    public float shootingCooldown = 2f;    // Time between shots
    public float predictionTime = 1f;      // How many seconds ahead to predict player’s position
    public GameObject bulletPrefab;        // Bullet prefab
    public float bulletSpeed = 6f;         // Speed of the bullet

    private Rigidbody2D _rb;
    private GameObject _player;
    private bool isFleeing = false;
    private bool canShoot = true;

    // --- START ---
    protected override void Start()
    {
        base.Start();
        _rb = GetComponent<Rigidbody2D>();
        if (_rb == null)
            Debug.LogError("LightEnemy requires a Rigidbody2D on the prefab.");

        if (_steeringBehaviors == null)
            Debug.LogError("LightEnemy requires RigidbodySteeringBehaviours on the prefab.");

        StartCoroutine(FleeCycle()); // start flee/rest cycle
    }

    // --- FLEE/REST CYCLE ---
    private IEnumerator FleeCycle()
    {
        while (true)
        {
            // Flee
            isFleeing = true;
            yield return new WaitForSeconds(fleeDuration);

            // Rest
            isFleeing = false;
            if (_rb != null) _rb.linearVelocity = Vector2.zero;
            if (_steeringBehaviors != null)
                _steeringBehaviors.currentBehavior = ESteeringBehaviors.DontMove;

            yield return new WaitForSeconds(restDuration);
        }
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
            // (A) Flee from player
            _steeringBehaviors.currentBehavior = ESteeringBehaviors.Flee;

            // (D) Obstacle avoidance only during flee
            _steeringBehaviors.useObstacleAvoidance = true;

            _steeringBehaviors.SetTarget(_player.transform.position, playerRb);
        }
        else
        {
            _steeringBehaviors.useObstacleAvoidance = false;
        }

        // (B) Predictive shooting
        if (canShoot)
        {
            StartCoroutine(ShootAtPredictedPosition(playerRb));
        }

        // (C) Light movement: fast acceleration, low max speed
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
            // Predict player’s future position
            Vector2 predictedPosition = (Vector2)_player.transform.position + playerRb.linearVelocity * predictionTime;

            // Direction towards predicted position
            Vector2 direction = (predictedPosition - (Vector2)transform.position).normalized;

            // Instantiate bullet
            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
                bulletRb.linearVelocity = direction * bulletSpeed;
        }

        yield return new WaitForSeconds(shootingCooldown);
        canShoot = true;
    }
}
