using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BaseEnemy : MonoBehaviour
{
    private Senses _senses;
    private RigidbodySteeringBehaviours _steeringBehaviors;

    [Header("Estadísticas de Combate")]
    public int maxHP = 100;
    public int currentHP;
    public int damageToPlayer = 1;

    [Header("Comportamiento de IA")]
    public int hpToFlee = 25;
    public float radiusBeforeStopMovingDuringFlee = 10.0f;

    [Header("Efectos Visuales de Daño")]
    public float damageFlashDuration = 0.1f;
    public Color damageFlashColor = Color.red;
    public float invulnerabilityTime = 0.2f;

    private SpriteRenderer _spriteRenderer;
    private Color _originalColor;
    private bool _canTakeDamage = true;
    
    private bool _canDamagePlayer = true;
    private float _damageCooldown = 1.0f;

    void Start()
    {
        currentHP = maxHP;
        _senses = GetComponent<Senses>();
        _steeringBehaviors = GetComponent<RigidbodySteeringBehaviours>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer != null) _originalColor = _spriteRenderer.color;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            TakeDamage(1);
        }
        
        List<GameObject> foundPlayers = _senses.GetAllObjectsByLayer(LayerMask.NameToLayer("Player"));
        GameObject nearestPlayer = foundPlayers.Any() 
            ? foundPlayers.OrderBy(p => Vector2.Distance(transform.position, p.transform.position)).FirstOrDefault() 
            : null;

        if (nearestPlayer != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, nearestPlayer.transform.position);

            if (currentHP <= hpToFlee)
            {
                _steeringBehaviors.currentBehavior = distanceToPlayer < radiusBeforeStopMovingDuringFlee ? ESteeringBehaviors.Flee : ESteeringBehaviors.DontMove;
            }
            else
            {
                _steeringBehaviors.currentBehavior = ESteeringBehaviors.Arrive;
            }

            _steeringBehaviors.SetTarget(nearestPlayer.transform.position, nearestPlayer.GetComponent<Rigidbody2D>());
        }
        else
        {
            _steeringBehaviors.RemoveTarget();
        }
    }

    public void TakeDamage(int damage)
    {
        if (!_canTakeDamage) return;
        currentHP -= 1;
        StartCoroutine(InvulnerabilityCoroutine());
        if (currentHP <= 0) Die();
        else StartCoroutine(DamageFlash());
    }

    private IEnumerator DamageFlash()
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = damageFlashColor;
            yield return new WaitForSeconds(damageFlashDuration);
            _spriteRenderer.color = _originalColor;
        }
    }

    private IEnumerator InvulnerabilityCoroutine()
    {
        _canTakeDamage = false;
        yield return new WaitForSeconds(invulnerabilityTime);
        _canTakeDamage = true;
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!_canDamagePlayer) return;
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerSalud saludDelPlayer = collision.gameObject.GetComponent<PlayerSalud>();
            if (saludDelPlayer != null)
            {
                saludDelPlayer.RecibirDamage(damageToPlayer);
                StartCoroutine(DamageCooldown());
            }
        }
    }

    private IEnumerator DamageCooldown()
    {
        _canDamagePlayer = false;
        yield return new WaitForSeconds(_damageCooldown);
        _canDamagePlayer = true;
    }
}