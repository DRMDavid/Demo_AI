// ControladorEscena.cs
using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para manejar escenas

public class ControladorEscena : MonoBehaviour
{
    /// <summary>
    /// Requisito f): Tener una tecla para reiniciar la escena.
    /// </summary>
    void Update()
    {
        // Si el usuario presiona la tecla 'R'
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Reiniciando la escena...");
            
            // Obtiene el nombre de la escena actual
            string currentSceneName = SceneManager.GetActiveScene().name;
            
            // Vuelve a cargar la escena actual
            SceneManager.LoadScene(currentSceneName);
        }
    }
}