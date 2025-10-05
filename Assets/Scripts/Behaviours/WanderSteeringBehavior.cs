/*
Integrantes del equipo:
- Hannin Abarca
- David Sánchez
- Gael Jiménez
*/

using UnityEngine;

/// <summary>
/// Comportamiento de Wander (deambular) para un agente con Rigidbody2D.
/// Adaptación 2D del código original del profesor.
/// </summary>
public class WanderSteeringBehavior : RigidbodySteeringBehaviours
{
    [Header("Configuración del Wander")]
    public float minWanderDistance = 3.0f;
    public float maxWanderDistance = 10.0f;
    public float radiusToTargetPositionTolerance = 1.0f;

    private Vector2 _currentTargetPosition; // Cambiado a Vector2 respecto a Vector3 en el código del profesor

    /// <summary>
    /// Calcula una nueva posición aleatoria a la que el agente se dirigirá
    /// </summary>
    public void UpdateWanderTargetPosition()
    {
        // Genera una dirección aleatoria normalizada
        // Modificación: se eliminó la componente Y porque ahora trabajamos en 2D
        Vector2 direction = new Vector2(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)).normalized;

        // Selecciona una distancia aleatoria dentro del rango permitido
        float randomDistance = Random.Range(minWanderDistance, maxWanderDistance);

        // Calcula la nueva posición objetivo
        // Modificación: casteamos transform.position a Vector2 para compatibilidad 2D
        _currentTargetPosition = (Vector2)transform.position + direction * randomDistance;
    }

    /// <summary>
    /// Inicializa el comportamiento y selecciona un primer objetivo aleatorio
    /// </summary>
    new public void Start()
    {
        base.Start(); // Primero manda a llamar el Start de la clase padre
        UpdateWanderTargetPosition(); // Inicializamos para el primer frame
    }

    /// <summary>
    /// Actualiza el movimiento del agente en cada frame físico
    /// </summary>
    public void FixedUpdate()
    {
        // Si el agente llegó al objetivo, selecciona uno nuevo
        // Modificación: casteamos transform.position a Vector2 para compatibilidad 2D
        if (Utilities.IsObjectInRange((Vector2)transform.position, _currentTargetPosition, radiusToTargetPositionTolerance))
        {
            UpdateWanderTargetPosition();
        }

        // Calcula la fuerza combinando Arrive y ObstacleAvoidance
        Vector2 steeringForce = Arrive(_currentTargetPosition) + ObstacleAvoidance();

        // Limita la magnitud de la fuerza
        steeringForce = Vector2.ClampMagnitude(steeringForce, maxForce);

        // Aplica la fuerza al Rigidbody2D
        // Modificación: _rb ahora es Rigidbody2D, por eso no usamos ForceMode
        _rb.AddForce(steeringForce);

        // Limita la velocidad máxima del agente
        if (_rb.linearVelocity.magnitude > maxSpeed)
        {
            _rb.linearVelocity = _rb.linearVelocity.normalized * maxSpeed;
        }
    }
}
