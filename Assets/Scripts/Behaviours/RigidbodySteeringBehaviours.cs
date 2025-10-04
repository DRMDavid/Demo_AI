using System;
using UnityEngine;

public class RigidbodySteeringBehaviours : MonoBehaviour
{
    public ESteeringBehaviors currentBehavior = ESteeringBehaviors.Seek;
    public float maxSpeed = 10.0f;
    public float maxForce = 5.0f;
    public float arriveBrakeRadius = 5.0f;
    public float arriveToleranceRadius = 0.5f;
    public float arriveSpeedTolerance = 0.5f;
    public bool useObstacleAvoidance = true;
    public float obstacleAvoidanceDetectionRadius = 5.0f;
    public LayerMask obstacleAvoidanceLayerMask;
    protected Rigidbody2D _rb;
    public Vector2 _targetPosition = Vector2.zero;
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
        _targetRb = null;
    }

    public void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        if (_rb == null)
        {
            Debug.LogWarning($"No se encontró el rigidbody2D para el agente: {name}. ¿Sí está asignado?");
        }
    }

    public Vector2 Seek(Vector2 targetPosition)
    {
        Vector2 puntaMenosCola = Utilities.PuntaMenosCola(targetPosition, transform.position);
        Vector2 desiredDirection = puntaMenosCola.normalized;
        Vector2 desiredVelocity = desiredDirection * maxSpeed;
        return desiredVelocity - _rb.linearVelocity;
    }

    public Vector2 Flee(Vector2 targetPosition)
    {
        return -Seek(targetPosition);
    }

    public Vector2 PredictPosition(Vector2 startingTargetPosition, Vector2 targetVelocity)
    {
        float lookAheadCalculado = (startingTargetPosition - (Vector2)transform.position).magnitude / maxSpeed;
        return startingTargetPosition + targetVelocity * lookAheadCalculado;
    }

    public Vector2 Pursuit(Vector2 targetPosition)
    {
        if (_targetRb == null) return Seek(targetPosition);
        Vector2 predictedPosition = PredictPosition(_targetPosition, _targetRb.linearVelocity);
        return Seek(predictedPosition);
    }

    public Vector2 Evade(Vector2 targetPosition)
    {
        return -Pursuit(targetPosition);
    }

    public Vector2 Arrive(Vector2 targetPosition)
    {
        if (!Utilities.IsObjectInRange(transform.position, targetPosition, arriveBrakeRadius))
        {
            return Seek(targetPosition);
        }
        
        Vector2 arrowToTarget = targetPosition - (Vector2)transform.position;
        float distanceToTarget = arrowToTarget.magnitude;
        if (distanceToTarget <= arriveToleranceRadius && _rb.linearVelocity.magnitude <= arriveSpeedTolerance)
        {
            _rb.linearVelocity = Vector2.zero;
            return Vector2.zero;
        }
        
        float desiredSpeed = Mathf.Lerp(0.0f, maxSpeed, distanceToTarget / arriveBrakeRadius);
        Vector2 desiredVelocity = arrowToTarget.normalized * desiredSpeed;
        return desiredVelocity - _rb.linearVelocity;
    }

    public Vector2 ObstacleAvoidance()
    {
        Vector2 steeringForce = Vector2.zero;
        if (useObstacleAvoidance)
        {
            var obstacles = Utilities.GetObjectsInRadius(transform.position, obstacleAvoidanceDetectionRadius, obstacleAvoidanceLayerMask);
            foreach (var obstacle in obstacles)
            {
                if (obstacle == null) continue;
                float distanceToObstacle = ((Vector2)transform.position - (Vector2)obstacle.transform.position).magnitude;
                steeringForce += Flee(obstacle.transform.position) * (1 - (distanceToObstacle / obstacleAvoidanceDetectionRadius));
            }
        }
        return steeringForce;
    }
    
    void FixedUpdate()
    {
        if (!_targetIsSet)
        {
            if (_rb.linearVelocity != Vector2.zero) _rb.linearVelocity = Vector2.Lerp(_rb.linearVelocity, Vector2.zero, Time.fixedDeltaTime * 5f);
            return;
        }

        Vector2 steeringForce = Vector2.zero;
        switch (currentBehavior)
        {
            case ESteeringBehaviors.DontMove: _rb.linearVelocity = Vector2.zero; break;
            case ESteeringBehaviors.Seek: steeringForce = Seek(_targetPosition); break;
            case ESteeringBehaviors.Flee: steeringForce = Flee(_targetPosition); break;
            case ESteeringBehaviors.Pursuit: steeringForce = Pursuit(_targetPosition); break;
            case ESteeringBehaviors.Evade: steeringForce = Evade(_targetPosition); break;
            case ESteeringBehaviors.Arrive: steeringForce = Arrive(_targetPosition); break;
            default: throw new ArgumentOutOfRangeException();
        }

        steeringForce += ObstacleAvoidance();
        steeringForce = Vector2.ClampMagnitude(steeringForce, maxForce);
        _rb.AddForce(steeringForce);

        if (_rb.linearVelocity.magnitude > maxSpeed)
        {
            _rb.linearVelocity = _rb.linearVelocity.normalized * maxSpeed;
        }
    }
}