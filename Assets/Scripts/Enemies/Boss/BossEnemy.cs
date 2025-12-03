/*******************************************************
 * NOMBRE DEL ARCHIVO: BossEnemy.cs
 * AUTORES: Hannin Abarca, Gael Jimenez, David Sanchez
 * * DESCRIPCIÓN:
 * Controlador principal de la IA del Jefe Final.
 * Gestiona una Máquina de Estados Finita (FSM) para alternar entre:
 * - Persecución (IdleMove)
 * - Ataque Melee (Cuerpo a cuerpo)
 * - Ataque Ranged (Distancia con balas especiales)
 * - Ultimate (Ataque definitivo)
 * * CARACTERÍSTICAS NUEVAS:
 * - Modo Furia (Enrage): Aumenta velocidad y agresividad al 40% de vida.
 * - Bullet Hell: Ataque en espiral.
 * - Feedback Visual: Instancia números de daño flotantes.
 * * REFERENCIAS:
 * - Boss AI Pattern: https://www.youtube.com/watch?v=AD4JIXQDw0s
 *******************************************************/

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
    // SISTEMA DE VFX (Efectos Visuales)
    // ================================================================
    [Header("CONEXIÓN DE VFX")]
    [Tooltip("Script que gestiona las partículas y animaciones del Boss.")]
    public BossVFXManager vfx;

    // ================================================================
    // NAVEGACIÓN (NavMesh)
    // ================================================================
    [Header("CONFIGURACIÓN DE NAVMESH")]
    [SerializeField] public float agentSpeed = 3.5f;
    [SerializeField] public float agentStoppingDistance = 1.0f;
    private NavMeshAgent _navAgent;

    // ================================================================
    // MÁQUINA DE ESTADOS (FSM)
    // ================================================================
    [Header("RANGOS DE IA")]
    [SerializeField] public float meleeRange = 3.0f;
    [SerializeField] public float rangedRange = 8.0f;
    [Range(0f, 1f)]
    [SerializeField] public float ultimateHPThreshold = 0.3f; // 30% vida activa Ultimate

    // ================================================================
    // COMBATE
    // ================================================================
    [Header("CONFIGURACIÓN DE ATAQUE")]
    [SerializeField] public float attackCooldown = 1.5f;
    [SerializeField] public float dashForce = 15f;

    [Header("PROYECTILES")]
    [Tooltip("Lista de balas variadas (Veneno, Hielo, etc.)")]
    [SerializeField] public GameObject[] bulletPrefabs; 
    [SerializeField] public Transform firePoint;
    [SerializeField] public float bulletSpeed = 10f;
    [SerializeField] public int rangedBurstCount = 5;
    [SerializeField] public float aoeRadius = 4.0f;
    [SerializeField] public int aoeDamageMultiplier = 4;

    [Header("ATAQUE ESPIRAL")]
    [SerializeField] private int spiralShoots = 24; // Número de balas en la espiral

    // ================================================================
    // FEEDBACK VISUAL
    // ================================================================
    [Header("FEEDBACK DE DAÑO")]
    [Tooltip("Prefab del Texto Flotante (DamagePopup)")]
    public GameObject damagePopupPrefab;

    // ESTADOS INTERNOS
    private EBossState _currentBossState = EBossState.IdleMove;
    private EBossAttackType _nextAttackIndex = EBossAttackType.BasicAttack;
    private Transform _playerTarget;
    private float _lastAttackTime = 0f;
    private Rigidbody2D _rb;
    private int _playerLayerMask;
    
    // VARIABLES DE FURIA
    private bool _isEnraged = false;
    private SpriteRenderer _bossSprite;
    private const float FIXED_Z_POSITION = 0f;

    // ================================================================
    // MÉTODOS DE INICIALIZACIÓN
    // ================================================================
    protected override void Start()
    {
        base.Start();

        // Desactivamos IAs genéricas para usar la personalizada del Boss
        if (_senses != null) _senses.enabled = false;
        if (_steeringBehaviors != null) _steeringBehaviors.enabled = false;

        _rb = GetComponent<Rigidbody2D>();
        _navAgent = GetComponent<NavMeshAgent>();
        _bossSprite = GetComponentInChildren<SpriteRenderer>();

        // Configuración NavMesh 2D
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
        // Corrección de posición Z para evitar que desaparezca en 2D
        if (transform.position.z != FIXED_Z_POSITION)
            transform.position = new Vector3(transform.position.x, transform.position.y, FIXED_Z_POSITION);

        // Movimiento base si no está atacando
        if (_navAgent != null && _playerTarget != null && _currentBossState == EBossState.IdleMove)
        {
            _navAgent.isStopped = false;
            _navAgent.speed = agentSpeed; 
            _navAgent.SetDestination(_playerTarget.position);
        }

        CheckEnrage(); // Verificar estado de furia cada frame
    }

    // ================================================================
    // SISTEMA DE DAÑO Y FEEDBACK
    // ================================================================
    public void RecibirDamage(int damage)
    {
        currentHP -= damage;

        // Instanciar número flotante
        if (damagePopupPrefab != null)
        {
            // Z negativo (-2) para asegurar que se dibuje ENCIMA del boss
            Vector3 spawnPosition = transform.position + new Vector3(0, 0.5f, -2f); 
            GameObject popup = Instantiate(damagePopupPrefab, spawnPosition, Quaternion.identity);
            
            // Configurar el texto
            DamagePopup popupScript = popup.GetComponent<DamagePopup>();
            if (popupScript != null)
            {
                popupScript.Setup(damage);
            }
        }
        // (La lógica de muerte se maneja en el loop principal o BaseEnemy)
    }

    void CheckEnrage()
    {
        // Activa el modo furia si la salud baja del 40%
        if (!_isEnraged && currentHP < maxHP * 0.4f)
        {
            _isEnraged = true;
            Debug.Log(">>> BOSS ENRAGED: ¡MODO FURIA ACTIVADO! <<<");

            // Buffs de Furia
            agentSpeed *= 1.5f;          // 50% más rápido
            attackCooldown *= 0.6f;      // Ataca más seguido
            bulletSpeed *= 1.2f;         // Proyectiles más veloces

            // Feedback Visual (Rojo)
            if (_bossSprite) _bossSprite.color = Color.red;
            if (vfx) vfx.TriggerShake(0.5f, 1.0f);
        }
    }

    private void OnDestroy()
    {
        if (vfx != null) vfx.StopAllCoroutines();
    }

    // ================================================================
    // MÁQUINA DE ESTADOS (CORRUTINA PRINCIPAL)
    // ================================================================
    IEnumerator BossAILoop()
    {
        if (_playerTarget == null) yield break;

        while (currentHP > 0)
        {
            // 1. Decidir el siguiente estado basado en distancia y vida
            EBossState newState = DetermineNextMainState();

            // 2. Cambiar de estado
            if (newState != _currentBossState)
            {
                _navAgent.isStopped = true; // Parar para atacar
                _currentBossState = newState;
            }

            // 3. Ejecutar comportamiento del estado
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
                    _currentBossState = EBossState.IdleMove; // Volver a moverse tras el ulti
                    break;
                case EBossState.IdleMove:
                    yield return null;
                    break;
            }
            yield return null;
        }
        
        // Muerte
        Destroy(gameObject, 0.2f);
    }

    EBossState DetermineNextMainState()
    {
        if (_playerTarget == null) return EBossState.IdleMove;
        float distance = Vector3.Distance(transform.position, _playerTarget.position);

        if (currentHP <= maxHP * ultimateHPThreshold && !_isEnraged) return EBossState.Ultimate; 
        if (distance <= meleeRange) return EBossState.Melee;
        if (distance <= rangedRange) return EBossState.Ranged;

        return EBossState.IdleMove;
    }

    // ================================================================
    // LÓGICA DE COMBATE
    // ================================================================
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
            // Probabilidad de Ataque Espiral si está enfurecido
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
        _nextAttackIndex = (EBossAttackType)(((int)_nextAttackIndex + 1) % 3); // Ciclar ataques
    }

    // Métodos auxiliares para duración de ataques (Omitidos detalles por brevedad)
    float GetMeleeAttackDuration(EBossAttackType type) { return 0.8f; }
    float GetRangedAttackDuration(EBossAttackType type) { return 0.8f; }

    // ================================================================
    // EJECUCIÓN DE ATAQUES
    // ================================================================
    IEnumerator PerformMeleeAttackSequence(EBossAttackType type)
    {
        if (_playerTarget == null) yield break;
        // Lógica de ataque melee simplificada para el ejemplo
        // Incluye Dash y Golpe Básico
        if (type == EBossAttackType.BasicAttack)
        {
            if (vfx) vfx.TriggerSquash(0.2f, -0.2f, 0.2f);
            yield return new WaitForSeconds(0.2f);
            if(Vector3.Distance(transform.position, _playerTarget.position) < meleeRange)
                DamagePlayer(_playerTarget.GetComponent<PlayerSalud>(), damageToPlayer * 2);
        }
    }

    IEnumerator PerformRangedAttackSequence(EBossAttackType type)
    {
        if (_playerTarget == null || firePoint == null) yield break;
        Vector2 targetDir = (_playerTarget.position - firePoint.position).normalized;

        if (vfx) vfx.SpawnVisualEffect(vfx.shootFlashPrefab, firePoint.position, Quaternion.identity);

        switch (type)
        {
            case EBossAttackType.BasicAttack:
                FireBullet(targetDir, bulletSpeed);
                break;
            case EBossAttackType.SpecialAttack1: // Triple disparo
                yield return StartCoroutine(FireTripleShot(targetDir));
                break;
            case EBossAttackType.SpecialAttack2: // Ráfaga circular
                yield return StartCoroutine(FireWideBurst());
                break;
        }
    }

    IEnumerator FireSpiralRoutine()
    {
        float currentAngle = 0f;
        for (int i = 0; i < spiralShoots; i++)
        {
            Vector2 dir = RotateVector2(Vector2.right, currentAngle);
            FireBullet(dir, bulletSpeed);
            currentAngle += 20f; // Rotar para crear espiral
            yield return new WaitForSeconds(_isEnraged ? 0.02f : 0.05f);
        }
    }

    IEnumerator ExecuteUltimateAttack()
    {
        // Secuencia cinemática de ataque final
        _navAgent.isStopped = true;
        if (vfx) vfx.TriggerFlash(Color.magenta, 0.5f);
        yield return new WaitForSeconds(1.0f);
        
        // Explosión de balas
        StartCoroutine(FireWideBurst(true));
        
        yield return new WaitForSeconds(1.0f);
        _lastAttackTime = Time.time + 5f; // Descanso tras el ultimate
    }

    // ================================================================
    // UTILIDADES
    // ================================================================
    private void DamagePlayer(PlayerSalud playerSalud, int damage)
    {
        if (playerSalud != null) 
        {
            Debug.Log($"[BOSS] Golpeó al jugador por {damage} de daño.");
            playerSalud.RecibirDamage(damage);
        }
    }

    // Selecciona una bala aleatoria del arsenal y la dispara
    void FireBullet(Vector2 direction, float speed)
    {
        if (bulletPrefabs == null || bulletPrefabs.Length == 0 || firePoint == null) return;

        GameObject prefabToUse = bulletPrefabs[Random.Range(0, bulletPrefabs.Length)];
        GameObject bullet = Instantiate(prefabToUse, firePoint.position, Quaternion.identity);
        
        Bullet bScript = bullet.GetComponent<Bullet>();
        if (bScript != null) bScript.Init(direction, speed);
        else 
        {
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = direction * speed;
        }
    }

    IEnumerator FireTripleShot(Vector2 targetDir)
    {
        FireBullet(targetDir, bulletSpeed);
        yield return new WaitForSeconds(0.2f);
        FireBullet(RotateVector2(targetDir, 25f), bulletSpeed);
        yield return new WaitForSeconds(0.2f);
        FireBullet(RotateVector2(targetDir, -25f), bulletSpeed);
    }

    IEnumerator FireWideBurst(bool isUltimate = false)
    {
        int count = isUltimate ? rangedBurstCount * 3 : rangedBurstCount;
        for (int i = 0; i < count; i++)
        {
            float angle = Random.Range(0f, 360f);
            FireBullet(RotateVector2(Vector2.right, angle), bulletSpeed);
            yield return new WaitForSeconds(0.05f);
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
}