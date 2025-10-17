using UnityEngine;
using System.Collections;

/*
Integrantes del equipo:
- Hannin Abarca
- David Sánchez
- Gael Jiménez

Referencia:
Este código se basó en las ideas y mecánicas presentadas en el siguiente video:
https://www.youtube.com/watch?v=YI5wRkTuok0
*/

public class EnemigoPesado : BaseEnemy
{
    [Header("Ajustes Enemigo Pesado")]
    public float acceleration = 1.5f;     // Baja aceleración
    public float maxSpeed = 8f;           // Alta velocidad máxima
    public float pursuitRange = 50f;      // Alcance para empezar a perseguir
    public float wallCollisionResetSpeed = 0.0f; // Velocidad al chocar con pared

    private Rigidbody2D _rb;
    private GameObject _player;

    // --- START ---
    protected override void Start()
    {
        base.Start();

        _rb = GetComponent<Rigidbody2D>();
        if (_rb == null)
            Debug.LogError("El EnemigoPesado necesita un Rigidbody2D en el prefab.");

        if (_steeringBehaviors == null)
            Debug.LogError("El EnemigoPesado necesita el componente RigidbodySteeringBehaviours en el prefab.");

        // Por defecto no se mueve hasta detectar jugador
        if (_steeringBehaviors != null)
            _steeringBehaviors.currentBehavior = ESteeringBehaviors.DontMove;
    }

    // --- UPDATE ---
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            TakeDamage(1);
        }

        if (_steeringBehaviors == null) return;

        // Buscar jugador si aún no está cacheado
        if (_player == null)
        {
            var players = GameObject.FindGameObjectsWithTag("Player");
            if (players.Length > 0)
                _player = players[0];
        }

        if (_player == null) return;

        Rigidbody2D playerRb = _player.GetComponent<Rigidbody2D>();
        if (playerRb == null) return; // seguridad

        float distanceToPlayer = Vector2.Distance(transform.position, _player.transform.position);

        // (B) Si el jugador está en rango, empieza a perseguir
        if (distanceToPlayer < pursuitRange)
        {
            _steeringBehaviors.currentBehavior = ESteeringBehaviors.Pursuit;
            _steeringBehaviors.SetTarget(_player.transform.position, playerRb);
        }
        else
        {
            _steeringBehaviors.currentBehavior = ESteeringBehaviors.DontMove;
        }

        // (C) Movimiento pesado: lenta aceleración, pero velocidad máxima alta
        if (_rb != null)
        {
            _rb.linearVelocity = Vector2.ClampMagnitude(_rb.linearVelocity, maxSpeed);
            _rb.linearVelocity = Vector2.Lerp(
                _rb.linearVelocity,
                _rb.linearVelocity.normalized * maxSpeed,
                acceleration * Time.deltaTime
            );
        }
    }

    // --- COLISIONES ---
    void OnCollisionEnter2D(Collision2D collision)
    {
        // (E) Si choca con pared, resetea su velocidad
        if (collision.gameObject.layer == LayerMask.NameToLayer("Walls") ||
            collision.gameObject.CompareTag("Wall"))
        {
            if (_rb != null)
                _rb.linearVelocity = Vector2.zero;
        }

        // (F) Daño proporcional a la velocidad al tocar al jugador
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerSalud saludDelPlayer = collision.gameObject.GetComponent<PlayerSalud>();
            if (saludDelPlayer != null && _rb != null)
            {
                float damageMultiplier = _rb.linearVelocity.magnitude * 0.5f; // Ajusta factor si quieres más daño
                int totalDamage = Mathf.CeilToInt(damageToPlayer * damageMultiplier);
                saludDelPlayer.RecibirDamage(totalDamage);
            }
        }
    }
}
