using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverMenu : MonoBehaviour
{
    public void ReiniciarNivel()
    {
        // Carga la escena anterior (nivel en el que murió el jugador)
        SceneManager.LoadScene("SampleScene"); // ⚠️ Cambia por el nombre real de tu escena principal
    }

    public void SalirDelJuego()
    {
        Application.Quit();
    }
}
