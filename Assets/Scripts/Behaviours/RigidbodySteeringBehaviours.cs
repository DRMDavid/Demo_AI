using System;
using UnityEngine;

/*
Integrantes del equipo:
- Hannin Abarca
- David Sánchez
- Gael Jiménez
*/

public class RigidbodySteeringBehaviours : MonoBehaviour
{
    public ESteeringBehaviors currentBehavior = ESteeringBehaviors.Seek;

    // Velocidad máxima a la que puede ir este agente.
    public float maxSpeed = 10.0f;

    // máxima fuerza que se le puede aplicar
    public float maxForce = 5.0f;

    // Ya no es necesaria porque lo calculamos dinámicamente.
    // public float lookAheadTime = 2.0f;

    // Radio de distancia desde el cual comenzaremos a frenar.
    public float arriveBrakeRadius = 5.0f;

    public float arriveToleranceRadius = 0.5f;
    public float arriveSpeedTolerance = 0.5f;

    // Poder activar y desactivar el Obstacle Avoidance
    public bool useObstacleAvoidance = true;
    public float obstacleAvoidanceDetectionRadius = 5.0f;
    public LayerMask obstacleAvoidanceLayerMask;

    // Componente que maneja las fuerzas y la velocidad de nuestro agente.
    protected Rigidbody2D _rb; // ⚠️ CAMBIO 2D

    // Posición del objetivo.
    public Vector2 _targetPosition = Vector2.zero; // ⚠️ CAMBIO 2D
    public Rigidbody2D _targetRb; // ⚠️ CAMBIO 2D: Rigidbody → Rigidbody2D

    protected bool _targetIsSet = false;

    public void SetTarget(Vector2 target, Rigidbody2D targetRb)
    {
        _targetPosition = target;
        _targetRb = targetRb;
        _targetIsSet = true;
    }

    public void RemoveTarget()
    {
        _targetIsSet = false;
        _targetRb = null; // lo quitamos, ahorita por pura seguridad, pero idealmente hay que quitarlo.
    }

    public void Start()
    {
        _rb = GetComponent<Rigidbody2D>(); // ⚠️ CAMBIO 2D
        if (_rb == null)
        {
            Debug.LogWarning($"No se encontró el rigidbody2D para el agente: {name}. ¿Sí está asignado?");
        }
    }

    public Vector2 Seek(Vector2 targetPosition)
    {
        // Si sí hay un objetivo, empezamos a hacer Seek, o sea, a perseguir ese objetivo.
        // Lo primero es obtener la dirección deseada. El método punta menos cola lo usamos con nuestra posición
        // como la cola, y la posición objetivo como la punta
        Vector2 puntaMenosCola = targetPosition - (Vector2)transform.position; // ⚠️ CAMBIO 2D
        Vector2 desiredDirection = puntaMenosCola.normalized; // normalized nos da la pura dirección con una magnitud de 1.

        // Ya que tenemos esa dirección, la multiplicamos por nuestra velocidad máxima posible, y eso es la velocidad deseada.
        Vector2 desiredVelocity = desiredDirection * maxSpeed;

        // La steering force es la diferencia entre la velocidad deseada y la velocidad actual
        return desiredVelocity - _rb.linearVelocity; // ⚠️ CAMBIO 2D
    }
}
