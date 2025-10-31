using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshEscapistEnemy : BaseEnemy
{
    public enum EnemyState { Active, Tired }

    [Header("NavMesh Settings")]
    public float acceleration = 10f;
    public float maxSpeed = 3f;

    [Header("Escapist Settings")]
    public float tiredDuration = 3f;       // Tiempo que permanece cansado
    public float fleeDistance = 5f;        // Distancia a la que intenta huir
    public float visionRange = 10f;        // Radio de detección
    public LayerMask obstacleMask;

    [Header("Shooting Settings")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 6f;
    public float shootingCooldown = 1.5f;

    private EnemyState currentState = EnemyState.Active;
    private NavMeshAgent agent;
    private GameObject player;
    private bool canShoot = true;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    protected override void Start()
    {
        base.Start();

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // Configuración inicial del agente
        if (agent != null)
        {
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            agent.speed = maxSpeed;
            agent.acceleration = acceleration;

            // Esperar hasta el siguiente frame para asegurarnos de que esté sobre el NavMesh
            StartCoroutine(InitializeAgent());
        }
    }

    private IEnumerator InitializeAgent()
    {
        yield return null; // espera 1 frame
        if (agent != null && agent.isOnNavMesh)
            agent.isStopped = false;
    }

    private void Update()
    {
        // Buscar jugador si no se ha asignado
        if (player == null)
        {
            var players = GameObject.FindGameObjectsWithTag("Player");
            if (players.Length > 0)
                player = players[0];
        }

        if (player == null || agent == null || !agent.isOnNavMesh)
            return;

        // Ejecutar comportamiento según estado
        switch (currentState)
        {
            case EnemyState.Active:
                ActiveBehavior();
                break;
            case EnemyState.Tired:
                TiredBehavior();
                break;
        }

        // Disparo hacia el jugador
        if (canShoot && player != null)
        {
            StartCoroutine(ShootAtPlayer());
        }
    }

    private void ActiveBehavior()
    {
        Vector3 directionToPlayer = player.transform.position - transform.position;

        // Raycast para línea de visión
        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, visionRange, obstacleMask);
        bool hasLOS = hit.collider == null || hit.collider.gameObject == player;

        if (hasLOS)
        {
            agent.isStopped = true; // Detenerse si ve al jugador
        }
        else
        {
            // Punto de flee opuesto al jugador
            Vector3 fleeDir = (transform.position - player.transform.position).normalized;
            Vector3 fleePoint = transform.position + fleeDir * fleeDistance;

            // Verificar validez del punto en NavMesh
            NavMeshHit navHit;
            if (NavMesh.SamplePosition(fleePoint, out navHit, 1.0f, NavMesh.AllAreas))
            {
                agent.SetDestination(navHit.position);
            }
            else
            {
                // Si no es válido, moverse hacia el jugador
                agent.SetDestination(player.transform.position);
            }

            agent.isStopped = false;
        }
    }

    private void TiredBehavior()
    {
        if (agent.isOnNavMesh)
            agent.isStopped = true;

        if (spriteRenderer != null)
            spriteRenderer.color = Color.blue; // Visualizar cansancio
    }

    private IEnumerator ShootAtPlayer()
    {
        canShoot = false;
        if (bulletPrefab != null && player != null)
        {
            Vector3 dir = (player.transform.position - transform.position).normalized;
            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.linearVelocity = dir * bulletSpeed;
        }
        yield return new WaitForSeconds(shootingCooldown);
        canShoot = true;
    }

    public void EnterTiredState()
    {
        if (currentState != EnemyState.Tired)
            StartCoroutine(TiredTimer());
    }

    private IEnumerator TiredTimer()
    {
        currentState = EnemyState.Tired;
        if (spriteRenderer != null) spriteRenderer.color = Color.blue;

        if (agent != null && agent.isOnNavMesh)
            agent.isStopped = true;

        yield return new WaitForSeconds(tiredDuration);

        currentState = EnemyState.Active;
        if (spriteRenderer != null) spriteRenderer.color = Color.white;

        if (agent != null && agent.isOnNavMesh)
            agent.isStopped = false;
    }

    private void OnDrawGizmos()
    {
        if (player != null)
        {
            // Raycast hacia el jugador
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, player.transform.position);

            // Punto de flee
            Vector3 fleeDir = (transform.position - player.transform.position).normalized;
            Vector3 fleePoint = transform.position + fleeDir * fleeDistance;
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(fleePoint, 0.2f);
        }
    }
}
