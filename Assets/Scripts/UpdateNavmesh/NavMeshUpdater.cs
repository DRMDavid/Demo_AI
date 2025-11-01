// EN Assets/Scripts/Niveles/NavMeshUpdater.cs

using UnityEngine;
using NavMeshPlus.Components;
using UnityEngine.AI; // Necesario para NavMesh

/*
Integrantes del equipo:
- Hannin Abarca
- David Sánchez
- Gael Jiménez

Descripción:
Este script gestiona la actualización del NavMesh en tiempo de ejecución.
Funciona como un Singleton para que cualquier script pueda solicitar
una actualización del NavMesh cuando se destruyen obstáculos u objetos
en la escena.

Referencias:
- https://www.youtube.com/watch?v=6wfsDdgjEKo
- https://www.youtube.com/watch?v=mV-Uh_FEBn4
*/

/// <summary>
/// Clase que gestiona la actualización de la superficie NavMesh en tiempo real.
/// </summary>
public class NavMeshUpdater : MonoBehaviour
{
    /// <summary>
    /// Instancia estática del Singleton.
    /// Permite acceso global al gestor de NavMesh.
    /// </summary>
    public static NavMeshUpdater Instance;

    [Header("Referencia al NavMesh Surface")]
    /// <summary>
    /// Componente NavMeshSurface de la escena que se actualizará.
    /// Se puede asignar desde el Inspector.
    /// </summary>
    [SerializeField] private NavMeshSurface navMeshSurface; 
    
    /// <summary>
    /// Radio de actualización para optimizar el recalculo del NavMesh.
    /// En la implementación básica se actualiza toda la superficie.
    /// </summary>
    [SerializeField] private float updateRadius = 5f; 

    /// <summary>
    /// Inicialización del Singleton y comprobación del NavMeshSurface.
    /// </summary>
    private void Awake()
    {
        // Configura la instancia del Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Si no se asignó NavMeshSurface desde el Inspector, busca uno en la escena
        if (navMeshSurface == null)
        {
            navMeshSurface = FindObjectOfType<NavMeshSurface>();
            if (navMeshSurface == null)
            {
                Debug.LogError("NavMeshSurface no encontrado en la escena. ¡La IA no se actualizará!");
            }
        }
    }

    /// <summary>
    /// Solicita la actualización del NavMesh en tiempo de ejecución.
    /// Se llama típicamente cuando un objeto destructible es removido.
    /// </summary>
    /// <param name="destroyedPosition">Posición del objeto destruido (informativa para debug).</param>
    public void RequestNavMeshUpdate(Vector3 destroyedPosition)
    {
        if (navMeshSurface != null && navMeshSurface.navMeshData != null)
        {
            // Fuerza la actualización del NavMesh actual
            navMeshSurface.UpdateNavMesh(navMeshSurface.navMeshData); 

            // Mensaje de depuración para confirmar la actualización
            Debug.Log($"NavMesh actualizado en tiempo de ejecución cerca de {destroyedPosition}.");
        }
    }
}
