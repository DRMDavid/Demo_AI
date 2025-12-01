/*
Este código fue basado en el video de YouTube del canal Alexis Kotsiras:
Enlace: https://youtu.be/3Nl8UPyODgQ?si=UISpmu5yan8PTVtU
Integrantes del Equipo:
Hannin Abarca
Gael Jimenez
David Sanchez
*/
using UnityEngine;

// Componente para crear un efecto de "fantasma" o rastro que se desvanece con el tiempo (comúnmente usado en dash o movimientos rápidos).
public class BossGhost : MonoBehaviour
{
    [Header("Configuración del Desvanecimiento")]
    public float fadeSpeed = 3f; // La velocidad a la que el sprite pierde opacidad (se vuelve transparente).

    private SpriteRenderer sr; // Referencia al componente que dibuja el sprite.
    private Color color; // Variable temporal para manipular el color y la transparencia (Alpha).

    void Start()
    {
        // Se obtiene la referencia del SpriteRenderer adjunto a este objeto.
        sr = GetComponent<SpriteRenderer>();
        
        // Se guarda el color actual del sprite para comenzar a modificarlo.
        color = sr.color;
    }

    void Update()
    {
        // Reduce el valor del canal Alpha (transparencia) multiplicando la velocidad por el tiempo transcurrido.
        // Esto hace que el desvanecimiento sea suave e independiente de los FPS.
        color.a -= fadeSpeed * Time.deltaTime;

        // Aplica el color modificado de vuelta al SpriteRenderer.
        sr.color = color;

        // Si el valor Alpha es menor o igual a 0, significa que el objeto es totalmente invisible.
        if (color.a <= 0)
        {
            // Destruye el objeto fantasma para limpiar la escena y ahorrar memoria.
            Destroy(gameObject);
        }
    }
}