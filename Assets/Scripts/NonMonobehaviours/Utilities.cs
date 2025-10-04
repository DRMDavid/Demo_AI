using System.Collections.Generic;
using UnityEngine;

public class Utilities
{
    // Todas las funciones aquí van a ser estáticas.
    
    public static Vector2 PuntaMenosCola(Vector2 punta, Vector2 cola)
    {
        return punta - cola;
    }
    
    public static float Pitagoras(Vector2 vector)
    {
        return vector.magnitude;
    }
    
    public static bool IsObjectInRange(Vector2 posA, Vector2 posB, float range)
    {
        return (posA - posB).sqrMagnitude < range * range;
    }
    
    // El GetObjectsInCube no tiene un equivalente directo en 2D, pero aquí está una versión adaptada.
    public static List<GameObject> GetObjectsInBox(Vector2 position, Vector2 size, float angle, LayerMask desiredLayers)
    {
        Collider2D[] collidersInBox = Physics2D.OverlapBoxAll(position, size, angle, desiredLayers);

        var objectsInBox = new List<GameObject>();
        foreach (var collider in collidersInBox)
        {
            objectsInBox.Add(collider.gameObject);
        }
        return objectsInBox;
    }
    
    /// <summary>
    /// Requiere que los objetos a detectarse tengan colliders 2D que toquen el círculo descrito por estos parámetros.
    /// </summary>
    public static List<GameObject> GetObjectsInRadius(Vector2 position, float radius, LayerMask desiredLayers)
    {
        Collider2D[] collidersInRadius = Physics2D.OverlapCircleAll(position, radius, desiredLayers);

        var objectsInRadius = new List<GameObject>();
        foreach (var collider in collidersInRadius)
        {
            objectsInRadius.Add(collider.gameObject);
        }
        return objectsInRadius;
    }
    
    // esta es la que se usa para comparar entre Tags
    public static bool CompareString(string a, string b)
    {
        return a == b;
    }
}