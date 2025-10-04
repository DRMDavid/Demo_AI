using UnityEngine;

public class WanderSteeringBehavior : RigidbodySteeringBehaviours
{
    public float minWanderDistance = 3.0f;
    public float maxWanderDistance = 10.0f;
    public float radiusToTargetPositionTolerance = 1.0f;
    private Vector2 _currentTargetPosition;

    public void UpdateWanderTargetPosition()
    {
        Vector2 direction = new Vector2(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)).normalized;
        float randomDistance = Random.Range(minWanderDistance, maxWanderDistance);
        _currentTargetPosition = (Vector2)transform.position + direction * randomDistance;
    }

    new public void Start()
    {
        base.Start();
        UpdateWanderTargetPosition();
    }
    
    public void FixedUpdate()
    {
        if (Utilities.IsObjectInRange((Vector2)transform.position, _currentTargetPosition, radiusToTargetPositionTolerance))
        {
            UpdateWanderTargetPosition();
        }

        Vector2 steeringForce = Arrive(_currentTargetPosition);
        steeringForce += ObstacleAvoidance();
        steeringForce = Vector2.ClampMagnitude(steeringForce, maxForce);
        _rb.AddForce(steeringForce);

        if (_rb.linearVelocity.magnitude > maxSpeed)
        {
            _rb.linearVelocity = _rb.linearVelocity.normalized * maxSpeed;
        }
    }
}