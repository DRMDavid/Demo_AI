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
    [SerializeField] public float agentSpeed = 3.5f;
    [SerializeField] public float agentStoppingDistance = 1.0f;

    // ================================================================
    // CONFIGURACIÓN DE FSM
    // ================================================================
    [Header("RANGOS DE FSM")]
    [SerializeField] public float meleeRange = 3.0f;
    [SerializeField] public float rangedRange = 8.0f;
    [Range(0f, 1f)]
    [SerializeField] public float ultimateHPThreshold = 0.3f;

    // ================================================================
    // CONFIGURACIÓN DE ATAQUES
    // ================================================================
    [Header("CONFIGURACIÓN DE ATAQUE")]
    [SerializeField] public float attackCooldown = 1.5f;
    [SerializeField] public float dashForce = 15f;

    // ================================================================
    // PROYECTILES Y DAÑO
    // ================================================================
    [Header("PROYECTILES Y DAÑO")]
    [Tooltip("Pon aquí tus prefabs de bala (Normal, Veneno, Hielo, Explosiva)")]
    [SerializeField] public GameObject[] bulletPrefabs; // ARRAY NUEVO
    [SerializeField] public Transform firePoint;
    [SerializeField] public float bulletSpeed = 10f;
    [SerializeField] public int rangedBurstCount = 5;
    [SerializeField] public float aoeRadius = 4.0f;
    [SerializeField] public int aoeDamageMultiplier = 4;

    [Header("ATAQUE ESPIRAL")]
    [SerializeField] private int spiralShoots = 24; // Cantidad de balas en espiral

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
    
    // Control de Furia
    private bool _isEnraged = false;
    private SpriteRenderer _bossSprite;

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
        _bossSprite = GetComponentInChildren<SpriteRenderer>();

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
            _navAgent.speed = agentSpeed; // Usar variable modificada por furia
            _navAgent.SetDestination(_playerTarget.position);
        }

        // Chequear fase de furia constantemente
        CheckEnrage();
    }

    // Lógica nueva de Furia
    void CheckEnrage()
    {
        if (!_isEnraged && currentHP < maxHP * 0.4f) // Al 40% de vida
        {
            _isEnraged = true;
            Debug.Log(">>> BOSS ENRAGED: ¡MODO FURIA ACTIVADO! <<<");

            // Buffs
            agentSpeed *= 1.5f;
            attackCooldown *= 0.6f;
            bulletSpeed *= 1.2f;

            // Visual
            if (_bossSprite) _bossSprite.color = Color.red; // Se pone rojo
            if (vfx) vfx.TriggerShake(0.5f, 1.0f);
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

        if (currentHP <= maxHP * ultimateHPThreshold && !_isEnraged) return EBossState.Ultimate; // Solo una vez o controlado
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
            // Si está furioso, chance de ataque espiral sorpresa
            if (_isEnraged && Random.value > 0.6f)
            {
                Debug.Log("BOSS: Ataque Espiral Furia");
                attackDuration = 2.0f;
                yield return StartCoroutine(FireSpiralRoutine());
            }
            else
            {
                attackDuration = GetRangedAttackDuration(attackType);
                StartCoroutine(PerformRangedAttackSequence(attackType));
            }
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
    // NUEVO: ATAQUE ESPIRAL (Bullet Hell)
    // ================================================================
    IEnumerator FireSpiralRoutine()
    {
        float angleStep = 360f / spiralShoots;
        float currentAngle = 0f;

        for (int i = 0; i < spiralShoots; i++)
        {
            Vector2 dir = RotateVector2(Vector2.right, currentAngle);
            FireBullet(dir, bulletSpeed);
            currentAngle += 20f; // Rotación para efecto espiral
            
            // Si está furioso dispara más rápido
            yield return new WaitForSeconds(_isEnraged ? 0.02f : 0.05f);
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

    // Modificado para elegir bala aleatoria del array
    void FireBullet(Vector2 direction, float speed)
    {
        // CORRECCIÓN: Verificamos directamente el array 'bulletPrefabs'
        if (bulletPrefabs == null || bulletPrefabs.Length == 0 || firePoint == null) return;

        // Selección aleatoria de bala del array
        GameObject prefabToUse = bulletPrefabs[Random.Range(0, bulletPrefabs.Length)];
        
        GameObject bullet = Instantiate(prefabToUse, firePoint.position, Quaternion.identity);
        
        // Configurar bala
        Bullet bScript = bullet.GetComponent<Bullet>();
        if (bScript != null)
        {
            bScript.Init(direction, speed);
        }
        else
        {
            // Soporte por si usas una bala simple sin el script Bullet nuevo
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = direction * speed;
        }
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