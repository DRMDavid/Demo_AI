using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI; 

public class BossEnemy : BaseEnemy 
{
    // --- CONFIGURACIÓN DE NAVMESH Y MOVIMIENTO ---
    [Header("CONFIGURACIÓN DE NAVMESH")]
    private NavMeshAgent _navAgent;
    [Tooltip("Velocidad de movimiento del NavMeshAgent.")]
    public float agentSpeed = 3.5f;
    [Tooltip("Distancia a la que el agente debe detenerse para Arrive/Melee.")] 
    public float agentStoppingDistance = 1.0f; 

    [Header("RANGOS DE FSM (Ajustables en el Inspector)")]
    [Tooltip("Distancia para entrar en el MELEE STATE.")]
    public float meleeRange = 3.0f;          
    [Tooltip("Distancia máxima para el RANGED STATE.")]
    public float rangedRange = 8.0f;         
    [Tooltip("Porcentaje de vida (0.0 a 1.0) para activar ULTIMATE STATE.")]
    [Range(0f, 1f)]
    public float ultimateHPThreshold = 0.3f; 

    [Header("CONFIGURACIÓN DE ATAQUE (Ajustables)")]
    [Tooltip("Tiempo de espera entre ataques normales (Selector).")]
    public float attackCooldown = 1.5f;      
    [Tooltip("Fuerza usada para el ataque DASH (Melee Special 2).")]
    public float dashForce = 15f;         

    [Header("PROYECTILES Y DAÑO (Ajustables)")]
    [Tooltip("Prefab de la bala para ataques a distancia.")]
    public GameObject bulletPrefab;       
    [Tooltip("Punto de spawn de proyectiles.")]
    public Transform firePoint;           
    [Tooltip("Velocidad de los proyectiles.")]
    public float bulletSpeed = 10f;       
    [Tooltip("Número de balas para la Ráfaga Circular (Ranged Special 2).")]
    public int rangedBurstCount = 5;      
    [Tooltip("Radio de impacto del Ataque de Área (Melee Special 1).")]
    public float aoeRadius = 4.0f;
    [Tooltip("Multiplicador de daño para el Ataque de Área.")]
    public int aoeDamageMultiplier = 4;
    
    // --- Variables Internas de la FSM ---
    private EBossState _currentBossState = EBossState.IdleMove;
    private EBossAttackType _nextAttackIndex = EBossAttackType.BasicAttack;
    private Transform _playerTarget;        
    private float _lastAttackTime = 0f;
    private Rigidbody2D _rb;
    private int _playerLayerMask; // Máscara de capa para detección

    // --- Inicialización ---
    protected override void Start()
    {
        // 1. Inicialización de BaseEnemy (Salud, Senses)
        currentHP = maxHP;                            
        _senses = GetComponent<Senses>();
        _rb = GetComponent<Rigidbody2D>(); 

        // 2. Inicialización de NavMesh
        _navAgent = GetComponent<NavMeshAgent>();
        
        if (_navAgent == null)
            Debug.LogError("NavMeshAgent no encontrado. ¡Agregue el componente NavMeshAgent!");
        
        if (_navAgent != null)
        {
            _navAgent.updateRotation = false; 
            _navAgent.updateUpAxis = false;   
            _navAgent.speed = agentSpeed;
            _navAgent.stoppingDistance = agentStoppingDistance;
        }

        // 3. Buscar Target y Layers
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
            _playerTarget = playerObject.transform;
        else
            Debug.LogError("¡Jugador no encontrado! Asegúrate de que tenga la Tag 'Player'.");

        // Asume que tienes una capa llamada "Player" para las colisiones de daño
        _playerLayerMask = 1 << LayerMask.NameToLayer("Player"); 

        StartCoroutine(BossAILoop());
    }

    // --- UPDATE: Mantiene el movimiento de Seek y NO SPAMEA la consola ---
    void Update()
    {
        // Si estamos en IdleMove, el NavMeshAgent se mueve
        if (_navAgent != null && _playerTarget != null && _currentBossState == EBossState.IdleMove)
        {
            _navAgent.isStopped = false;
            _navAgent.speed = agentSpeed;
            _navAgent.SetDestination(_playerTarget.position);
        }
        // Se elimina el Debug.Log del Update para evitar el spam de consola
    }

    // --- CICLO PRINCIPAL DE LA FSM (Loguea al cambiar de estado) ---
    IEnumerator BossAILoop()
    {
        if (_playerTarget == null) yield break;
        EBossState previousState = EBossState.IdleMove;

        while (currentHP > 0)
        {
            EBossState newState = DetermineNextMainState();
            
            if (newState != _currentBossState)
            {
                // ✅ FIX: Loguea SOLO cuando el estado cambia
                Debug.Log($"FSM CHANGE: {previousState} -> {newState}"); 
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
        // FIX: Reemplaza la llamada a Die() privada con Destroy()
        Destroy(gameObject, 0.2f); 
        yield break;
    }

    // --- LÓGICA DE FLUJO Y ATAQUE ---
    
    // Ejecuta el Selector de Ataque y espera su duración
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
        yield break;
    }

    // --- COROUTINES DE ACCIÓN (Simulan el ataque sin animaciones) ---
    
    // Ataques Melee (Verifica daño por proximidad)
    IEnumerator PerformMeleeAttackSequence(EBossAttackType type)
    {
        if (_playerTarget == null) yield break;
        
        switch (type)
        {
            case EBossAttackType.BasicAttack: 
                Debug.Log("VERIFIED: MELEE - Ataque Básico (Golpe a corta distancia).");
                // ✅ Lógica de Ataque Básico: Si el jugador está MUY cerca, aplica daño
                if (Vector3.Distance(transform.position, _playerTarget.position) < agentStoppingDistance + 0.1f)
                    DamagePlayer(_playerTarget.GetComponent<PlayerSalud>(), damageToPlayer * 2); 
                yield return null; 
                break;
            case EBossAttackType.SpecialAttack1: 
                Debug.Log("VERIFIED: MELEE - Ataque Especial 1 (ÁREA: Carga + Impacto).");
                yield return new WaitForSeconds(0.5f); 
                ApplyRadialDamage(aoeRadius, damageToPlayer * aoeDamageMultiplier);
                yield return null;
                break;
            case EBossAttackType.SpecialAttack2: 
                Debug.Log("VERIFIED: MELEE - Ataque Especial 2 (DASH ejecutado con Rigidbody).");
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
    
    // Ataques Ranged (Disparo)
    IEnumerator PerformRangedAttackSequence(EBossAttackType type)
    {
        if (_playerTarget == null || firePoint == null) yield break;
        Vector2 targetDir = (_playerTarget.position - firePoint.position).normalized;

        switch (type)
        {
            case EBossAttackType.BasicAttack: 
                Debug.Log("VERIFIED: RANGED - Ataque Básico (Tiro Único)");
                FireBullet(targetDir, bulletSpeed);
                break; 
            case EBossAttackType.SpecialAttack1: 
                Debug.Log("VERIFIED: RANGED - Ataque Especial 1 (Tiro Triple)");
                yield return StartCoroutine(FireTripleShot(targetDir)); 
                break; 
            case EBossAttackType.SpecialAttack2: 
                Debug.Log("VERIFIED: RANGED - Ataque Especial 2 (Ráfaga Circular)");
                yield return StartCoroutine(FireWideBurst()); 
                break;
        }
        yield break;
    }

    IEnumerator ExecuteUltimateAttack()
    {
        Debug.Log("FSM VERIFIED: ULTIMATE - ¡INICIANDO FASE FINAL!");
        _navAgent.isStopped = true;
        
        float distance = Vector3.Distance(transform.position, _playerTarget.position);
        bool isMeleePhase = distance <= meleeRange; 
        
        float chargeTime = 2.0f;
        Debug.Log($"ULTIMATE: Cargando por {chargeTime}s. Ejecutará Ultimate {(isMeleePhase ? "MELEE (Nova)" : "RANGED (Barrage)")}");
        yield return new WaitForSeconds(chargeTime);

        if (isMeleePhase)
        {
            Debug.Log("ULTIMATE: NOVA DE CONTACTO ejecutada. Daño AoE + Empuje.");
            ApplyRadialDamage(10f, maxHP); 
        }
        else 
        {
            Debug.Log("ULTIMATE: BARRAGE CAÓTICO ejecutado. Disparos 360.");
            StartCoroutine(FireWideBurst(true)); 
        }
        
        yield return new WaitForSeconds(1.0f); 
        _lastAttackTime = Time.time + 15f; 
        yield break; 
    }
    
    // --- UTILIDADES (Lógica de Daño y Matemáticas) ---

    private void DamagePlayer(PlayerSalud playerSalud, int damage)
    {
        if (playerSalud != null)
        {
            playerSalud.RecibirDamage(damage); 
        }
        Debug.Log($"DAÑO: Infligido {damage} al jugador.");
    }

    private void ApplyRadialDamage(float radius, int damage)
    {
        // Usa la máscara del jugador para la detección de capas
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(transform.position, radius, _playerLayerMask);

        foreach (Collider2D hit in hitObjects)
        {
            if (hit.gameObject.CompareTag("Player")) 
            {
                PlayerSalud playerSalud = hit.GetComponent<PlayerSalud>();
                DamagePlayer(playerSalud, damage);
            }
        }
        Debug.Log($"DAÑO RADIAL: Daño {damage} aplicado en radio {radius}.");
    }
    
    void FireBullet(Vector2 direction, float speed)
    {
        if (bulletPrefab != null && firePoint != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = direction * speed;
            } else {
                Debug.LogError("El prefab de la bala no tiene Rigidbody2D. ¡El disparo falló!");
            }
        }
    }
    
    // Coroutine de Tiro Triple
    IEnumerator FireTripleShot(Vector2 targetDir)
    {
        float angleOffset = 25f; 
        FireBullet(targetDir, bulletSpeed);
        yield return new WaitForSeconds(0.2f);
        Vector2 dir1 = RotateVector2(targetDir, angleOffset);
        Vector2 dir2 = RotateVector2(targetDir, -angleOffset);
        FireBullet(dir1, bulletSpeed);
        yield return new WaitForSeconds(0.2f);
        FireBullet(dir2, bulletSpeed);
        yield break;
    }
    
    // Coroutine de Ráfaga Circular
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

    // Método RotateVector2 (utilidad matemática)
    private Vector2 RotateVector2(Vector2 v, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);
        
        return new Vector2(
            v.x * cos - v.y * sin,
            v.x * sin + v.y * cos
        );
    }
    
    // --- VISUALIZACIÓN DE GIZMOS EN EL EDITOR ---
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