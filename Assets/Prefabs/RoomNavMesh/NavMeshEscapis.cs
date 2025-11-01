/*******************************************************
 * NOMBRE DEL ARCHIVO: NavMeshEscapistEnemy.cs
 * DESCRIPCIÓN:
 * Enemigo que usa EXCLUSIVAMENTE NavMeshAgent para moverse
 * entre dos estados: Activo (huye) y Cansado (solo dispara).
 * Cumple: disparo hacia posición actual del jugador, sensación
 * ligera (alta aceleración, velocidad moderada), LoS con NavMesh,
 * temporizadores por coroutines, gizmos del punto de destino.
 *******************************************************/

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

// Estados IA
public enum EEscapistState { Activo, Cansado }

public class NavMeshEscapistEnemy : BaseEnemy
{
    [Header("Componentes NavMesh")]
    [SerializeField] private NavMeshAgent _agent;

    [Header("Configuración de Estados")]
    [SerializeField] private EEscapistState _currentState = EEscapistState.Activo;
    [SerializeField] private float tiempoCansancio = 4.0f;          // D) inicia al huir y pasa a Cansado al terminar
    [SerializeField] private float tiempoActivacion = 2.0f;         // A del estado Cansado
    [SerializeField] private float distanciaHuida = 10.0f;          // C: punto opuesto
    [SerializeField] private float radioDeteccion = 8.0f;           // B: radio de decisión
    [SerializeField] private float tiempoSinLoSParaPerseguir = 1.5f;// F: re-aplica A) si no hay LoS reciente

    [Header("Configuración de Disparo")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    [SerializeField] private float fireRateActivo = 0.5f;           // A: dispara hacia posición ACTUAL
    [SerializeField] private float fireRateCansado = 0.8f;          // D.Extra: más lento en Cansado

    [Header("Configuración Visual")]
    [SerializeField] private Color colorActivo = Color.white;
    [SerializeField] private Color colorCansado = Color.blue;

    [Header("Amenazas (tags o referencias)")]
    public bool usarTags = true;
    public string[] threatTags = new string[] { "Player" };
    public List<Transform> threatOverrides;
    public float retargetInterval = 0.25f;

    // Control interno
    private Coroutine _stateCoroutine;
    private float _lastLoSTime;
    private float _currentFireRate;
    private float _lastFireTime;
    private float _retargetTimer;
    private Transform _currentThreat;

    protected override void Start()
    {
        base.Start();

        if (_agent == null) _agent = GetComponent<NavMeshAgent>();
        if (_agent == null)
        {
            Debug.LogError("NavMeshEscapistEnemy requiere NavMeshAgent.");
            enabled = false;
            return;
        }

        // Movimiento “ligero”: alta aceleración, velocidad moderada
        _agent.acceleration = 25.0f;
        _agent.speed = 5.0f;
        _agent.angularSpeed = 120f;

        // Uso 2D con NavMesh de superficie
        _agent.updateRotation = false; // 2D
        _agent.updateUpAxis = false;   // 2D

        StartCoroutine(InitializeAgent());
        CambiarEstado(EEscapistState.Activo);
    }

    private IEnumerator InitializeAgent()
    {
        yield return null;
        if (_agent != null && _agent.isOnNavMesh)
            _agent.isStopped = false;
    }

    // Oculta Update de BaseEnemy si no es virtual
    protected new void Update()
    {
        if (_agent == null || !_agent.isOnNavMesh) return;

        // Retargeteo de amenaza
        _retargetTimer -= Time.deltaTime;
        if (_retargetTimer <= 0f)
        {
            _currentThreat = SeleccionarAmenaza();
            _retargetTimer = retargetInterval;
        }

        TryShoot(); // dispara hacia posición actual del jugador si el cooldown lo permite

        // Reanudación de movimiento en Activo (F)
        if (_currentState == EEscapistState.Activo && _currentThreat != null)
        {
            bool hasLoS = !_agent.Raycast(_currentThreat.position, out _); // LoS con NavMeshAgent [web:3]
            if (hasLoS) _lastLoSTime = Time.time;

            float timeSinceLastLoS = Time.time - _lastLoSTime;
            if (_agent.isStopped && timeSinceLastLoS > tiempoSinLoSParaPerseguir)
            {
                _agent.isStopped = false;
                _agent.SetDestination(_currentThreat.position);
            }
        }
    }

    // ÚNICA implementación para evitar ambigüedad
    private Transform SeleccionarAmenaza()
    {
        // 1) Si BaseEnemy expone _senses por capa (player layer), usarlo
        if (_senses != null)
        {
            var foundPlayers = _senses.GetAllObjectsByLayer(LayerMask.NameToLayer("Player"));
            if (foundPlayers != null && foundPlayers.Count > 0)
            {
                GameObject bestGo = null;
                float bestSqr = float.PositiveInfinity;
                foreach (var p in foundPlayers)
                {
                    if (p == null) continue;
                    float sqr = ((Vector2)p.transform.position - (Vector2)transform.position).sqrMagnitude;
                    if (sqr < bestSqr && sqr <= radioDeteccion * radioDeteccion)
                    {
                        bestSqr = sqr;
                        bestGo = p;
                    }
                }
                if (bestGo != null) return bestGo.transform;
            }
        }

        // 2) Fallback por tags o lista overrides
        Transform best = null;
        float bestSqrTag = float.PositiveInfinity;

        if (usarTags && threatTags != null && threatTags.Length > 0)
        {
            foreach (var tag in threatTags)
            {
                if (string.IsNullOrEmpty(tag)) continue;
                var arr = GameObject.FindGameObjectsWithTag(tag);
                foreach (var go in arr)
                {
                    float sqr = ((Vector2)go.transform.position - (Vector2)transform.position).sqrMagnitude;
                    if (sqr < bestSqrTag && sqr <= radioDeteccion * radioDeteccion)
                    {
                        bestSqrTag = sqr;
                        best = go.transform;
                    }
                }
            }
        }
        else if (threatOverrides != null && threatOverrides.Count > 0)
        {
            foreach (var t in threatOverrides)
            {
                if (t == null) continue;
                float sqr = ((Vector2)t.position - (Vector2)transform.position).sqrMagnitude;
                if (sqr < bestSqrTag && sqr <= radioDeteccion * radioDeteccion)
                {
                    bestSqrTag = sqr;
                    best = t;
                }
            }
        }

        return best;
    }

    private void TryShoot()
    {
        if (Time.time < _lastFireTime + _currentFireRate) return;
        if (bulletPrefab == null || firePoint == null) return;

        Transform target = _currentThreat;
        if (target == null) return;

        _lastFireTime = Time.time;

        // Disparo hacia la POSICIÓN ACTUAL del jugador
        Vector2 dir = ((Vector2)target.position - (Vector2)firePoint.position).normalized;

        GameObject go = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        var rb = go.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = dir * 15f; // o parametrizar velocidad
    }

    private bool HasNavLoS(Vector3 targetPosition)
    {
        return !_agent.Raycast(targetPosition, out _); // true si no hay obstáculo en NavMesh [web:3]
    }

    private void CambiarEstado(EEscapistState newState)
    {
        if (_stateCoroutine != null) StopCoroutine(_stateCoroutine);
        _currentState = newState;

        switch (newState)
        {
            case EEscapistState.Activo:
                _currentFireRate = fireRateActivo;
                if (_spriteRenderer != null) _spriteRenderer.color = colorActivo;
                _agent.isStopped = false;
                _stateCoroutine = StartCoroutine(EstadoActivoCoroutine());
                break;

            case EEscapistState.Cansado:
                _currentFireRate = fireRateCansado; // D.Extra
                if (_spriteRenderer != null) _spriteRenderer.color = colorCansado;
                if (_agent.isOnNavMesh)
                {
                    _agent.isStopped = true;
                    _agent.ResetPath(); // limpiar destino al cansarse [web:55]
                }
                _stateCoroutine = StartCoroutine(EstadoCansadoCoroutine());
                break;
        }
    }

    private void SetDestinationToPlayer(Vector3 playerPos)
    {
        _agent.isStopped = false;
        _agent.SetDestination(playerPos);
    }

    private void SetDestinationToFleePoint(Vector3 playerPos, float distance)
    {
        Vector3 fleeDir = (transform.position - playerPos).normalized;
        Vector3 desired = transform.position + fleeDir * Mathf.Max(1f, distance);

        if (NavMesh.SamplePosition(desired, out var hit, 1.0f, NavMesh.AllAreas))
        {
            _agent.isStopped = false;
            _agent.SetDestination(hit.position);
            Debug.DrawLine(transform.position, hit.position, Color.cyan, tiempoCansancio);
        }
        else
        {
            // C.2) fallback: hacia el jugador si el opuesto no es válido
            Vector3 toward = transform.position - fleeDir * Mathf.Max(1f, distance);
            if (NavMesh.SamplePosition(toward, out var hit2, 1.0f, NavMesh.AllAreas))
            {
                _agent.isStopped = false;
                _agent.SetDestination(hit2.position);
                Debug.DrawLine(transform.position, hit2.position, Color.yellow, tiempoCansancio);
            }
        }
    }

    // --- Coroutines de estados ---

    // Estado Cansado: solo dispara; sale por temporizador a Activo
    private IEnumerator EstadoCansadoCoroutine()
    {
        yield return new WaitForSeconds(tiempoActivacion); // tip: usar coroutines para tiempos [web:29]
        CambiarEstado(EEscapistState.Activo);
    }

    // Estado Activo: asigna destino, LoS, huida y temporizador de cansancio
    private IEnumerator EstadoActivoCoroutine()
    {
        while (_currentState == EEscapistState.Activo)
        {
            Transform target = _currentThreat;

            if (target == null)
            {
                _agent.isStopped = true;
                yield return new WaitForSeconds(0.25f);
                continue;
            }

            float dist = Vector3.Distance(transform.position, target.position);

            // A) Estar/entrar Activo: acercarse al jugador
            SetDestinationToPlayer(target.position);

            // A.1) Si hay LoS directo, detenerse
            if (HasNavLoS(target.position))
            {
                _agent.isStopped = true;
                _lastLoSTime = Time.time;
            }
            else
            {
                _agent.isStopped = false;
            }

            // B) Si el jugador está dentro del radio, ejecutar huida (C)
            if (dist <= radioDeteccion)
            {
                SetDestinationToFleePoint(target.position, distanciaHuida);

                // D) Comienza temporizador de Cansancio
                yield return new WaitForSeconds(tiempoCansancio);

                // E) Pasa a Cansado
                CambiarEstado(EEscapistState.Cansado);
                yield break;
            }

            // F) Si no hubo LoS en los últimos X segundos, re-aplica A)
            if (Time.time - _lastLoSTime > tiempoSinLoSParaPerseguir)
            {
                _agent.isStopped = false;
                _agent.SetDestination(target.position);
            }

            yield return null; // tick por frame
        }
    }

    // Gizmos: radio y destino actual (C.1)
    protected new void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, radioDeteccion); // depuración visual [web:28]

        if (_agent != null && _agent.hasPath)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(_agent.destination, 0.2f); // muestra punto de destino [web:28]
        }
    }
}
