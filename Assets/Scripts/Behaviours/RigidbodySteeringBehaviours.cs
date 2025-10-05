using System;
using UnityEngine;

public class RigidbodySteeringBehaviours : MonoBehaviour
{
    public ESteeringBehaviors currentBehavior = ESteeringBehaviors.Seek;
    
    // Velocidad máxima a la que puede ir este agente.
    public float maxSpeed = 10.0f;
    
    // Máxima fuerza que se le puede aplicar (fuerza de redirección o steering force).
    public float maxForce = 5.0f;
    
    // Radio de distancia desde el cual el agente comienza a frenar (para Arrive).
    public float arriveBrakeRadius = 5.0f;
    public float arriveToleranceRadius = 0.5f;
    public float arriveSpeedTolerance = 0.5f;
    
    // Poder activar y desactivar el Obstacle Avoidance
    public bool useObstacleAvoidance = true;
    public float obstacleAvoidanceDetectionRadius = 5.0f;
    public LayerMask obstacleAvoidanceLayerMask;
    
    // Componente que maneja las fuerzas y la velocidad de nuestro agente.
    protected Rigidbody2D _rb;
    
    // Posición del objetivo.
    public Vector2 _targetPosition = Vector2.zero;
    // Rigidbody del objetivo (necesario para Pursuit/Evade).
    public Rigidbody2D _targetRb; 
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
        // Se quita la referencia del Rigidbody del objetivo por seguridad.
        _targetRb = null;
    }

    public void Start()
    {
        // Se asegura de obtener el Rigidbody2D al iniciar.
        _rb = GetComponent<Rigidbody2D>();
        if (_rb == null)
        {
            Debug.LogWarning($"No se encontró el rigidbody2D para el agente: {name}. ¿Sí está asignado?");
        }
    }

    public Vector2 Seek(Vector2 targetPosition)
    {
        // 1. Obtener la dirección deseada (Vector punta menos cola).
        Vector2 puntaMenosCola = targetPosition - (Vector2)transform.position;
        // 2. Normalizar la dirección para obtener un vector de magnitud 1.
        Vector2 desiredDirection = puntaMenosCola.normalized; 
        // 3. Multiplicar por la velocidad máxima para obtener la velocidad deseada.
        Vector2 desiredVelocity = desiredDirection * maxSpeed;
        // 4. Calcular la Steering Force (Fuerza de Redirección): deseada - actual.
        // La steering force es la diferencia entre la velocidad deseada y la velocidad actual
        return desiredVelocity - _rb.linearVelocity;
    }

    public Vector2 Flee(Vector2 targetPosition)
    {
        // Flee es lo mismo que Seek, pero en la dirección opuesta.
        return -Seek(targetPosition);
    }

    public Vector2 PredictPosition(Vector2 startingTargetPosition, Vector2 targetVelocity)
    {
        // Calculamos el tiempo de anticipación (look-ahead time) dinámicamente.
        // Fórmula: distancia entre agente y objetivo / velocidad máxima del agente.
        float lookAheadCalculado = (startingTargetPosition - (Vector2)transform.position).magnitude / maxSpeed;
        return startingTargetPosition + targetVelocity * lookAheadCalculado;
    }

    public Vector2 Pursuit(Vector2 targetPosition)
    {
        // Si no hay rigidbody del objetivo, se degrada a un simple Seek.
        if (_targetRb == null) return Seek(targetPosition);
        Vector2 predictedPosition = PredictPosition(_targetPosition, _targetRb.linearVelocity);
        return Seek(predictedPosition);
    }

    public Vector2 Evade(Vector2 targetPosition)
    {
        // Evade es lo mismo que Pursuit, pero en la dirección opuesta (el signo '-').
        return -Pursuit(targetPosition);
    }

    public Vector2 Arrive(Vector2 targetPosition)
    {
        // Si no estamos dentro del radio de frenado, no frenamos y hacemos Seek.
        if (!Utilities.IsObjectInRange(transform.position, targetPosition, arriveBrakeRadius))
        {
            return Seek(targetPosition);
        }
        
        Vector2 arrowToTarget = targetPosition - (Vector2)transform.position;
        float distanceToTarget = arrowToTarget.magnitude;
        
        // Si ya estamos muy cerca y nuestra velocidad es baja, nos detenemos completamente.
        if (distanceToTarget <= arriveToleranceRadius && _rb.linearVelocity.magnitude <= arriveSpeedTolerance)
        {
            _rb.linearVelocity = Vector2.zero;
            return Vector2.zero;
        }
        
        // Si sí estamos dentro del rango de frenado, calculamos la velocidad deseada con una reducción gradual (Lerp).
        float desiredSpeed = Mathf.Lerp(0.0f, maxSpeed, distanceToTarget / arriveBrakeRadius);
        Vector2 desiredVelocity = arrowToTarget.normalized * desiredSpeed;
        return desiredVelocity - _rb.linearVelocity;
    }

    public Vector2 ObstacleAvoidance()
    {
        Vector2 steeringForce = Vector2.zero;
        // Si la evasión de obstáculos está activada, se detectan los obstáculos.
        if (useObstacleAvoidance)
        {
            // Usamos OverlapCircleAll para la detección de obstáculos en 2D.
            Collider2D[] obstacles = Physics2D.OverlapCircleAll(transform.position, obstacleAvoidanceDetectionRadius, obstacleAvoidanceLayerMask);
            
            // Sumamos las fuerzas de huida para todos los obstáculos detectados.
            foreach (var obstacle in obstacles)
            {
                if (obstacle == null || obstacle.attachedRigidbody == _rb) continue;
                float distanceToObstacle = ((Vector2)transform.position - (Vector2)obstacle.transform.position).magnitude;
                
                // La fuerza de huida es inversamente proporcional a la distancia (más cerca = más fuerza).
                steeringForce += Flee(obstacle.transform.position) * (1 - (distanceToObstacle / obstacleAvoidanceDetectionRadius));
            }
        }
        return steeringForce;
    }
    
    void FixedUpdate()
    {
        // Ver si hay una posición objetivo a la cual moverse.
        if (!_targetIsSet)
        {
            // Si no hay objetivo, frenamos suavemente al agente.
            if (_rb.linearVelocity != Vector2.zero) _rb.linearVelocity = Vector2.Lerp(_rb.linearVelocity, Vector2.zero, Time.fixedDeltaTime * 5f);
            return;
        }

        Vector2 steeringForce = Vector2.zero;
        switch (currentBehavior)
        {
            case ESteeringBehaviors.DontMove: _rb.linearVelocity = Vector2.zero; break; // le hacemos la velocidad 0 para que deje de moverse completamente.
            case ESteeringBehaviors.Seek: steeringForce = Seek(_targetPosition); break;
            case ESteeringBehaviors.Flee: steeringForce = Flee(_targetPosition); break;
            case ESteeringBehaviors.Pursuit: steeringForce = Pursuit(_targetPosition); break;
            case ESteeringBehaviors.Evade: steeringForce = Evade(_targetPosition); break;
            case ESteeringBehaviors.Arrive: steeringForce = Arrive(_targetPosition); break;
            default: throw new ArgumentOutOfRangeException();
        }

        steeringForce += ObstacleAvoidance();
        
        // La steering force no puede ser mayor que la maxForce, pero sí puede ser menor.
        steeringForce = Vector2.ClampMagnitude(steeringForce, maxForce);
        // Aplicamos esta fuerza para mover a nuestro agente.
        _rb.AddForce(steeringForce);

        if (_rb.linearVelocity.magnitude > maxSpeed)
        {
            _rb.linearVelocity = _rb.linearVelocity.normalized * maxSpeed;
        }
    }
    
    // Dibuja información de depuración en la escena.
    protected void OnDrawGizmos()
    {
        if (!Application.isPlaying || !_targetIsSet)
            return;
        
        // Dibuja el target (caja roja)
        Gizmos.color = Color.red;
        Gizmos.DrawCube(_targetPosition, Vector2.one * 0.5f);

        Vector2 displayTargetPosition = _targetPosition;
        Vector2 steeringForce = Vector2.zero;
        
        switch (currentBehavior)
        {
            case ESteeringBehaviors.Pursuit:
            case ESteeringBehaviors.Evade:
                if (_targetRb != null)
                {
                    Vector2 predictedPosition = PredictPosition(_targetPosition, _targetRb.linearVelocity);
                    // Dibuja la posición predicha (caja amarilla)
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawCube(predictedPosition, Vector2.one * 0.5f);
                    displayTargetPosition = predictedPosition;
                }
                break;
        }

        // Línea del agente al target o posición predicha (amarilla)
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, displayTargetPosition);
        
        // Línea de la velocidad deseada (verde)
        Vector2 directionToPosition = (displayTargetPosition - (Vector2)transform.position).normalized;
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + directionToPosition * maxSpeed);
        
        // Línea de la velocidad actual (azul)
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + _rb.linearVelocity);
        
        // Línea de la fuerza final aplicada (magenta)
        switch (currentBehavior)
        {
            case ESteeringBehaviors.Seek: steeringForce = Seek(_targetPosition); break;
            case ESteeringBehaviors.Flee: steeringForce = Flee(_targetPosition); break;
            case ESteeringBehaviors.Pursuit: steeringForce = Pursuit(_targetPosition); break;
            case ESteeringBehaviors.Evade: steeringForce = Evade(_targetPosition); break;
            case ESteeringBehaviors.Arrive: steeringForce = Arrive(_targetPosition); break;
        }
        steeringForce += ObstacleAvoidance();
        steeringForce = Vector2.ClampMagnitude(steeringForce, maxForce);
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + steeringForce);

        // Dibuja el radio de detección de obstáculos.
        if (useObstacleAvoidance)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, obstacleAvoidanceDetectionRadius);
        }
    }
}