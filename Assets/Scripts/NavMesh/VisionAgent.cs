// ================================================================
// Archivo: VisionAgent.cs
// Descripción: Controla un agente con cono de visión que detecta al 
// jugador y lo persigue por un tiempo determinado antes de regresar 
// a su posición inicial.
// IMPLEMENTADO POR: Gael, David y Steve
// ================================================================

using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Gestiona la visión y persecución de un agente enemigo utilizando un cono de visión.
/// Al detectar al jugador dentro de su rango y ángulo, lo persigue por un tiempo limitado.
/// </summary>
public class VisionAgent : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform player; // A quién detecta (el jugador)
    private NavMeshAgent agent;

    [Header("Cono de visión")]
    [SerializeField] private float visionRadius = 6f;   // Distancia máxima de detección
    [SerializeField] private float visionAngle = 45f;   // Ángulo del cono de visión

    [Header("Comportamiento")]
    [SerializeField] private float chaseTime = 3f;      // Duración de la persecución
    [SerializeField] private float speed = 4f;          // Velocidad del agente

    private bool isChasing = false;                     // Indica si el agente está persiguiendo al jugador
    private Vector3 startPosition;                      // Posición inicial del agente
    private Coroutine chaseRoutine;                     // Referencia a la rutina de persecución

    /// <summary>
    /// Inicializa el componente NavMeshAgent.
    /// </summary>
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    /// <summary>
    /// Configura parámetros iniciales del agente y desactiva rotaciones automáticas.
    /// </summary>
    private void Start()
    {
        startPosition = transform.position;
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = speed;
    }

    /// <summary>
    /// Revisa continuamente si el jugador entra en el cono de visión.
    /// Si lo detecta, inicia una persecución temporal.
    /// </summary>
    private void Update()
    {
        if (!isChasing && player != null)
        {
            // Detecta si el jugador entra en el cono de visión
            if (IsInVisionCone())
            {
                if (chaseRoutine != null) StopCoroutine(chaseRoutine);
                chaseRoutine = StartCoroutine(ChaseForSeconds());
            }
        }
    }

    /// <summary>
    /// Determina si el jugador se encuentra dentro del radio y ángulo de visión del agente.
    /// </summary>
    /// <returns>True si el jugador está dentro del cono de visión, de lo contrario False.</returns>
    private bool IsInVisionCone()
    {
        Vector3 dirToPlayer = player.position - transform.position;

        // Verifica distancia
        if (dirToPlayer.magnitude > visionRadius) return false;

        // Verifica ángulo (usa transform.right si el sprite mira a la derecha)
        float angle = Vector3.Angle(transform.right, dirToPlayer);
        return angle < visionAngle;
    }

    /// <summary>
    /// Persigue al jugador durante un tiempo determinado antes de regresar a su posición inicial.
    /// </summary>
    private IEnumerator ChaseForSeconds()
    {
        isChasing = true;
        float timer = 0f;

        while (timer < chaseTime)
        {
            if (player != null)
                agent.SetDestination(player.position);

            timer += Time.deltaTime;
            yield return null;
        }

        // Termina la persecución
        agent.ResetPath();

        // Regresa a la posición inicial
        agent.SetDestination(startPosition);

        isChasing = false;
    }

    /// <summary>
    /// Dibuja el cono de visión del agente en la vista de escena para depuración.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRadius);

        Vector3 rightDir = Quaternion.Euler(0, 0, visionAngle) * transform.right;
        Vector3 leftDir = Quaternion.Euler(0, 0, -visionAngle) * transform.right;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + rightDir * visionRadius);
        Gizmos.DrawLine(transform.position, transform.position + leftDir * visionRadius);
    }

    /// <summary>
    /// Destruye a los enemigos con los que entra en contacto.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Destroy(collision.gameObject);
            Debug.Log("El agente con visión destruyó al enemigo.");
        }
    }
}
