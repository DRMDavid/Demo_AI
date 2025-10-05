using UnityEngine;
using UnityEngine.SceneManagement;

/*
Integrantes del equipo:
- Hannin Abarca
- David Sánchez
- Gael Jiménez

Script basado en el tutorial de YouTube:
https://www.youtube.com/watch?v=hQaCjy8mz9I&t=386s
*/

/// <summary>
/// GameOverMenu: Clase que maneja la UI de Game Over.
/// Permite reiniciar el nivel actual o salir del juego.
/// </summary>
public class GameOverMenu : MonoBehaviour
{
    /// <summary>
    /// ReiniciarNivel: Carga la escena del nivel actual para reiniciar el juego.
    /// ⚠️ Cambia "SampleScene".
    /// </summary>
    public void ReiniciarNivel()
    {
        SceneManager.LoadScene("SampleScene");
    }

    /// <summary>
    /// SalirDelJuego: Cierra la aplicación.
    /// En el editor de Unity no hará nada, solo funciona en build final.
    /// </summary>
    public void SalirDelJuego()
    {
        Application.Quit();
    }
}
