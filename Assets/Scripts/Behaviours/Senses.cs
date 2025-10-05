/*
Integrantes del equipo:
- Hannin Abarca
- David Sánchez
- Gael Jiménez
*/

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Gestiona la percepción del entorno (cono de visión) para un agente 2D.
/// Adaptación del código del profesor a un entorno 2D.
/// </summary>
public class Senses : MonoBehaviour
{
    [Header("Configuración de Visión")]
    [Tooltip("El alcance máximo de la visión del enemigo.")]
    public float radioDeDeteccion = 20.0f;

    [Tooltip("El ángulo del cono de visión en grados.")]
    [Range(0, 360)]
    public float anguloDeVision = 120f;

    [Tooltip("Las capas (Layers) en las que se buscarán objetivos.")]
    [SerializeField] private LayerMask desiredDetectionLayers;

    private List<GameObject> _foundGameObjects = new List<GameObject>();
    public List<GameObject> FoundGameObjects => _foundGameObjects;

    private bool _isPlayerDetected = false;

    void Update()
    {
        DetectarObjetos(); // Detectamos todos los objetos dentro del radio y ángulo
    }

    /// <summary>
    /// Detecta objetos en el cono de visión del agente
    /// </summary>
    private void DetectarObjetos()
    {
        _foundGameObjects.Clear();
        _isPlayerDetected = false;

        // Obtenemos todos los colliders dentro del radio de detección filtrando por capas
        Collider2D[] collidersInRadius = Physics2D.OverlapCircleAll(transform.position, radioDeDeteccion, desiredDetectionLayers);

        foreach (var col in collidersInRadius)
        {
            if (col.gameObject == this.gameObject) continue;

            Vector2 directionToTarget = ((Vector2)col.transform.position - (Vector2)transform.position).normalized;

            // Modificación: usamos transform.up como vector 2D para el cálculo del ángulo
            if (Vector2.Angle((Vector2)transform.up, directionToTarget) < anguloDeVision / 2)
            {
                _foundGameObjects.Add(col.gameObject);

                if (col.CompareTag("Player"))
                {
                    _isPlayerDetected = true;
                }
            }
        }
    }

    /// <summary>
    /// Obtiene todos los objetos detectados en una layer específica
    /// </summary>
    public List<GameObject> GetAllObjectsByLayer(int layer)
    {
        return _foundGameObjects.Where(obj => obj != null && obj.layer == layer).ToList();
    }

    public List<GameObject> GetPlayers()
    {
        return GetAllObjectsByLayer(LayerMask.NameToLayer("Player"));
    }

    public List<GameObject> GetEnemies()
    {
        return GetAllObjectsByLayer(LayerMask.NameToLayer("Enemy"));
    }

    public List<GameObject> GetEnemyBullets()
    {
        return GetAllObjectsByLayer(LayerMask.NameToLayer("EnemyBullet"));
    }

    // --- MÉTODO MODIFICADO PARA 2D ---
    // Comentario: El código del profesor estaba pensado para 3D, usando Vector3 y filtrando por radio con Utilities.
    // Cambios realizados en esta versión:
    // 1) Se eliminó todo lo relacionado con Utilities.GetObjectsInRadius y Vector3.
    // 2) Se usa Physics2D.OverlapCircleAll para detección eficiente en 2D.
    // 3) Se adaptó el cálculo del ángulo de visión a 2D con Vector2.Angle.
    // 4) Se mantiene la lógica de detectar jugadores y enemigos mediante tags y layers.
    // 5) OnDrawGizmos original del profesor no se incluyó, se puede añadir si se desea visualizar en escena.
}
