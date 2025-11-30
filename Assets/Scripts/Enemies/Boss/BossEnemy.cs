using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using NavMeshPlus.Extensions;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AgentOverride2d))]
public class BossEnemy : BaseEnemy
{
    // ================================================================
    // CONEXIÓN DE EFECTOS
    // ================================================================
    [Header("CONEXIÓN DE VFX")]
    [Tooltip("Arrastra aquí el objeto BossEffects que tiene el script VFXManager")]
    public BossVFXManager vfx;

    // ================================================================
    // CONFIGURACIÓN DE NAVMESH
    // ================================================================
    [Header("CONFIGURACIÓN DE NAVMESH")]
    private NavMeshAgent _navAgent;
    public float agentSpeed = 3.5f;
    public float agentStoppingDistance = 1.0f;

    // ================================================================
    // CONFIGURACIÓN DE FSM
    // ================================================================
    [Header("RANGOS DE FSM")]
    public float meleeRange = 3.0f;
    public float rangedRange = 8.0f;
    [Range(0f, 1f)]
    public float ultimateHPThreshold = 0.3f;

    // ================================================================
    // CONFIGURACIÓN DE ATAQUES
    // ================================================================
    [Header("CONFIGURACIÓN DE ATAQUE")]
    public float attackCooldown = 1.5f;
    public float dashForce = 15f;

    // ================================================================
    // PROYECTILES Y DAÑO
    // ================================================================
    [Header("PROYECTILES Y DAÑO")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 10f;
    public int rangedBurstCount = 5;
    public float aoeRadius = 4.0f;
    public int aoeDamageMultiplier = 4;

    // ================================================================
    // VARIABLES INTERNAS
    // ================================================================
    private EBossState _currentBossState = EBossState.IdleMove;
    private EBossAttackType _nextAttackIndex = EBossAttackType.BasicAttack;
    private Transform _playerTarget;
    private float _lastAttackTime = 0f;
    private Rigidbody2D _rb;
    private int _playerLayerMask;
    private const float FIXED_Z_POSITION = 0f;

    // ================================================================
    // MÉTODOS UNITY
    // ================================================================
    protected override void Start()
    {
        base.Start();

        if (_senses != null) _senses.enabled = false;
        if (_steeringBehaviors != null) _steeringBehaviors.enabled = false;

        _rb = GetComponent<Rigidbody2D>();
        _navAgent = GetComponent<NavMeshAgent>();

        if (_navAgent == null) { Debug.LogError("NavMeshAgent no encontrado."); return; }

        _navAgent.updateRotation = false;
        _navAgent.updateUpAxis = false;
        _navAgent.speed = agentSpeed;
        _navAgent.stoppingDistance = agentStoppingDistance;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null) _playerTarget = playerObject.transform;

        _playerLayerMask = 1 << LayerMask.NameToLayer("Player");

        StartCoroutine(BossAILoop());
    }

    void Update()
    {
        if (transform.position.z != FIXED_Z_POSITION)
            transform.position = new Vector3(transform.position.x, transform.position.y, FIXED_Z_POSITION);

        if (_navAgent != null && _playerTarget != null && _currentBossState == EBossState.IdleMove)
        {
            _navAgent.isStopped = false;
            _navAgent.speed = agentSpeed;
            _navAgent.SetDestination(_playerTarget.position);
        }
    }

    // Limpieza de seguridad por si el boss muere
    private void OnDestroy()
    {
        if (vfx != null) vfx.StopAllCoroutines();
    }

    // ================================================================
    // MÁQUINA DE ESTADOS
    // ================================================================
    IEnumerator BossAILoop()
    {
        if (_playerTarget == null) yield break;
        EBossState previousState = EBossState.IdleMove;

        while (currentHP > 0)
        {
            EBossState newState = DetermineNextMainState();

            if (newState != _currentBossState)
            {
                previousState = newState;
                _navAgent.isStopped = true;
                _currentBossState = newState;
            }

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

        // Destrucción
        Destroy(gameObject, 0.2f);
    }

    EBossState DetermineNextMainState()
    {
        if (_playerTarget == null) return EBossState.IdleMove;
        float distance = Vector3.Distance(transform.position, _playerTarget.position);

        if (currentHP <= maxHP * ultimateHPThreshold) return EBossState.Ultimate;
        if (distance <= meleeRange) return EBossState.Melee;
        if (distance <= rangedRange) return EBossState.Ranged;

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
    }

    IEnumerator HandleRangedState()
    {
        _navAgent.isStopped = true;
        if (Time.time > _lastAttackTime + attackCooldown)
        {
            _lastAttackTime = Time.time;
            yield return StartCoroutine(ExecuteAttack(_currentBossState));
        }
    }

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
        _nextAttackIndex = (EBossAttackType)(((int)_nextAttackIndex + 1) % 3);
    }

    float GetMeleeAttackDuration(EBossAttackType type)
    {
        switch (type)
        {
            case EBossAttackType.BasicAttack: return 0.5f;
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
    // SECUENCIAS DE ATAQUE CUERPO A CUERPO
    // ================================================================
    IEnumerator PerformMeleeAttackSequence(EBossAttackType type)
    {
        if (_playerTarget == null) yield break;

        switch (type)
        {
            case EBossAttackType.BasicAttack:
                Debug.Log("MELEE: Ataque básico.");
                if (vfx) vfx.TriggerSquash(0.2f, -0.2f, 0.2f);
                yield return new WaitForSeconds(0.1f);

                if (vfx)
                {
                    vfx.TriggerSquash(-0.3f, 0.3f, 0.2f);
                    vfx.SpawnVisualEffect(vfx.meleeImpactPrefab, transform.position + (Vector3.down * 0.5f), Quaternion.identity);
                }

                if (Vector3.Distance(transform.position, _playerTarget.position) < agentStoppingDistance + 0.5f)
                    DamagePlayer(_playerTarget.GetComponent<PlayerSalud>(), damageToPlayer * 2);
                break;

            case EBossAttackType.SpecialAttack1: // AOE
                Debug.Log("MELEE: Ataque de área (AoE).");
                if (vfx)
                {
                    vfx.TriggerShake(0.15f, 0.5f);
                    vfx.TriggerFlash(Color.red, 0.3f);
                }

                yield return new WaitForSeconds(0.5f);

                if (vfx)
                {
                    vfx.SpawnVisualEffect(vfx.shockwavePrefab, transform.position, Quaternion.identity);
                    vfx.TriggerSquash(0.5f, 0.5f, 0.4f);
                }
                ApplyRadialDamage(aoeRadius, damageToPlayer * aoeDamageMultiplier);
                break;

            case EBossAttackType.SpecialAttack2: // DASH
                Debug.Log("MELEE: Ataque DASH.");
                if (vfx)
                {
                    StartCoroutine(vfx.PlayDashGhostTrail(0.5f));
                    vfx.TriggerSquash(0.4f, -0.4f, 0.5f);
                }

                if (_rb != null)
                {
                    Vector3 dashDir = (_playerTarget.position - transform.position).normalized;
                    _rb.AddForce(dashDir * dashForce, ForceMode2D.Impulse);
                    yield return new WaitForSeconds(0.4f);
                    _rb.linearVelocity = Vector2.zero;

                    if (vfx) vfx.TriggerSquash(-0.2f, 0.2f, 0.2f);
                }
                break;
        }
    }

    // ================================================================
    // SECUENCIAS DE ATAQUE A DISTANCIA
    // ================================================================
    IEnumerator PerformRangedAttackSequence(EBossAttackType type)
    {
        if (_playerTarget == null || firePoint == null) yield break;
        Vector2 targetDir = (_playerTarget.position - firePoint.position).normalized;

        if (vfx) vfx.SpawnVisualEffect(vfx.shootFlashPrefab, firePoint.position, Quaternion.identity);

        switch (type)
        {
            case EBossAttackType.BasicAttack:
                Debug.Log("RANGED: Disparo único.");
                if (vfx) vfx.TriggerSquash(-0.15f, 0.1f, 0.1f);
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
    }

    // ================================================================
    // ATAQUE ULTIMATE (AQUÍ ESTÁ EL ARREGLO)
    // ================================================================
    IEnumerator ExecuteUltimateAttack()
    {
        Debug.Log("ULTIMATE: Activada fase final.");
        _navAgent.isStopped = true;

        float distance = Vector3.Distance(transform.position, _playerTarget.position);
        bool isMeleePhase = distance <= meleeRange;

        float chargeTime = 2.0f;

        // Variable para guardar la referencia del efecto
        GameObject chargeInstance = null;

        // FASE DE CARGA
        if (vfx)
        {
            vfx.TriggerShake(0.2f, chargeTime);
            vfx.TriggerFlash(Color.magenta, 0.5f);

            // ---> AQUI GUARDAMOS EL EFECTO EN LA VARIABLE <---
            chargeInstance = vfx.SpawnVisualEffect(vfx.chargePrefab, transform.position, Quaternion.identity);

            // Opcional: Hacer que siga al boss si se mueve (aunque aquí está quieto)
            if (chargeInstance != null) chargeInstance.transform.SetParent(transform);
        }

        yield return new WaitForSeconds(chargeTime);

        // ---> AQUI LO DESTRUIMOS ANTES DE DISPARAR <---
        if (chargeInstance != null) Destroy(chargeInstance);

        // FASE DE DISPARO
        if (vfx)
        {
            vfx.TriggerSquash(1.0f, 1.0f, 0.5f);
            vfx.TriggerFlash(Color.white, 0.2f);

            if (!isMeleePhase)
                vfx.SpawnVisualEffect(vfx.burstPrefab, transform.position, Quaternion.identity);
            else
                vfx.SpawnVisualEffect(vfx.shockwavePrefab, transform.position, Quaternion.identity);
        }

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
    }

    // ================================================================
    // FUNCIONES AUXILIARES
    // ================================================================
    private void DamagePlayer(PlayerSalud playerSalud, int damage)
    {
        if (playerSalud != null) playerSalud.RecibirDamage(damage);
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
    }

    void FireBullet(Vector2 direction, float speed)
    {
        if (bulletPrefab == null || firePoint == null) return;
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = direction * speed;
    }

    IEnumerator FireTripleShot(Vector2 targetDir)
    {
        float angleOffset = 25f;
        FireBullet(targetDir, bulletSpeed);
        if (vfx) vfx.TriggerSquash(-0.1f, 0.05f, 0.1f);

        yield return new WaitForSeconds(0.2f);

        FireBullet(RotateVector2(targetDir, angleOffset), bulletSpeed);
        if (vfx) vfx.SpawnVisualEffect(vfx.shootFlashPrefab, firePoint.position, Quaternion.identity);

        yield return new WaitForSeconds(0.2f);

        FireBullet(RotateVector2(targetDir, -angleOffset), bulletSpeed);
        if (vfx) vfx.SpawnVisualEffect(vfx.shootFlashPrefab, firePoint.position, Quaternion.identity);
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

            if (vfx && !isUltimate) vfx.SpawnVisualEffect(vfx.shootFlashPrefab, firePoint.position, Quaternion.identity);

            yield return new WaitForSeconds(delay);
        }
    }

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
    }
}