// ================================================================
// Archivo: enemyia.cs
// Descripción: Controla el comportamiento básico de un
// enemigo que sigue al jugador usando un NavMeshAgent.
// IMPLEMENTADO POR: Gael, David y Steve
// ================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyPrueba : MonoBehaviour
{
    [Header("Configuración del Enemigo")]
    [Tooltip("Transform del objetivo a seguir (normalmente el jugador).")]
    [SerializeField] private Transform target;

    [Tooltip("Velocidad de movimiento del enemigo.")]
    [SerializeField] private float movementSpeed = 5f;

    // Referencia al componente NavMeshAgent
    private NavMeshAgent agent;

    private void Awake()
    {
        // Obtiene y guarda la referencia al NavMeshAgent del enemigo
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        // Configura el agente para un entorno 2D
        agent.speed = movementSpeed;
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    private void Update()
    {
        // Actualiza constantemente el destino hacia el objetivo (jugador)
        agent.SetDestination(target.position);
    }

    /// <summary>
    /// Detecta colisiones con el jugador y aplica daño.
    /// </summary>
    /// <param name="other">Collider que entra en el trigger.</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Si el enemigo toca al jugador
        if (other.CompareTag("Player"))
        {
            // Intenta obtener el script de movimiento del jugador
            PlayerMovimiento2D jugador = other.GetComponent<PlayerMovimiento2D>();

            // Si el jugador existe, se aplica daño
            if (jugador != null)
            {
                jugador.RecibirDano(1); // Resta una vida (puede ajustarse)
            }
        }
    }
}
