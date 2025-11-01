using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using NavMeshPlus.Extensions;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AgentOverride2d))]
public class NavMeshEscapistEnemy : BaseEnemy
{
    public enum EnemyState
    {
        Active,
        Tired
    }

    [Header("Estado Actual")]
    [SerializeField] private EnemyState currentState = EnemyState.Active;

    // --- COMPONENTES ---
    private NavMeshAgent agent;
    private Transform player;
    private Color originalColor;
    private Coroutine activationCoroutine;
    private Coroutine tirednessCoroutine;
    private Coroutine blinkCoroutine;

    // --- MOVIMIENTO Y FLEE ---
    [Header("Movimiento y Flee")]
    [SerializeField] private float agentSpeed = 3.2f;            // Acelera rápido pero no tanta velocidad máxima
    [SerializeField] private float agentAcceleration = 28f;      // Ligero, sensible
    [SerializeField] private float detectionRadius = 9f;         // Radio de detección para huida
    [SerializeField] private float fleeDistance = 7.5f;          // Qué tanto se aleja al huir

    private bool isFleeing = false;
    private Vector3 fleeDebugPosition;

    // --- TEMPORIZADORES Y TIEMPOS ---
    [Header("Tiempos de Estado")]
    [SerializeField] private float tirednessDuration = 12f;       // Activo antes de cansarse
    [SerializeField] private float activationDuration = 5f;      // Cansado antes de volver a estar activo
    [SerializeField] private float timeWithoutLOSUntilSeek = 2f; // Tiempo sin visión antes de moverse otra vez

    private float timeSinceLastSawPlayer = 0f;

    // --- DISPARO ---
    [Header("Disparo")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float shootCooldownActive = 1.1f;
    [SerializeField] private float shootCooldownTired = 2.3f;
    [SerializeField] private float bulletSpeed = 11.5f;

    private float nextShootTime = 0f;

    // --- RAYCAST / VISUALES ---
    [Header("Visión y Visuales")]
    [SerializeField] private LayerMask lineOfSightMask;
    [SerializeField] private Color tiredBlinkColor = Color.blue;
    [SerializeField] private float blinkInterval = 0.25f;

    private const float FIXED_Z_POSITION = 0f;

    // ===============================
    //          INICIALIZACIÓN
    // ===============================
    protected override void Start()
    {
        base.Start();

        if (_senses != null) _senses.enabled = false;
        if (_steeringBehaviors != null) _steeringBehaviors.enabled = false;

        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (_spriteRenderer != null) originalColor = _spriteRenderer.color;

        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = agentSpeed;
        agent.acceleration = agentAcceleration;

        // 💚 Vida base del escapista
        currentHP = 15;

        if (currentState == EnemyState.Active)
            TransitionToActive();
        else
            TransitionToTired();
    }

    // ===============================
    //            UPDATE
    // ===============================
    private void Update()
    {
        if (player == null || currentHP <= 0)
        {
            StopAgent();
            return;
        }

        // Mantiene Z fija
        if (transform.position.z != FIXED_Z_POSITION)
            transform.position = new Vector3(transform.position.x, transform.position.y, FIXED_Z_POSITION);

        if (nextShootTime > 0)
            nextShootTime -= Time.deltaTime;

        switch (currentState)
        {
            case EnemyState.Active:
                HandleActiveState();
                break;
            case EnemyState.Tired:
                HandleTiredState();
                break;
        }
    }

    // ===============================
    //        ESTADO ACTIVO
    // ===============================
    private void HandleActiveState()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        bool hasLOS = CheckLOS();

        // Si el jugador está cerca → huir
        if (distance <= detectionRadius && !isFleeing)
        {
            if (TryFlee())
            {
                isFleeing = true;
                if (tirednessCoroutine != null) StopCoroutine(tirednessCoroutine);
                tirednessCoroutine = StartCoroutine(TirednessTimer());
            }
        }

        if (isFleeing)
        {
            if (!agent.pathPending && agent.remainingDistance < 0.2f)
            {
                isFleeing = false;
                agent.ResetPath();
            }
        }
        else if (hasLOS)
        {
            StopAgent();
            timeSinceLastSawPlayer = 0f;
        }
        else
        {
            timeSinceLastSawPlayer += Time.deltaTime;
            if (timeSinceLastSawPlayer >= timeWithoutLOSUntilSeek)
            {
                if (agent.isOnNavMesh)
                    agent.SetDestination(player.position);
                timeSinceLastSawPlayer = 0f;
            }
        }

        TryShoot(player.position);
    }

    // ===============================
    //        ESTADO CANSADO
    // ===============================
    private void HandleTiredState()
    {
        TryShoot(player.position);
    }

    // ===============================
    //         TRANSICIONES
    // ===============================
    private void TransitionToActive()
    {
        currentState = EnemyState.Active;
        isFleeing = false;

        if (agent.isOnNavMesh && player != null)
            agent.SetDestination(player.position);

        if (_spriteRenderer != null) _spriteRenderer.color = originalColor;
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
    }

    private void TransitionToTired()
    {
        currentState = EnemyState.Tired;

        StopAgent();

        if (activationCoroutine != null) StopCoroutine(activationCoroutine);
        activationCoroutine = StartCoroutine(ActivationTimer());

        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
        blinkCoroutine = StartCoroutine(TirednessBlink());
    }

    // ===============================
    //        COROUTINES DE TIEMPO
    // ===============================
    private IEnumerator TirednessTimer()
    {
        yield return new WaitForSeconds(tirednessDuration);
        TransitionToTired();
    }

    private IEnumerator ActivationTimer()
    {
        yield return new WaitForSeconds(activationDuration);
        TransitionToActive();
    }

    private IEnumerator TirednessBlink()
    {
        if (_spriteRenderer == null) yield break;

        while (currentState == EnemyState.Tired)
        {
            _spriteRenderer.color = tiredBlinkColor;
            yield return new WaitForSeconds(blinkInterval);
            _spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(blinkInterval);
        }
    }

    // ===============================
    //        LÓGICA DE HUIDA
    // ===============================
    private bool TryFlee()
    {
        Vector3 fleeDir = (transform.position - player.position).normalized;
        Vector3 targetPos = transform.position + (fleeDir * fleeDistance);

        fleeDebugPosition = targetPos;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPos, out hit, fleeDistance * 0.5f, NavMesh.AllAreas))
        {
            fleeDebugPosition = hit.position;
            agent.SetDestination(fleeDebugPosition);
            return true;
        }

        targetPos = transform.position - (fleeDir * fleeDistance);
        if (NavMesh.SamplePosition(targetPos, out hit, fleeDistance * 0.5f, NavMesh.AllAreas))
        {
            fleeDebugPosition = hit.position;
            agent.SetDestination(fleeDebugPosition);
            return true;
        }

        return false;
    }

    // ===============================
    //       DISPARO Y VISIÓN
    // ===============================
    private void TryShoot(Vector3 targetPos)
    {
        Vector3 dir = (targetPos - transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        float cooldown = (currentState == EnemyState.Active) ? shootCooldownActive : shootCooldownTired;

        if (nextShootTime <= 0)
        {
            nextShootTime = cooldown;
            Shoot(transform.rotation, dir);
        }
    }

    private bool CheckLOS()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        float dist = Vector3.Distance(transform.position, player.position);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, dist, lineOfSightMask);

        Debug.DrawRay(transform.position, dir * dist, hit.collider == null ? Color.green : Color.red);
        return hit.collider == null;
    }

    protected void Shoot(Quaternion rotation, Vector3 dir)
    {
        if (!bulletPrefab || !shootPoint) return;

        GameObject bullet = Instantiate(bulletPrefab, shootPoint.position, rotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb)
            rb.linearVelocity = dir * bulletSpeed;
    }

    private void StopAgent()
    {
        if (agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    private void OnCollisionStay2D(Collision2D collision) { }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        if (Application.isPlaying && isFleeing)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(fleeDebugPosition, 0.5f);
            Gizmos.DrawLine(transform.position, fleeDebugPosition);
        }
    }
}
