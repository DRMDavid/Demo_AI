/*
Este código fue basado en el video de YouTube del canal Coco Code:
Enlace: https://youtu.be/K4uOjb5p3Io?si=WcXB9Ts16Knlnhx
Integrantes del Equipo:
Hannin Abarca
Gael Jimenez
David Sanchez
*/
using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para cargar escenas (niveles) al reiniciar.

// Componente que maneja la lógica de los botones en la pantalla de Game Over.
public class GameOverMenu : MonoBehaviour
{
    // Función que se enlaza a un botón para reiniciar el nivel.
    public void ReiniciarNivel()
    {
        // Carga la escena del juego (el nivel en el que murió el jugador).
        SceneManager.LoadScene("SampleScene");
    }

    // Función que se enlaza a un botón para salir.
    public void SalirDelJuego()
    {
        // Finaliza la ejecución de la aplicación (solo funciona en compilaciones, no en el editor de Unity).
        Application.Quit();
    }
}