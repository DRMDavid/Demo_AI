/*
Este código fue basado en tutoriales sobre cómo crear Efectos Visuales (VFX) 2D simples.
Enlace: https://www.youtube.com/watch?v=_z68_OoC_0o 
Integrantes del Equipo:
Hannin Abarca
Gael Jimenez
David Sanchez
*/
using UnityEngine;

// Clase que gestiona el desvanecimiento de un rastro visual (fantasma o "afterimage").
// Este componente reduce la transparencia (Alpha) del sprite hasta que desaparece, momento en el cual se autodestruye.
public class Ghost : MonoBehaviour
{
    private SpriteRenderer sr; // Referencia al renderizador para manipular el color.
    private float alpha;       // Variable que guarda el valor actual de transparencia (entre 0 y 1).
    [Header("Configuración del Desvanecimiento")]
    public float fadeSpeed = 5f; // Velocidad a la que se reduce la transparencia por segundo.

    void Start()
    {
        // Obtiene el componente SpriteRenderer adjunto al objeto.
        sr = GetComponent<SpriteRenderer>();
        
        // Inicializa la transparencia en 1 (totalmente visible) al inicio.
        alpha = 1f;
    }

    void Update()
    {
        // Reduce el valor 'alpha' gradualmente.
        // Multiplicar por Time.deltaTime garantiza que la velocidad de desvanecimiento sea consistente.
        alpha -= fadeSpeed * Time.deltaTime;

        // Se comprueba si el SpriteRenderer existe antes de intentar manipularlo.
        if (sr != null)
        {
            // 1. Obtiene el color actual del sprite.
            Color c = sr.color;
            
            // 2. Asigna el nuevo valor de 'alpha' al componente de transparencia del color.
            c.a = alpha;
            
            // 3. Vuelve a asignar el color modificado al SpriteRenderer.
            sr.color = c;
        }

        // Si la transparencia llega a 0 o menos, el fantasma es invisible.
        if (alpha <= 0f)
        {
            // Destruye el objeto para liberar memoria de la escena.
            Destroy(gameObject);
        }
    }
}
