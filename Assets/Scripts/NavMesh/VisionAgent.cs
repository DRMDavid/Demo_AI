using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class VisionAgent : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform player; // A quién detecta (el jugador)
    private NavMeshAgent agent;

    [Header("Cono de visión")]
    [SerializeField] private float visionRadius = 6f;   // Distancia de detección
    [SerializeField] private float visionAngle = 45f;   // Ángulo del cono de visión

    [Header("Comportamiento")]
    [SerializeField] private float chaseTime = 3f;      // Tiempo de persecución
    [SerializeField] private float speed = 4f;          // Velocidad del agente

    private bool isChasing = false;
    private Vector3 startPosition;
    private Coroutine chaseRoutine;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        startPosition = transform.position;
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = speed;
    }

    private void Update()
    {
        if (!isChasing && player != null)
        {
            // Detectar al jugador dentro del cono
            if (IsInVisionCone())
            {
                if (chaseRoutine != null) StopCoroutine(chaseRoutine);
                chaseRoutine = StartCoroutine(ChaseForSeconds());
            }
        }
    }

    private bool IsInVisionCone()
    {
        Vector3 dirToPlayer = player.position - transform.position;

        // 1️⃣ Verifica distancia
        if (dirToPlayer.magnitude > visionRadius) return false;

        // 2️⃣ Verifica ángulo (usa right si el sprite mira a la derecha)
        float angle = Vector3.Angle(transform.right, dirToPlayer);
        return angle < visionAngle;
    }

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

        // 🔸 Termina la persecución
        agent.ResetPath();

        // 🔸 Extra: regresar a su posición inicial
        agent.SetDestination(startPosition);

        isChasing = false;
    }

    private void OnDrawGizmosSelected()
    {
        // Dibuja el cono de visión en la vista de escena
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRadius);

        Vector3 rightDir = Quaternion.Euler(0, 0, visionAngle) * transform.right;
        Vector3 leftDir = Quaternion.Euler(0, 0, -visionAngle) * transform.right;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + rightDir * visionRadius);
        Gizmos.DrawLine(transform.position, transform.position + leftDir * visionRadius);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Si choca con un enemigo, lo destruye
        if (collision.CompareTag("Enemy"))
        {
            Destroy(collision.gameObject);
            Debug.Log("El agente con visión destruyó al enemigo.");
        }
    }
}
