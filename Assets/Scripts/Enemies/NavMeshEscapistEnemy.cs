using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using NavMeshPlus.Extensions;

/**
 * @file NavMeshEscapistEnemy.cs
 * @brief Script para un enemigo escapista que usa NavMesh en Unity 2D.
 *
 * Este enemigo puede huir del jugador, disparar proyectiles,
 * y cambiar entre estados "Activo" y "Cansado" con parpadeo visual.
 *
 * @author Hannin Abarca
 * @coauthor David Sánchez
 * @coauthor Gael Jiménez
 * Código basado en:
 * https://www.youtube.com/watch?v=SDfEytEjb5o
 * https://www.youtube.com/watch?v=HRX0pUSucW4
 */

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AgentOverride2d))]
public class NavMeshEscapistEnemy : BaseEnemy
{
    /**
     * @enum EnemyState
     * @brief Estados posibles del enemigo.
     */
    public enum EnemyState { Active, Tired }

    [Header("Estado Actual")]
    [SerializeField] private EnemyState currentState = EnemyState.Active; ///< Estado inicial del enemigo

    // --- COMPONENTES ---
    private NavMeshAgent agent;          ///< Componente NavMeshAgent para moverse
    private Transform player;            ///< Transform del jugador
    private Color originalColor;         ///< Color original del sprite
    private Coroutine activationCoroutine; ///< Corrutina para transición a activo
    private Coroutine tirednessCoroutine;  ///< Corrutina para transición a cansado
    private Coroutine blinkCoroutine;      ///< Corrutina de parpadeo visual

    // --- MOVIMIENTO Y HUIDA ---
    [Header("Movimiento y Flee")]
    [SerializeField] private float agentSpeed = 3.2f;            ///< Velocidad máxima del agente
    [SerializeField] private float agentAcceleration = 28f;      ///< Aceleración del agente
    [SerializeField] private float detectionRadius = 9f;         ///< Radio de detección para huir
    [SerializeField] private float fleeDistance = 7.5f;          ///< Distancia de huida
    [SerializeField] private float stoppingDistance = 0.2f;      ///< Distancia mínima para considerar llegada (editable)

    private bool isFleeing = false;       ///< Indica si actualmente está huyendo
    private Vector3 fleeDebugPosition;    ///< Posición objetivo de huida para depuración

    // --- TEMPORIZADORES ---
    [Header("Tiempos de Estado")]
    [SerializeField] private float tirednessDuration = 12f;       ///< Tiempo activo antes de cansarse
    [SerializeField] private float activationDuration = 5f;       ///< Tiempo cansado antes de volver a activo
    [SerializeField] private float timeWithoutLOSUntilSeek = 2f; ///< Tiempo sin visión antes de perseguir

    private float timeSinceLastSawPlayer = 0f; ///< Contador desde que perdió visión

    // --- DISPARO ---
    [Header("Disparo")]
    [SerializeField] private GameObject bulletPrefab;           ///< Prefab de la bala
    [SerializeField] private Transform shootPoint;              ///< Punto desde el cual dispara
    [SerializeField] private float shootCooldownActive = 1.1f; ///< Tiempo entre disparos activo
    [SerializeField] private float shootCooldownTired = 2.3f;  ///< Tiempo entre disparos cansado
    [SerializeField] private float bulletSpeed = 11.5f;        ///< Velocidad de la bala

    private float nextShootTime = 0f; ///< Temporizador interno para controlar cooldown

    // --- VISIÓN / RAYCAST ---
    [Header("Visión y Visuales")]
    [SerializeField] private LayerMask lineOfSightMask;        ///< Máscara para comprobar línea de visión
    [SerializeField] private Color tiredBlinkColor = Color.blue;///< Color al parpadear cuando está cansado
    [SerializeField] private float blinkInterval = 0.25f;       ///< Intervalo de parpadeo

    private const float FIXED_Z_POSITION = 0f; ///< Posición Z fija para mantener 2D

    // ===============================
    //          INICIALIZACIÓN
    // ===============================
    /**
     * @brief Inicializa componentes, variables y estado inicial del enemigo.
     */
    protected override void Start()
    {
        base.Start();

        if (_senses != null) _senses.enabled = false;           ///< Desactiva sensores si existen
        if (_steeringBehaviors != null) _steeringBehaviors.enabled = false; ///< Desactiva steering

        agent = GetComponent<NavMeshAgent>();                   ///< Obtiene NavMeshAgent
        player = GameObject.FindGameObjectWithTag("Player")?.transform; ///< Obtiene jugador
        if (_spriteRenderer != null) originalColor = _spriteRenderer.color; ///< Guarda color original

        agent.updateRotation = false; ///< Evita rotación automática
        agent.updateUpAxis = false;   ///< Evita ajuste eje Y
        agent.speed = agentSpeed;
        agent.acceleration = agentAcceleration;

        currentHP = 15; ///< Vida inicial

        // Transición al estado inicial
        if (currentState == EnemyState.Active) TransitionToActive();
        else TransitionToTired();
    }

    // ===============================
    //            UPDATE
    // ===============================
    /**
     * @brief Lógica por frame según estado del enemigo.
     */
    private void Update()
    {
        if (player == null || currentHP <= 0)
        {
            StopAgent(); ///< Detiene movimiento si no hay jugador o está muerto
            return;
        }

        // Mantener Z fijo
        if (transform.position.z != FIXED_Z_POSITION)
            transform.position = new Vector3(transform.position.x, transform.position.y, FIXED_Z_POSITION);

        // Reduce temporizador de disparo
        if (nextShootTime > 0)
            nextShootTime -= Time.deltaTime;

        // Lógica según estado
        switch (currentState)
        {
            case EnemyState.Active:
                HandleActiveState(); ///< Huida y disparo rápido
                break;
            case EnemyState.Tired:
                HandleTiredState();  ///< Disparo lento solo
                break;
        }
    }

    // ===============================
    //        ESTADO ACTIVO
    // ===============================
    /**
     * @brief Lógica del enemigo cuando está activo.
     * Incluye huida, detección y disparo.
     */
    private void HandleActiveState()
    {
        float distance = Vector3.Distance(transform.position, player.position); ///< Distancia al jugador
        bool hasLOS = CheckLOS(); ///< Comprueba línea de visión

        if (distance <= detectionRadius && !isFleeing)
        {
            if (TryFlee())
            {
                isFleeing = true;
                if (tirednessCoroutine != null) StopCoroutine(tirednessCoroutine);
                tirednessCoroutine = StartCoroutine(TirednessTimer()); ///< Comienza cronómetro de cansancio
            }
        }

        // Control llegada a destino de huida
        if (isFleeing)
        {
            if (!agent.pathPending && agent.remainingDistance < stoppingDistance)
            {
                isFleeing = false;
                agent.ResetPath();
            }
        }
        else if (hasLOS)
        {
            StopAgent(); ///< Detiene si ve al jugador
            timeSinceLastSawPlayer = 0f;
        }
        else
        {
            // Persigue si perdió visión
            timeSinceLastSawPlayer += Time.deltaTime;
            if (timeSinceLastSawPlayer >= timeWithoutLOSUntilSeek)
            {
                if (agent.isOnNavMesh)
                    agent.SetDestination(player.position);
                timeSinceLastSawPlayer = 0f;
            }
        }

        // Disparo solo si hay línea de visión
        if (hasLOS)
            TryShoot(player.position);
    }

    // ===============================
    //        ESTADO CANSADO
    // ===============================
    /**
     * @brief Lógica del enemigo cuando está cansado.
     * Solo dispara si ve al jugador, no huye.
     */
    private void HandleTiredState()
    {
        if (CheckLOS())
            TryShoot(player.position);
    }

    // ===============================
    //         TRANSICIONES
    // ===============================
    /**
     * @brief Cambia enemigo a estado activo.
     */
    private void TransitionToActive()
    {
        currentState = EnemyState.Active;
        isFleeing = false;

        if (agent.isOnNavMesh && player != null)
            agent.SetDestination(player.position);

        if (_spriteRenderer != null) _spriteRenderer.color = originalColor;
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
    }

    /**
     * @brief Cambia enemigo a estado cansado.
     */
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
    /**
     * @brief Calcula dirección y destino de huida.
     * @return true si pudo establecer un destino válido.
     */
    private bool TryFlee()
    {
        Vector3 fleeDir = (transform.position - player.position).normalized;
        Vector3 targetPos = transform.position + (fleeDir * fleeDistance);
        fleeDebugPosition = targetPos;

        NavMeshHit hit;
        int walkableMask = 1 << NavMesh.GetAreaFromName("Walkable"); ///< Solo zonas caminables

        if (NavMesh.SamplePosition(targetPos, out hit, fleeDistance * 0.5f, walkableMask))
        {
            fleeDebugPosition = hit.position;
            agent.SetDestination(fleeDebugPosition);
            return true;
        }

        targetPos = transform.position - (fleeDir * fleeDistance);
        if (NavMesh.SamplePosition(targetPos, out hit, fleeDistance * 0.5f, walkableMask))
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

    /**
     * @brief Comprueba si hay línea de visión hacia el jugador.
     * @return true si no hay obstáculos entre enemigo y jugador
     */
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

    // ===============================
    //        CONTROL DE AGENTE
    // ===============================
    private void StopAgent()
    {
        if (agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    // ===============================
    //           GIZMOS
    // ===============================
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
