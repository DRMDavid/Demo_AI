// ================================================================
// Archivo: BossEnemy.cs
// Descripción general:
// Script que controla el comportamiento completo del jefe (Boss) en un juego 2D
// con vista aérea utilizando NavMeshPlus para movimiento y una máquina de estados finita (FSM).
//
// Este jefe combina IA de persecución, ataques cuerpo a cuerpo (Melee),
// ataques a distancia (Ranged) y una habilidad definitiva (Ultimate),
// cambiando entre ellos dinámicamente según la distancia al jugador
// y su porcentaje de vida restante.
//
//
// Integrantes del equipo:
// - Hannin Abarca
// - David Sánchez
// - Gael Jiménez
// ================================================================

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using NavMeshPlus.Extensions;

// Requiere componentes esenciales para movimiento con NavMesh 2D
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AgentOverride2d))]
public class BossEnemy : BaseEnemy
{
    // ================================================================
    // CONFIGURACIÓN DE MOVIMIENTO Y NAVMESH
    // ================================================================
    [Header("CONFIGURACIÓN DE NAVMESH")]
    private NavMeshAgent _navAgent;

    [Tooltip("Velocidad de movimiento del NavMeshAgent.")]
    public float agentSpeed = 3.5f;

    [Tooltip("Distancia a la que el agente se detiene para ataques cuerpo a cuerpo.")]
    public float agentStoppingDistance = 1.0f;


    // ================================================================
    // CONFIGURACIÓN DE LA FSM (Finite State Machine)
    // ================================================================
    [Header("RANGOS DE FSM (Ajustables en el Inspector)")]
    [Tooltip("Distancia máxima para entrar en el estado de ataque Melee.")]
    public float meleeRange = 3.0f;

    [Tooltip("Distancia máxima para el ataque a distancia.")]
    public float rangedRange = 8.0f;

    [Tooltip("Porcentaje de vida (0 a 1) para activar el modo Ultimate.")]
    [Range(0f, 1f)]
    public float ultimateHPThreshold = 0.3f;


    // ================================================================
    // CONFIGURACIÓN DE ATAQUES
    // ================================================================
    [Header("CONFIGURACIÓN DE ATAQUE (Ajustables)")]
    [Tooltip("Tiempo entre ataques consecutivos.")]
    public float attackCooldown = 1.5f;

    [Tooltip("Fuerza aplicada durante el ataque DASH (melee especial 2).")]
    public float dashForce = 15f;


    // ================================================================
    // CONFIGURACIÓN DE PROYECTILES Y EFECTOS
    // ================================================================
    [Header("PROYECTILES Y DAÑO (Ajustables)")]
    [Tooltip("Prefab de la bala utilizada en ataques a distancia.")]
    public GameObject bulletPrefab;

    [Tooltip("Transform desde donde se disparan los proyectiles.")]
    public Transform firePoint;

    [Tooltip("Velocidad de los proyectiles lanzados.")]
    public float bulletSpeed = 10f;

    [Tooltip("Cantidad de balas disparadas en una ráfaga circular.")]
    public int rangedBurstCount = 5;

    [Tooltip("Radio del daño de área (Melee Special 1).")]
    public float aoeRadius = 4.0f;

    [Tooltip("Multiplicador de daño del ataque de área.")]
    public int aoeDamageMultiplier = 4;


    // ================================================================
    // VARIABLES INTERNAS DE ESTADO
    // ================================================================
    private EBossState _currentBossState = EBossState.IdleMove;   // Estado actual del jefe
    private EBossAttackType _nextAttackIndex = EBossAttackType.BasicAttack; // Próximo ataque
    private Transform _playerTarget;                              // Referencia al jugador
    private float _lastAttackTime = 0f;                           // Control de cooldown
    private Rigidbody2D _rb;                                      // Control físico del jefe
    private int _playerLayerMask;                                 // Capa del jugador para colisiones

    private const float FIXED_Z_POSITION = 0f; // Evita que el boss cambie su posición Z


    // ================================================================
    // MÉTODOS UNITY
    // ================================================================
    protected override void Start()
    {
        // Inicializa BaseEnemy (vida, sentidos, etc.)
        base.Start();

        // Desactiva comportamientos heredados no usados (como Steering)
        if (_senses != null) _senses.enabled = false;
        if (_steeringBehaviors != null) _steeringBehaviors.enabled = false;

        _rb = GetComponent<Rigidbody2D>();
        _navAgent = GetComponent<NavMeshAgent>();

        // Validaciones de componentes
        if (_navAgent == null)
        {
            Debug.LogError("NavMeshAgent no encontrado. Agrega el componente NavMeshAgent.");
            return;
        }

        // Configuración esencial del agente NavMesh 2D
        _navAgent.updateRotation = false;
        _navAgent.updateUpAxis = false;
        _navAgent.speed = agentSpeed;
        _navAgent.stoppingDistance = agentStoppingDistance;

        // Encuentra al jugador mediante su Tag
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
            _playerTarget = playerObject.transform;
        else
            Debug.LogError("Jugador no encontrado. Asegúrate de que tenga la Tag 'Player'.");

        _playerLayerMask = 1 << LayerMask.NameToLayer("Player");

        // Inicia el ciclo principal de IA
        StartCoroutine(BossAILoop());
    }


    void Update()
    {
        // Fija el valor Z en 0 para mantener al Boss en el plano 2D
        if (transform.position.z != FIXED_Z_POSITION)
            transform.position = new Vector3(transform.position.x, transform.position.y, FIXED_Z_POSITION);

        // Movimiento del boss cuando está en IdleMove
        if (_navAgent != null && _playerTarget != null && _currentBossState == EBossState.IdleMove)
        {
            _navAgent.isStopped = false;
            _navAgent.speed = agentSpeed;
            _navAgent.SetDestination(_playerTarget.position);
        }
    }


    // ================================================================
    // MÁQUINA DE ESTADOS PRINCIPAL (FSM LOOP)
    // ================================================================
    IEnumerator BossAILoop()
    {
        if (_playerTarget == null) yield break;
        EBossState previousState = EBossState.IdleMove;

        while (currentHP > 0)
        {
            EBossState newState = DetermineNextMainState();

            // Cambio de estado con log visual
            if (newState != _currentBossState)
            {
                Debug.Log($"FSM CHANGE: {previousState} -> {newState}");
                previousState = newState;
                _navAgent.isStopped = true;
                _currentBossState = newState;
            }

            // Ejecución de estados
            switch (_currentBossState)
            {
                case EBossState.Melee:
                    yield return StartCoroutine(HandleMeleeState());
                    break;

                case EBossState.Ranged:
                    yield return StartCoroutine(HandleRangedState());
                    break;

                case EBossState.Ultimate:
                    yield return StartCoroutine(ExecuteUltimateAttack());
                    _currentBossState = EBossState.IdleMove;
                    break;

                case EBossState.IdleMove:
                    yield return null;
                    break;
            }

            yield return null;
        }

        Destroy(gameObject, 0.2f);
        yield break;
    }


    // ================================================================
    // LÓGICA DE ESTADOS
    // ================================================================
    EBossState DetermineNextMainState()
    {
        if (_playerTarget == null) return EBossState.IdleMove;
        float distance = Vector3.Distance(transform.position, _playerTarget.position);

        if (currentHP <= maxHP * ultimateHPThreshold)
            return EBossState.Ultimate;

        if (distance <= meleeRange)
            return EBossState.Melee;

        if (distance <= rangedRange)
            return EBossState.Ranged;

        return EBossState.IdleMove;
    }


    IEnumerator HandleMeleeState()
    {
        _navAgent.isStopped = true;

        if (Time.time > _lastAttackTime + attackCooldown)
        {
            _lastAttackTime = Time.time;
            yield return StartCoroutine(ExecuteAttack(_currentBossState));
        }
        yield break;
    }


    IEnumerator HandleRangedState()
    {
        _navAgent.isStopped = true;

        if (Time.time > _lastAttackTime + attackCooldown)
        {
            _lastAttackTime = Time.time;
            yield return StartCoroutine(ExecuteAttack(_currentBossState));
        }
        yield break;
    }


    // ================================================================
    // ATAQUES PRINCIPALES
    // ================================================================
    IEnumerator ExecuteAttack(EBossState attackSource)
    {
        EBossAttackType attackType = _nextAttackIndex;
        float attackDuration;

        if (attackSource == EBossState.Melee)
        {
            attackDuration = GetMeleeAttackDuration(attackType);
            StartCoroutine(PerformMeleeAttackSequence(attackType));
        }
        else
        {
            attackDuration = GetRangedAttackDuration(attackType);
            StartCoroutine(PerformRangedAttackSequence(attackType));
        }

        yield return new WaitForSeconds(attackDuration);

        // Alterna entre los tres tipos de ataque secuencialmente
        _nextAttackIndex = (EBossAttackType)(((int)_nextAttackIndex + 1) % 3);
        yield break;
    }


    // ================================================================
    // DURACIÓN DE ATAQUES
    // ================================================================
    float GetMeleeAttackDuration(EBossAttackType type)
    {
        switch (type)
        {
            case EBossAttackType.BasicAttack: return 0.3f;
            case EBossAttackType.SpecialAttack1: return 1.0f;
            case EBossAttackType.SpecialAttack2: return 0.8f;
            default: return 0.5f;
        }
    }

    float GetRangedAttackDuration(EBossAttackType type)
    {
        switch (type)
        {
            case EBossAttackType.BasicAttack: return 0.4f;
            case EBossAttackType.SpecialAttack1: return 0.8f;
            case EBossAttackType.SpecialAttack2: return 1.2f;
            default: return 0.5f;
        }
    }


    // ================================================================
    // SECUENCIAS DE ATAQUE (CUERPO A CUERPO)
    // ================================================================
    IEnumerator PerformMeleeAttackSequence(EBossAttackType type)
    {
        if (_playerTarget == null) yield break;

        switch (type)
        {
            case EBossAttackType.BasicAttack:
                Debug.Log("MELEE: Ataque básico.");
                if (Vector3.Distance(transform.position, _playerTarget.position) < agentStoppingDistance + 0.1f)
                    DamagePlayer(_playerTarget.GetComponent<PlayerSalud>(), damageToPlayer * 2);
                break;

            case EBossAttackType.SpecialAttack1:
                Debug.Log("MELEE: Ataque de área (AoE).");
                yield return new WaitForSeconds(0.5f);
                ApplyRadialDamage(aoeRadius, damageToPlayer * aoeDamageMultiplier);
                break;

            case EBossAttackType.SpecialAttack2:
                Debug.Log("MELEE: Ataque DASH.");
                if (_rb != null)
                {
                    Vector3 dashDir = (_playerTarget.position - transform.position).normalized;
                    _rb.AddForce(dashDir * dashForce, ForceMode2D.Impulse);
                    yield return new WaitForSeconds(0.4f);
                    _rb.linearVelocity = Vector2.zero;
                }
                break;
        }
        yield break;
    }


    // ================================================================
    // SECUENCIAS DE ATAQUE (A DISTANCIA)
    // ================================================================
    IEnumerator PerformRangedAttackSequence(EBossAttackType type)
    {
        if (_playerTarget == null || firePoint == null) yield break;
        Vector2 targetDir = (_playerTarget.position - firePoint.position).normalized;

        switch (type)
        {
            case EBossAttackType.BasicAttack:
                Debug.Log("RANGED: Disparo único.");
                FireBullet(targetDir, bulletSpeed);
                break;

            case EBossAttackType.SpecialAttack1:
                Debug.Log("RANGED: Disparo triple.");
                yield return StartCoroutine(FireTripleShot(targetDir));
                break;

            case EBossAttackType.SpecialAttack2:
                Debug.Log("RANGED: Ráfaga circular.");
                yield return StartCoroutine(FireWideBurst());
                break;
        }
        yield break;
    }


    // ================================================================
    // ATAQUE ULTIMATE
    // ================================================================
    IEnumerator ExecuteUltimateAttack()
    {
        Debug.Log("ULTIMATE: Activada fase final.");
        _navAgent.isStopped = true;

        float distance = Vector3.Distance(transform.position, _playerTarget.position);
        bool isMeleePhase = distance <= meleeRange;

        float chargeTime = 2.0f;
        Debug.Log($"Cargando {chargeTime}s para {(isMeleePhase ? "MELEE Nova" : "RANGED Barrage")}");
        yield return new WaitForSeconds(chargeTime);

        if (isMeleePhase)
        {
            ApplyRadialDamage(10f, maxHP);
        }
        else
        {
            StartCoroutine(FireWideBurst(true));
        }

        yield return new WaitForSeconds(1.0f);
        _lastAttackTime = Time.time + 15f;
        yield break;
    }


    // ================================================================
    // FUNCIONES DE DAÑO
    // ================================================================
    private void DamagePlayer(PlayerSalud playerSalud, int damage)
    {
        if (playerSalud != null)
            playerSalud.RecibirDamage(damage);

        Debug.Log($"Daño infligido al jugador: {damage}");
    }


    private void ApplyRadialDamage(float radius, int damage)
    {
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(transform.position, radius, _playerLayerMask);

        foreach (Collider2D hit in hitObjects)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerSalud playerSalud = hit.GetComponent<PlayerSalud>();
                DamagePlayer(playerSalud, damage);
            }
        }

        Debug.Log($"Daño radial aplicado: {damage} en radio {radius}.");
    }


    // ================================================================
    // FUNCIONES DE DISPARO
    // ================================================================
    void FireBullet(Vector2 direction, float speed)
    {
        if (bulletPrefab == null || firePoint == null) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

        if (rb != null)
            rb.linearVelocity = direction * speed;
        else
            Debug.LogError("El prefab de bala no tiene Rigidbody2D.");
    }


    IEnumerator FireTripleShot(Vector2 targetDir)
    {
        float angleOffset = 25f;
        FireBullet(targetDir, bulletSpeed);
        yield return new WaitForSeconds(0.2f);
        FireBullet(RotateVector2(targetDir, angleOffset), bulletSpeed);
        yield return new WaitForSeconds(0.2f);
        FireBullet(RotateVector2(targetDir, -angleOffset), bulletSpeed);
        yield break;
    }


    IEnumerator FireWideBurst(bool isUltimate = false)
    {
        int count = isUltimate ? rangedBurstCount * 4 : rangedBurstCount;
        float delay = isUltimate ? 0.05f : 0.1f;

        for (int i = 0; i < count; i++)
        {
            float angle = Random.Range(0f, 360f);
            Vector2 randomDir = RotateVector2(Vector2.right, angle);
            FireBullet(randomDir, bulletSpeed * (isUltimate ? 1.5f : 0.8f));
            yield return new WaitForSeconds(delay);
        }
        yield break;
    }


    // ================================================================
    // FUNCIONES AUXILIARES Y DEBUG
    // ================================================================
    private Vector2 RotateVector2(Vector2 v, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        return new Vector2(
            v.x * Mathf.Cos(radians) - v.y * Mathf.Sin(radians),
            v.x * Mathf.Sin(radians) + v.y * Mathf.Cos(radians)
        );
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 0.5f, 1f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, rangedRange);

        Gizmos.color = new Color(1f, 0, 0, 0.6f);
        Gizmos.DrawWireSphere(transform.position, meleeRange);

        Gizmos.color = new Color(1f, 0.5f, 0, 0.8f);
        Gizmos.DrawWireSphere(transform.position, aoeRadius);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, meleeRange + (rangedRange - meleeRange) * ultimateHPThreshold);
    }
}
