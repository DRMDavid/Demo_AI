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
 * @author Hannin Abarca, David Sánchez, Gael Jiménez
 * Codigo basado en el siguiente tutorial : 
 * https://www.youtube.com/watch?v=SDfEytEjb5o
 * https://www.youtube.com/watch?v=HRX0pUSucW4
 */

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AgentOverride2d))]
public class NavMeshEscapistEnemy : BaseEnemy
{
    /**
     * @enum EnemyState
     * @brief Representa los posibles estados del enemigo.
     */
    public enum EnemyState
    {
        Active, ///< Estado activo: puede huir y disparar
        Tired   ///< Estado cansado: dispara más lento, no huye
    }

    [Header("Estado Actual")]
    [SerializeField] private EnemyState currentState = EnemyState.Active; ///< Estado inicial del enemigo

    // --- COMPONENTES ---
    private NavMeshAgent agent;          ///< Componente NavMeshAgent para moverse por el mapa
    private Transform player;            ///< Transform del jugador a perseguir o evitar
    private Color originalColor;         ///< Color original del sprite para restaurar
    private Coroutine activationCoroutine; ///< Corrutina que maneja la reactivación del enemigo
    private Coroutine tirednessCoroutine;  ///< Corrutina que maneja el tiempo hasta cansarse
    private Coroutine blinkCoroutine;      ///< Corrutina que maneja el parpadeo visual cuando está cansado

    // --- MOVIMIENTO Y HUIDA ---
    [Header("Movimiento y Flee")]
    [SerializeField] private float agentSpeed = 3.2f;            ///< Velocidad máxima del agente
    [SerializeField] private float agentAcceleration = 28f;      ///< Aceleración del agente
    [SerializeField] private float detectionRadius = 9f;         ///< Radio de detección del jugador para huir
    [SerializeField] private float fleeDistance = 7.5f;          ///< Distancia a la que huye al detectar al jugador

    private bool isFleeing = false;       ///< Indica si actualmente está huyendo
    private Vector3 fleeDebugPosition;    ///< Posición objetivo de huida (para depuración)

    // --- TEMPORIZADORES Y TIEMPOS ---
    [Header("Tiempos de Estado")]
    [SerializeField] private float tirednessDuration = 12f;       ///< Tiempo que permanece activo antes de cansarse
    [SerializeField] private float activationDuration = 5f;      ///< Tiempo que permanece cansado antes de volver a activo
    [SerializeField] private float timeWithoutLOSUntilSeek = 2f; ///< Tiempo sin ver al jugador antes de buscarlo

    private float timeSinceLastSawPlayer = 0f; ///< Contador de tiempo sin línea de visión al jugador

    // --- DISPARO ---
    [Header("Disparo")]
    [SerializeField] private GameObject bulletPrefab;           ///< Prefab de la bala a disparar
    [SerializeField] private Transform shootPoint;              ///< Punto desde el cual se disparan las balas
    [SerializeField] private float shootCooldownActive = 1.1f; ///< Tiempo entre disparos en estado activo
    [SerializeField] private float shootCooldownTired = 2.3f;  ///< Tiempo entre disparos en estado cansado
    [SerializeField] private float bulletSpeed = 11.5f;        ///< Velocidad de la bala

    private float nextShootTime = 0f; ///< Temporizador interno para controlar el cooldown de disparo

    // --- VISIÓN / RAYCAST ---
    [Header("Visión y Visuales")]
    [SerializeField] private LayerMask lineOfSightMask;        ///< Máscara de colisión para comprobar línea de visión
    [SerializeField] private Color tiredBlinkColor = Color.blue;///< Color de parpadeo cuando está cansado
    [SerializeField] private float blinkInterval = 0.25f;       ///< Intervalo de parpadeo del sprite

    private const float FIXED_Z_POSITION = 0f; ///< Posición Z constante para mantener el enemigo en 2D

    // ===============================
    //          INICIALIZACIÓN
    // ===============================
    /**
     * @brief Inicializa variables, componentes y estados del enemigo.
     */
    protected override void Start()
    {
        base.Start();

        if (_senses != null) _senses.enabled = false;           ///< Desactiva sensores si existen
        if (_steeringBehaviors != null) _steeringBehaviors.enabled = false; ///< Desactiva comportamientos de steering

        agent = GetComponent<NavMeshAgent>();                   ///< Obtiene NavMeshAgent
        player = GameObject.FindGameObjectWithTag("Player")?.transform; ///< Obtiene jugador
        if (_spriteRenderer != null) originalColor = _spriteRenderer.color; ///< Guarda color original

        agent.updateRotation = false; ///< Evita rotación automática del agente
        agent.updateUpAxis = false;   ///< Evita ajuste del eje Y
        agent.speed = agentSpeed;
        agent.acceleration = agentAcceleration;

        currentHP = 15; ///< Vida inicial

        // Transición al estado inicial configurado
        if (currentState == EnemyState.Active)
            TransitionToActive();
        else
            TransitionToTired();
    }

    // ===============================
    //            UPDATE
    // ===============================
    /**
     * @brief Actualiza comportamiento cada frame según estado.
     */
    private void Update()
    {
        if (player == null || currentHP <= 0)
        {
            StopAgent(); ///< Detiene movimiento si no hay jugador o está muerto
            return;
        }

        // Mantiene posición Z fija
        if (transform.position.z != FIXED_Z_POSITION)
            transform.position = new Vector3(transform.position.x, transform.position.y, FIXED_Z_POSITION);

        // Reduce temporizador de disparo
        if (nextShootTime > 0)
            nextShootTime -= Time.deltaTime;

        // Ejecuta lógica según estado
        switch (currentState)
        {
            case EnemyState.Active:
                HandleActiveState(); ///< Huida y disparo rápido
                break;
            case EnemyState.Tired:
                HandleTiredState();  ///< Solo disparo lento
                break;
        }
    }

    // ===============================
    //        ESTADO ACTIVO
    // ===============================
    /**
     * @brief Lógica del enemigo cuando está activo.
     *
     * Incluye huida si el jugador está cerca y disparo.
     */
    private void HandleActiveState()
    {
        float distance = Vector3.Distance(transform.position, player.position); ///< Distancia al jugador
        bool hasLOS = CheckLOS(); ///< Comprueba línea de visión

        // Si jugador está dentro del radio de detección → huir
        if (distance <= detectionRadius && !isFleeing)
        {
            if (TryFlee())
            {
                isFleeing = true;
                if (tirednessCoroutine != null) StopCoroutine(tirednessCoroutine);
                tirednessCoroutine = StartCoroutine(TirednessTimer()); ///< Empieza cronómetro de cansancio
            }
        }

        // Control de llegada a destino de huida
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

        TryShoot(player.position); ///< Disparo hacia el jugador
    }

    // ===============================
    //        ESTADO CANSADO
    // ===============================
    /**
     * @brief Lógica del enemigo cuando está cansado.
     *
     * Solo dispara con cooldown más largo, no huye.
     */
    private void HandleTiredState()
    {
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
    /**
     * @brief Cronómetro que determina cuando pasar a estado cansado.
     */
    private IEnumerator TirednessTimer()
    {
        yield return new WaitForSeconds(tirednessDuration);
        TransitionToTired();
    }

    /**
     * @brief Cronómetro que determina cuando pasar a estado activo.
     */
    private IEnumerator ActivationTimer()
    {
        yield return new WaitForSeconds(activationDuration);
        TransitionToActive();
    }

    /**
     * @brief Corrutina que hace parpadear al enemigo cuando está cansado.
     */
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
     * @brief Calcula dirección y destino de huida del enemigo.
     * @return true si pudo establecer un destino de huida válido.
     */
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
    /**
     * @brief Maneja el disparo hacia un objetivo.
     * @param targetPos Posición del objetivo
     */
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
     * @brief Verifica si hay línea de visión hacia el jugador.
     * @return true si no hay obstáculos
     */
    private bool CheckLOS()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        float dist = Vector3.Distance(transform.position, player.position);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, dist, lineOfSightMask);

        Debug.DrawRay(transform.position, dir * dist, hit.collider == null ? Color.green : Color.red);
        return hit.collider == null;
    }

    /**
     * @brief Instancia una bala y le aplica velocidad.
     */
    protected void Shoot(Quaternion rotation, Vector3 dir)
    {
        if (!bulletPrefab || !shootPoint) return;

        GameObject bullet = Instantiate(bulletPrefab, shootPoint.position, rotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb)
            rb.linearVelocity = dir * bulletSpeed;
    }

    /**
     * @brief Detiene el NavMeshAgent.
     */
    private void StopAgent()
    {
        if (agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    private void OnCollisionStay2D(Collision2D collision) { }

    /**
     * @brief Dibuja gizmos para debugging en el editor.
     */
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
