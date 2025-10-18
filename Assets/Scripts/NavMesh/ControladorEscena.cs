// ================================================================
// Archivo: ControladorEscena.cs
// Descripción: Permite reiniciar la escena actual al presionar la tecla 'R'.
// IMPLEMENTADO POR: Gael, David y Steve
// ================================================================

using UnityEngine;
using UnityEngine.SceneManagement; // Permite manipular y recargar escenas

/// <summary>
/// Controla el reinicio de la escena actual mediante una tecla.
/// </summary>
public class ControladorEscena : MonoBehaviour
{
    /// <summary>
    /// Escucha continuamente la entrada del usuario para detectar
    /// si se presiona la tecla 'R' y reinicia la escena actual.
    /// </summary>
    void Update()
    {
        // Detecta si se presiona la tecla 'R'
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Reiniciando la escena...");

            // Obtiene el nombre de la escena activa
            string currentSceneName = SceneManager.GetActiveScene().name;

            // Recarga la escena actual usando su nombre
            SceneManager.LoadScene(currentSceneName);
        }
    }
}
