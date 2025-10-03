using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class HeavyEnemy : BaseEnemy
{
    [Header("Movimiento Pesado")]
    [SerializeField] private float maxSpeed = 4f;
    [SerializeField] private float maxForce = 1.5f;
    [SerializeField] private float knockbackMultiplier = 0.5f;
    [SerializeField] private float detectionRadius = 10f;

    private Rigidbody2D _rb;
    private Transform _playerTransform;

    protected override void Awake()
    {
        base.Awake();
        _rb = GetComponent<Rigidbody2D>();
        if (_rb != null)
        {
            _rb.gravityScale = 0f;
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    private void FixedUpdate()
    {
        DetectPlayer();
        if (_rb == null || _playerTransform == null) return;

        Vector2 force = Pursuit(_playerTransform);
        _rb.AddForce(force);
        _rb.linearVelocity = Vector2.ClampMagnitude(_rb.linearVelocity, maxSpeed);
        RotateTowardsPlayer();
    }

    private void DetectPlayer()
    {
        if (_playerTransform != null) return;
        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRadius, LayerMask.GetMask("Player"));
        if (hit != null) _playerTransform = hit.transform;
    }

    private Vector2 Pursuit(Transform target)
    {
        Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
        if (targetRb == null) return Seek(target.position);

        Vector2 toTarget = target.position - transform.position;
        float lookAheadTime = toTarget.magnitude / (maxSpeed + targetRb.linearVelocity.magnitude);
        Vector2 predictedPosition = (Vector2)target.position + targetRb.linearVelocity * lookAheadTime;
        return Seek(predictedPosition);
    }

    private Vector2 Seek(Vector2 targetPosition)
    {
        Vector2 desiredVelocity = (targetPosition - (Vector2)transform.position).normalized * maxSpeed;
        Vector2 steeringForce = desiredVelocity - _rb.linearVelocity;
        return Vector2.ClampMagnitude(steeringForce, maxForce);
    }

    private void RotateTowardsPlayer()
    {
        if (_playerTransform == null) return;
        Vector2 direction = (_playerTransform.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        base.OnCollisionEnter2D(collision);
        if (_rb == null) return;

        if (collision.gameObject.CompareTag("Wall"))
            _rb.linearVelocity = Vector2.zero;

        if (collision.gameObject.CompareTag("Player"))
        {
            float speedFactor = Mathf.Clamp01(_rb.linearVelocity.magnitude / maxSpeed);
            float calculatedDamage = contactDamage * (1 + (speedFactor * 1.5f));

            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                Vector2 knockbackDir = (collision.transform.position - transform.position).normalized;
                playerHealth.TakeDamage(calculatedDamage, knockbackDir * knockbackMultiplier);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
