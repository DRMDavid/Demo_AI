/*******************************************************
 * NOMBRE DEL ARCHIVO: BaseEnemy.cs
 * AUTOR: Gael, Steve y David
 * BASADO EN: Clase Agent.cs del profesor de IA
 * DESCRIPCIÓN:
 * Script que maneja la lógica básica de enemigos en el juego:
 *  - Salud y daño al jugador.
 *  - Invulnerabilidad temporal tras recibir daño.
 *  - Flash visual al recibir daño.
 *  - Comportamiento de IA: perseguir jugador, huir si la salud es baja.
 *  - Control de cooldown para daño al jugador.
 * 
 * APORTACIONES PERSONALES:
 *  - Implementación desde cero siguiendo la idea del profesor.
 *  - Manejo de flash de daño, cooldown de ataque e invulnerabilidad.
 *  - Integración con Senses y RigidbodySteeringBehaviours.
 *  - Ajustes de IA y comportamiento de huida personalizados.
 * 
 * FUENTES CONSULTADAS:
 *  - Concepto del profesor de IA (Agent.cs)
 *  - Documentación de Unity sobre Rigidbody2D, Coroutine y SpriteRenderer
 * 
 *******************************************************/

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BaseEnemy : MonoBehaviour
{
    // --- Referencias a sistemas de IA ---
    protected Senses _senses;                          // Sistema de detección de jugadores/objetos
    protected RigidbodySteeringBehaviours _steeringBehaviors; // Control de movimiento/IA

    [Header("Estadísticas de Combate")]
    public int maxHP = 100;                            // Salud máxima del enemigo
    public int currentHP;                              // Salud actual del enemigo
    public int damageToPlayer = 1;                     // Daño que inflige al jugador al colisionar

    [Header("Comportamiento de IA")]
    public int hpToFlee = 25;                          // Salud mínima para huir
    public float radiusBeforeStopMovingDuringFlee = 10.0f; // Distancia mínima para detenerse durante huida

    [Header("Efectos Visuales de Daño")]
    public float damageFlashDuration = 0.1f;           // Duración del flash al recibir daño
    public Color damageFlashColor = Color.red;        // Color del flash
    public float invulnerabilityTime = 0.2f;          // Tiempo de invulnerabilidad tras recibir daño

    // --- Variables privadas ---
    private SpriteRenderer _spriteRenderer;           // Referencia al SpriteRenderer del enemigo
    private Color _originalColor;                      // Color original para restaurar después del flash
    private bool _canTakeDamage = true;               // Controla si puede recibir daño

    private bool _canDamagePlayer = true;             // Controla si puede infligir daño al jugador
    private float _damageCooldown = 1.0f;             // Tiempo entre ataques al jugador

    // --- Inicialización ---
    protected virtual void Start()
    {
        currentHP = maxHP;                            // Inicializa la salud al máximo
        _senses = GetComponent<Senses>();             // Obtiene componente de detección
        _steeringBehaviors = GetComponent<RigidbodySteeringBehaviours>(); // Obtiene componente de IA/movimiento

        // Busca el SpriteRenderer en el objeto o en hijos
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer == null)
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // Guarda color original o lanza advertencia si no hay SpriteRenderer
        if (_spriteRenderer != null)
            _originalColor = _spriteRenderer.color;
        else
            Debug.LogWarning($"{name} no tiene SpriteRenderer en este objeto ni en hijos.");
    }

    // --- Lógica principal de IA ---
    void Update()
    {
        // Detecta todos los jugadores presentes
        List<GameObject> foundPlayers = _senses.GetAllObjectsByLayer(LayerMask.NameToLayer("Player"));

        // Selecciona al jugador más cercano
        GameObject nearestPlayer = foundPlayers.Any()
            ? foundPlayers.OrderBy(p => Vector2.Distance(transform.position, p.transform.position)).FirstOrDefault()
            : null;

        if (nearestPlayer != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, nearestPlayer.transform.position);

            // Cambia comportamiento de IA según la salud
            if (currentHP <= hpToFlee)
            {
                _steeringBehaviors.currentBehavior = distanceToPlayer < radiusBeforeStopMovingDuringFlee
                    ? ESteeringBehaviors.Flee      // Huye si el jugador está cerca
                    : ESteeringBehaviors.DontMove; // Se detiene si está lejos
            }
            else
            {
                _steeringBehaviors.currentBehavior = ESteeringBehaviors.Arrive; // Persigue al jugador
            }

            // Actualiza la posición objetivo del enemigo
            _steeringBehaviors.SetTarget(nearestPlayer.transform.position, nearestPlayer.GetComponent<Rigidbody2D>());
        }
        else
        {
            // No hay jugadores, elimina objetivo
            _steeringBehaviors.RemoveTarget();
        }
    }

    // --- Recibir daño ---
    public void TakeDamage(int damage)
    {
        if (!_canTakeDamage) return;                   // Ignora si está en invulnerabilidad

        currentHP -= damage;                            // Reduce la salud
        StartCoroutine(InvulnerabilityCoroutine());    // Activa invulnerabilidad temporal

        if (currentHP <= 0)
            Die();                                     // Muere si la salud llega a 0
        else
            StartCoroutine(DamageFlash());             // Flash de daño
    }

    // Flash visual al recibir daño
    private IEnumerator DamageFlash()
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = damageFlashColor;
            yield return new WaitForSeconds(damageFlashDuration);
            _spriteRenderer.color = _originalColor;
        }
    }

    // Invulnerabilidad temporal
    private IEnumerator InvulnerabilityCoroutine()
    {
        _canTakeDamage = false;
        yield return new WaitForSeconds(invulnerabilityTime);
        _canTakeDamage = true;
    }

    // --- Muerte ---
    private void Die()
    {
        if (_spriteRenderer != null)
            _spriteRenderer.color = Color.gray; // Feedback visual de muerte

        Destroy(gameObject, 0.2f); // Retardo breve antes de destruir
    }

    // --- Colisión con el jugador ---
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!_canDamagePlayer) return;                 // Ignora si está en cooldown

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerSalud saludDelPlayer = collision.gameObject.GetComponent<PlayerSalud>();
            if (saludDelPlayer != null)
            {
                saludDelPlayer.RecibirDamage(damageToPlayer);
                StartCoroutine(DamageCooldown());    // Activa cooldown de daño
            }
        }
    }

    // Control de cooldown para infligir daño al jugador
    private IEnumerator DamageCooldown()
    {
        _canDamagePlayer = false;
        yield return new WaitForSeconds(_damageCooldown);
        _canDamagePlayer = true;
    }
}
