using UnityEngine;
using System.Collections;

public class TurretEnemy : BaseEnemy
{
    [Header("Cono de visión")]
    public Transform visionCone;           
    public float visionAngle = 45f;
    public float visionDistance = 5f;
    public float rotationStep = 45f;
    public float rotationInterval = 2f;

    [Header("Disparo")]
    public GameObject bulletPrefab;
    public Transform firePoint;            
    public float bulletSpeed = 10f;
    public float fireRate = 1f;

    [Header("Detección")]
    public float timeToLoseTarget = 2f;

    private bool playerDetected = false;
    private bool rotating = true;
    private Transform player;
    private Coroutine rotationCoroutine;
    private Coroutine fireCoroutine;
    private float lastDetectionTime;

    void OnValidate()
    {
        if (visionCone == null)
        {
            Transform t = transform.Find("VisionCone");
            if (t != null) visionCone = t;
        }
        if (firePoint == null)
        {
            Transform f = transform.Find("VisionCone/FirePoint") ?? transform.Find("FirePoint");
            if (f != null) firePoint = f;
        }
    }

    protected override void Start()
    {
        base.Start();

        if (visionCone == null) visionCone = transform;
        if (bulletPrefab == null) Debug.LogWarning("TurretEnemy: bulletPrefab no asignado en " + name);
        if (firePoint == null) Debug.LogWarning("TurretEnemy: firePoint no asignado en " + name);

        rotationCoroutine = StartCoroutine(RotateVisionCone());
    }

    void Update()
    {
        DetectPlayer();
    }

    void DetectPlayer()
    {
        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
            if (foundPlayer != null) player = foundPlayer.transform;
        }

        if (player == null) return;

        // validación extra para evitar errores si el player fue destruido
        if (player == null) return;

        Vector2 dirToPlayer = (player.position - transform.position).normalized;
        float angle = Vector2.Angle(visionCone.right, dirToPlayer);
        float distance = Vector2.Distance(transform.position, player.position);

        if (angle < visionAngle * 0.5f && distance <= visionDistance)
        {
            if (!playerDetected)
            {
                playerDetected = true;
                rotating = false;
                if (rotationCoroutine != null) StopCoroutine(rotationCoroutine);
                fireCoroutine = StartCoroutine(FireAtPlayer());
            }
            lastDetectionTime = Time.time;
        }
        else
        {
            if (playerDetected && Time.time > lastDetectionTime + timeToLoseTarget)
            {
                playerDetected = false;
                if (fireCoroutine != null) StopCoroutine(fireCoroutine);
                rotating = true;
                rotationCoroutine = StartCoroutine(RotateVisionCone());
            }
        }
    }

    IEnumerator RotateVisionCone()
    {
        while (rotating)
        {
            visionCone.Rotate(Vector3.forward, rotationStep);
            yield return new WaitForSeconds(rotationInterval);
        }
    }

    IEnumerator FireAtPlayer()
    {
        while (playerDetected)
        {
            // 🛑 chequeo para evitar MissingReference
            if (player == null) yield break;

            if (bulletPrefab != null && firePoint != null)
            {
                Vector2 dir = (player.position - firePoint.position).normalized;
                GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

                var bulletScript = bullet.GetComponent<Bullet>(); // ahora usa Proyectil
                if (bulletScript != null)
                {
                    bulletScript.damage = damageToPlayer;
                    bulletScript.Init(dir, bulletSpeed);
                }
                else
                {
                    Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                    if (rb != null) rb.linearVelocity = dir * bulletSpeed;
                    else Debug.LogWarning("Bullet prefab no tiene Rigidbody2D ni Proyectil.cs!");
                }
            }

            yield return new WaitForSeconds(1f / fireRate);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (visionCone == null) visionCone = transform;
        Gizmos.color = Color.yellow;
        Vector3 rightDir = Quaternion.Euler(0, 0, visionAngle * 0.5f) * visionCone.right;
        Vector3 leftDir = Quaternion.Euler(0, 0, -visionAngle * 0.5f) * visionCone.right;
        Gizmos.DrawRay(transform.position, rightDir * visionDistance);
        Gizmos.DrawRay(transform.position, leftDir * visionDistance);
    }
}
