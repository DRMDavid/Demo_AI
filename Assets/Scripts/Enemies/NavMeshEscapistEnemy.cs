using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using NavMeshPlus.Extensions;

// Asegura que el enemigo tiene los componentes de NavMesh necesarios
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AgentOverride2d))]
public class NavMeshEscapistEnemy : BaseEnemy
{
    // --- ESTADOS --- (Req C)
    public enum EnemyState
    {
        Active,
        Tired
    }
    
    [Header("NavMeshEscapist Config")]
    [SerializeField] private EnemyState currentState = EnemyState.Active; 
    private NavMeshAgent agent;
    protected Transform player;
    private Color originalColor; 

    // --- MOVIMIENTO (Req B) y Detección ---
    [Header("Movimiento y Detección")]
    [Tooltip("La velocidad máxima del agente.")]
    [SerializeField] private float agentSpeed = 3f;
    [Tooltip("La aceleración rápida del agente (movimiento 'ligero').")]
    [SerializeField] private float agentAcceleration = 20f;
    [SerializeField] private float detectionRadius = 10f; // Radio para iniciar la huida (Req B Activo)
    [SerializeField] private float fleeDistance = 8f;     // Distancia que intenta huir (Req C Activo)

    // --- ESTADOS Y TIEMPOS ---
    [Header("Estados y Tiempos")]
    [SerializeField] private float activationDuration = 3f; // Duración del cansancio (Req A Cansado)
    [SerializeField] private float tirednessDuration = 5f; // Duración de actividad (Req D Activo)
    [SerializeField] private float timeWithoutLOSUntilSeek = 2.0f; // Tiempo sin LOS para reanudar persecución (Req F Activo)
    private float timeSinceLastSawPlayer = 0f;
    private Coroutine activationCoroutine; 
    private Coroutine tirednessCoroutine;
    private bool isFleeing = false;
    
    // --- COMBATE ---
    [Header("Combate")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float shootCooldownActive = 1.0f;
    [SerializeField] private float shootCooldownTired = 2.0f; // Dispara más lento (Req D.Extra Cansado)
    [SerializeField] private float bulletSpeed = 15f; // Velocidad de la bala
    private float nextShootTime = 0f;

    // --- RAYCAST Y GIZMOS ---
    [Header("Raycast (Línea de Visión)")]
    [SerializeField] private LayerMask lineOfSightMask; // Capas que bloquean la visión (Req A.1 Activo)
    private Vector3 fleeDebugPosition; // Para el gizmo (Req C.1 Activo)

    // --- VISUALES ---
    [Header("Visuales de Cansancio")]
    [SerializeField] private Color tiredBlinkColor = Color.blue; // Parpadeo azul (Req D Cansado)
    [SerializeField] private float blinkInterval = 0.2f; // Velocidad del parpadeo
    private Coroutine blinkCoroutine;
    private const float FIXED_Z_POSITION = 0f; // Corrige el bug de visibilidad Z

    // --- INICIALIZACIÓN (Req D) ---

    protected override void Start()
    {
        base.Start();
        // Neutralizamos la IA base (Steering Behaviors) para evitar conflictos
        if (_senses != null) _senses.enabled = false;
        if (_steeringBehaviors != null) _steeringBehaviors.enabled = false;

        if (_spriteRenderer != null)
        {
            originalColor = _spriteRenderer.color;
        }

        agent = GetComponent<NavMeshAgent>();
        // Configuración NavMeshAgent 2D y "ligero" (Req B)
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = agentSpeed;
        agent.acceleration = agentAcceleration;
        agent.enabled = true; 
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        // Inicia el ciclo principal
        if (currentState == EnemyState.Active) TransitionToActive();
        else TransitionToTired();
    }

    // --- UPDATE (MAQUINA DE ESTADOS) ---

    void Update()
    {
        // 🔴 ARREGLO Z: Fuerza el eje Z a 0 (Soluciona problemas de visibilidad por Z)
        if (transform.position.z != FIXED_Z_POSITION)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, FIXED_Z_POSITION);
        }
        
        if (currentHP <= 0 || player == null)
        {
            if (agent.isOnNavMesh && agent.isActiveAndEnabled)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }
            return; 
        }

        if (nextShootTime > 0)
        {
            nextShootTime -= Time.deltaTime;
        }
        
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
    
    // --- LÓGICA DE ESTADO ACTIVO ---
    
    private void HandleActiveState()
    {
        bool hasLOS = CheckLOS();
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // 1. Huida (Flee) (Req B/C)
        if (distanceToPlayer <= detectionRadius && !isFleeing)
        {
            isFleeing = true;
            if (TryFlee())
            {
                // Req D (Activo): Inicia el temporizador de Cansancio SÓLO si se logra huir
                if (tirednessCoroutine != null) StopCoroutine(tirednessCoroutine);
                tirednessCoroutine = StartCoroutine(TirednessTimer());
            } 
            else
            {
                isFleeing = false; 
            }
        }
        
        // 2. Movimiento/Visión (Persecución y Stop)
        if (isFleeing)
        {
            agent.isStopped = false; 
            if (!agent.pathPending && agent.remainingDistance < 0.1f)
            {
                isFleeing = false;
                agent.ResetPath();
            }
        }
        else if (hasLOS)
        {
            // Req A.1 (Activo): Si tiene LOS, detiene el movimiento.
            if(agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }
            timeSinceLastSawPlayer = 0f;
        }
        else
        {
            // Req F (Activo): Perseguir si pierde LOS por un tiempo.
            agent.isStopped = false;
            timeSinceLastSawPlayer += Time.deltaTime;

            if (timeSinceLastSawPlayer >= timeWithoutLOSUntilSeek)
            {
                // Req A (Activo): Asigna la posición del jugador como destino (Perseguir)
                agent.SetDestination(player.position);
                timeSinceLastSawPlayer = 0f; 
            }
        }

        // 3. Disparo (Req A)
        TryShoot(player.position); 
    }

    // --- LÓGICA DE ESTADO CANSADO ---
    
    private void HandleTiredState()
    {
        // Req C (Cansado): Únicamente dispara.
        TryShoot(player.position);
    }

    // --- TRANSICIONES Y COROUTINES ---

    private void TransitionToActive()
    {
        currentState = EnemyState.Active;
        isFleeing = false;
        timeSinceLastSawPlayer = 0f;
        
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = originalColor;
        }
        // 🛑 Detener parpadeo azul
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);

        // Req A (Activo): Asigna la posición del jugador
        if (agent.isOnNavMesh)
        {
            agent.isStopped = false; 
            if (player != null)
            {
                 agent.SetDestination(player.position);
            }
        }
        if (activationCoroutine != null) StopCoroutine(activationCoroutine);
    }

    private void TransitionToTired()
    {
        // Req A (Cansado): Deja de moverse, se detiene
        currentState = EnemyState.Tired;
        
        if (agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();      
        }

        // Req A (Cansado): Inicia el temporizador de Activación
        if (activationCoroutine != null) StopCoroutine(activationCoroutine);
        activationCoroutine = StartCoroutine(ActivationTimer());
        
        // 💡 Inicia el parpadeo azul (Req D Cansado)
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
        blinkCoroutine = StartCoroutine(TirednessBlink());

        if (tirednessCoroutine != null) StopCoroutine(tirednessCoroutine);
    }

    // Req A/B (Cansado): Temporizador de descanso
    private IEnumerator ActivationTimer()
    {
        yield return new WaitForSeconds(activationDuration);
        TransitionToActive();
    }
    
    // Req E (Activo): Temporizador de actividad
    private IEnumerator TirednessTimer()
    {
        yield return new WaitForSeconds(tirednessDuration);
        TransitionToTired();
    }
    
    // 💡 NUEVA COROUTINE: Parpadeo visual (Req D Cansado)
    private IEnumerator TirednessBlink()
    {
        if (_spriteRenderer == null) yield break;

        while (currentState == EnemyState.Tired)
        {
            _spriteRenderer.color = tiredBlinkColor; // Parpadea a Azul
            yield return new WaitForSeconds(blinkInterval);
            _spriteRenderer.color = originalColor; // Vuelve al color original
            yield return new WaitForSeconds(blinkInterval);
        }
    }

    // --- LÓGICA DE HUÍDA ---

    private bool TryFlee()
    {
        // Req C: Calcula la posición de huida (dirección opuesta al jugador)
        Vector3 fleeDirection = (transform.position - player.position).normalized;
        Vector3 targetPosition = transform.position + (fleeDirection * fleeDistance);

        fleeDebugPosition = targetPosition; 

        NavMeshHit navHit;
        // 1. Muestra debug/gizmo (Req C.1) y busca un punto válido en la dirección de huida
        if (NavMesh.SamplePosition(targetPosition, out navHit, fleeDistance * 0.5f, NavMesh.AllAreas))
        {
            fleeDebugPosition = navHit.position;
            agent.SetDestination(fleeDebugPosition);
            return true;
        }
        else
        {
            // Req C.2: Si no es válido, intenta en dirección opuesta (hacia el jugador)
            targetPosition = transform.position - (fleeDirection * fleeDistance);
            fleeDebugPosition = targetPosition; 

            if (NavMesh.SamplePosition(targetPosition, out navHit, fleeDistance * 0.5f, NavMesh.AllAreas))
            {
                fleeDebugPosition = navHit.position;
                agent.SetDestination(fleeDebugPosition);
                return true;
            }
        }
        
        fleeDebugPosition = transform.position;
        return false;
    }


    // --- ACCIONES DE COMBATE Y VISIÓN ---
    
    protected void TryShoot(Vector3 targetPos)
    {
        // 1. Apuntar y rotar (Req A)
        Vector3 direction = (targetPos - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // 2. Cooldown (Req D.Extra)
        float currentCooldown = (currentState == EnemyState.Active) ? shootCooldownActive : shootCooldownTired;

        if (nextShootTime <= 0)
        {
            nextShootTime = currentCooldown;
            Shoot(transform.rotation, direction); // Llamada a la función de disparo que aplica velocidad
        }
    }
    
    private bool CheckLOS()
    {
        if (player == null) return false;

        Vector3 direction = (player.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, player.position);

        // Raycast para verificar si hay obstáculos
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distance, lineOfSightMask);

        // Debug Gizmo (Req: "debug/gizmo del raycast")
        Color rayColor = (hit.collider == null) ? Color.green : Color.red;
        Debug.DrawRay(transform.position, direction * distance, rayColor);

        return hit.collider == null;
    }
    
    // 🎯 ARREGLO DE DISPARO: Se hace público para ser llamado desde TryShoot con la dirección.
    protected void Shoot(Quaternion rotation, Vector3 direction)
    {
        if (bulletPrefab != null && shootPoint != null)
        {
             GameObject newBullet = Instantiate(bulletPrefab, shootPoint.position, rotation);
             
             // 🎯 ARREGLO DE BALAS FLOTANTES: Aplicar velocidad al Rigidbody2D de la bala.
             Rigidbody2D rb2d = newBullet.GetComponent<Rigidbody2D>();
             if (rb2d != null)
             {
                 rb2d.linearVelocity = direction * bulletSpeed; 
             }
             // Si tu bala usa un script Bullet.cs, esa script debe estar moviendo su Transform/Rigidbody.
        }
    }

    // Sobrescribe el método base que no usaremos.
    protected void Shoot() 
    {
        // No hace nada.
    }

    // Sobrescribe la colisión de BaseEnemy para no hacer daño por contacto.
    private void OnCollisionStay2D(Collision2D collision)
    {
        // No hace daño por contacto.
    }

    // --- DEBUG GIZMOS ---
    
    private void OnDrawGizmosSelected()
    {
        // Dibuja el radio de detección (Req B)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Dibuja la posición de huida (Req C.1)
        if (Application.isPlaying && isFleeing)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(fleeDebugPosition, 0.5f);
            Gizmos.DrawLine(transform.position, fleeDebugPosition);
        }
    }
}