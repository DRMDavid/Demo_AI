/*
Este código fue basado en tutoriales sobre cómo crear Efectos Visuales (VFX) 2D simples.
Enlace: https://www.youtube.com/watch?v=_z68_OoC_0o 
Integrantes del Equipo:
Hannin Abarca
Gael Jimenez
David Sanchez
*/
using UnityEngine;

// Clase que gestiona un efecto de ráfaga o explosión de energía definitiva ('Ultimate Burst').
// El efecto comienza siendo más brillante de lo normal, se expande rápidamente y luego se desvanece.
public class BossUltimateBurst : MonoBehaviour
{
    [Header("Configuración del Efecto")]
    public float growSpeed = 8f; // Velocidad a la que el objeto aumenta de tamaño (expansión).
    public float fadeSpeed = 5f; // Velocidad a la que el objeto pierde opacidad (transparencia).

    private SpriteRenderer sr; // Referencia al renderizador del sprite.
    private Color color; // Almacena el color actual, usado para manipular el brillo y la transparencia.

    void Start()
    {
        // Obtiene el componente SpriteRenderer.
        sr = GetComponent<SpriteRenderer>();
        
        // 💡 Ajuste de Brillo Inicial: 
        // Multiplica el color base por 1.5. Esto permite que el efecto sea más brillante (emite luz) 
        // de lo que permite el color normal (siempre que el material del sprite lo soporte).
        color = sr.color * 1.5f; 
    }

    void Update()
    {
        // 1. Efecto de Expansión (Crecimiento): Aumenta la escala del objeto en todos los ejes rápidamente.
        transform.localScale += Vector3.one * growSpeed * Time.deltaTime;

        // 2. Efecto de Desvanecimiento: Reduce el canal Alpha (transparencia) gradualmente.
        color.a -= fadeSpeed * Time.deltaTime;
        
        // Aplica el color modificado con la transparencia reducida.
        sr.color = color;

        // Si la opacidad (alpha) se agota (llega a 0 o menos), se destruye el objeto.
        if (color.a <= 0)
        {
            // Elimina el objeto de la ráfaga de la escena.
            Destroy(gameObject);
        }
    }
}

