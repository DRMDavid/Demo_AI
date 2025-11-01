// EN Assets/Scripts/Niveles/NavMeshUpdater.cs

using UnityEngine;
using NavMeshPlus.Components;
using UnityEngine.AI; // Necesario para NavMesh

// Este script actuará como un Singleton para gestionar la actualización del NavMesh.
public class NavMeshUpdater : MonoBehaviour
{
    public static NavMeshUpdater Instance;

    [Header("Referencia al NavMesh Surface")]
    // Asigna el componente NavMeshSurface de tu escena aquí.
    [SerializeField] private NavMeshSurface navMeshSurface; 
    
    // Un radio que NavMeshPlus puede usar para optimizar la actualización (aunque la implementación
    // pública básica suele actualizar toda la superficie).
    [SerializeField] private float updateRadius = 5f; 

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (navMeshSurface == null)
        {
            // Fallback: busca el componente NavMeshSurface en la escena si no está asignado
            navMeshSurface = FindObjectOfType<NavMeshSurface>();
            if (navMeshSurface == null)
            {
                Debug.LogError("NavMeshSurface no encontrado en la escena. ¡La IA no se actualizará!");
            }
        }
    }

    /// <summary>
    /// Llama a la función de actualización del NavMesh justo después de que un obstáculo es removido.
    /// </summary>
    /// <param name="destroyedPosition">La posición central del objeto destruido.</param>
    public void RequestNavMeshUpdate(Vector3 destroyedPosition)
    {
        if (navMeshSurface != null && navMeshSurface.navMeshData != null)
        {
            // NavMeshSurface.UpdateNavMesh(NavMeshData data) en NavMeshPlus fuerza una 
            // actualización de la superficie actual, abriendo el camino para los agentes.
            // Para objetos destructibles pequeños, esto es suficiente.
            navMeshSurface.UpdateNavMesh(navMeshSurface.navMeshData); 

            Debug.Log($"NavMesh actualizado en tiempo de ejecución cerca de {destroyedPosition}.");
        }
    }
}