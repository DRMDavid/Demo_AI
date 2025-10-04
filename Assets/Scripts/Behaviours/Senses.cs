using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Gestiona la percepción del entorno (cono de visión) para un agente 2D.
/// </summary>
public class Senses : MonoBehaviour
{
    [Header("Configuración de Visión")]
    [Tooltip("El alcance máximo de la visión del enemigo.")]
    public float radioDeDeteccion = 20.0f;

    [Tooltip("El ángulo del cono de visión en grados.")]
    [Range(0, 360)] // <-- ¡AQUÍ ESTÁ EL SLIDER!
    public float anguloDeVision = 120f;

    [Tooltip("Las capas (Layers) en las que se buscarán objetivos.")]
    [SerializeField] private LayerMask desiredDetectionLayers;

    private List<GameObject> _foundGameObjects = new List<GameObject>();
    public List<GameObject> FoundGameObjects => _foundGameObjects;
    private bool _isPlayerDetected = false;

    void Update()
    {
        DetectarObjetos();
    }
    
    private void DetectarObjetos()
    {
        _foundGameObjects.Clear();
        _isPlayerDetected = false;

        // 1. Buscamos todos los colliders en el radio que estén en las capas deseadas.
        Collider2D[] collidersInRadius = Physics2D.OverlapCircleAll(transform.position, radioDeDeteccion, desiredDetectionLayers);

        // 2. Para cada collider, revisamos si está dentro del cono de visión.
        foreach (var col in collidersInRadius)
        {
            if (col.gameObject == this.gameObject) continue;

            Vector2 directionToTarget = (col.transform.position - transform.position).normalized;

            // Asume que tu sprite "mira" hacia ARRIBA. Si mira a la DERECHA, cambia 'transform.up' por 'transform.right'.
            if (Vector2.Angle(transform.up, directionToTarget) < anguloDeVision / 2)
            {
                _foundGameObjects.Add(col.gameObject);
                
                if (col.CompareTag("Player"))
                {
                    _isPlayerDetected = true;
                }
            }
        }
    }

    public List<GameObject> GetAllObjectsByLayer(int layer)
    {
        return _foundGameObjects.Where(obj => obj != null && obj.layer == layer).ToList();
    }

    private void OnDrawGizmosSelected()
    {
        // Si el jugador está detectado, el gizmo es VERDE. Si no, es ROJO.
        Gizmos.color = _isPlayerDetected ? Color.green : Color.red;

        // Asume que el sprite mira hacia ARRIBA. Cambia 'transform.up' si mira a la DERECHA.
        Vector3 forwardDirection = transform.up; 
        Vector3 coneLine1 = Quaternion.AngleAxis(anguloDeVision / 2, Vector3.forward) * forwardDirection * radioDeDeteccion;
        Vector3 coneLine2 = Quaternion.AngleAxis(-anguloDeVision / 2, Vector3.forward) * forwardDirection * radioDeDeteccion;

        // Dibuja las líneas del cono.
        Gizmos.DrawLine(transform.position, transform.position + coneLine1);
        Gizmos.DrawLine(transform.position, transform.position + coneLine2);
        
        // Si se detecta al jugador, dibuja una línea verde directa hacia él.
        if (_isPlayerDetected)
        {
             foreach (var obj in _foundGameObjects)
             {
                 if(obj != null && obj.CompareTag("Player"))
                 {
                     Gizmos.DrawLine(transform.position, obj.transform.position);
                 }
             }
        }
    }
}