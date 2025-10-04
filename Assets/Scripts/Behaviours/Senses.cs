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
    [Range(0, 360)]
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
        
        Collider2D[] collidersInRadius = Physics2D.OverlapCircleAll(transform.position, radioDeDeteccion, desiredDetectionLayers);

        foreach (var col in collidersInRadius)
        {
            if (col.gameObject == this.gameObject) continue;

            Vector2 directionToTarget = ((Vector2)col.transform.position - (Vector2)transform.position).normalized;
            
            // Usamos (Vector2)transform.up para asegurarnos de que la comparación es 2D
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

    // --- MÉTODO MODIFICADO ---
    // private void OnDrawGizmosSelected()
    // {
    //     // --- Lógica de Dibujo con Vector2 ---
    //     
    //     // 1. Definimos nuestros puntos y direcciones usando Vector2
    //     Vector2 center = transform.position; // Unity convierte V3 a V2 automáticamente
    //     Vector2 forwardDirection = transform.up; // También lo convierte
    //
    //     // 2. Calculamos los vectores del cono. La rotación es más fácil con Quaternions, 
    //     // pero el resultado lo convertimos de vuelta a Vector2.
    //     float halfAngle = anguloDeVision / 2;
    //     Vector2 line1 = (Quaternion.Euler(0, 0, halfAngle) * forwardDirection).normalized * radioDeDeteccion;
    //     Vector2 line2 = (Quaternion.Euler(0, 0, -halfAngle) * forwardDirection).normalized * radioDeDeteccion;
    //
    //     // --- Dibujo Final (La parte inevitable) ---
    //     
    //     Gizmos.color = _isPlayerDetected ? Color.green : Color.red;
    //
    //     // 3. Al llamar a Gizmos.DrawLine, Unity convierte nuestros Vector2 (center, line1, etc.)
    //     // a Vector3 de forma automática y silenciosa.
    //     Gizmos.DrawLine(center, center + line1);
    //     Gizmos.DrawLine(center, center + line2);
    //     
    //     if (_isPlayerDetected)
    //     {
    //          foreach (var obj in _foundGameObjects)
    //          {
    //              if(obj != null && obj.CompareTag("Player"))
    //              {
    //                  // Aquí también, Unity convierte la posición del objeto (V3) y el centro (V2)
    //                  // para que la función los acepte.
    //                  Gizmos.DrawLine(center, obj.transform.position);
    //              }
    //          }
    //     }
    // }
}