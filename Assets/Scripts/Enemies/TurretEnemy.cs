using UnityEngine;
using System.Collections;

public class TurretEnemy : BaseEnemy
{
    [Header("Vision")]
    public float visionRange = 5f;
    public LayerMask playerMask;

    [Header("Shoot")]
    public Transform FirePoint;
    public GameObject bulletPrefab;
    public float bulletSpeed = 5f;
    public float fireRate = 1f;
    public float keepShootingTime = 2f;

    private Transform player;
    private bool playerDetected = false;
    private bool isShooting = false;
    private float loseSightTimer = 0f;

    protected override void Awake()
    {
        base.Awake();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Update()
    {
        if (player == null) return;

        DetectPlayer();

        if (playerDetected && !isShooting)
            StartCoroutine(Shoot());
    }

    void DetectPlayer()
    {
        Vector2 direction = player.position - transform.position;
        float distance = direction.magnitude;

        if (distance <= visionRange)
        {
            playerDetected = true;
            loseSightTimer = keepShootingTime;
        }
        else
        {
            if (loseSightTimer > 0)
            {
                loseSightTimer -= Time.deltaTime;
                playerDetected = true;
            }
            else
            {
                playerDetected = false;
            }
        }
    }

    IEnumerator Shoot()
    {
        isShooting = true;
        while (playerDetected)
        {
            if (player != null)
            {
                Vector2 dir = (player.position - FirePoint.position).normalized;
                GameObject bullet = Instantiate(bulletPrefab, FirePoint.position, Quaternion.identity);

                Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
                if (bulletRb != null)
                {
                    bulletRb.gravityScale = 0f;
                    bulletRb.linearVelocity = dir * bulletSpeed;
                }

                // Pasa el daño al script Bullet
                Bullet bulletScript = bullet.GetComponent<Bullet>();
                if (bulletScript != null) bulletScript.damage = contactDamage;

                // No rotamos la bala ni el enemigo
                // bullet.transform.rotation = Quaternion.identity;
            }

            yield return new WaitForSeconds(1f / fireRate);
        }
        isShooting = false;
    }
}
