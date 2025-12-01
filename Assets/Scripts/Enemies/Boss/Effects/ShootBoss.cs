/*
Este código fue basado en tutoriales sobre cómo crear Efectos Visuales (VFX) 2D simples.
Enlace: https://www.youtube.com/watch?v=_z68_OoC_0o 
Integrantes del Equipo:
Hannin Abarca
Gael Jimenez
David Sanchez
*/
using UnityEngine;

// Clase que gestiona un efecto de destello de disparo ('Shoot Flash') o una pequeña explosión.
// El efecto se encoge y se desvanece simultáneamente hasta su autodestrucción.
public class BossShootFlash : MonoBehaviour
{
    [Header("Configuración del Efecto")]
    public float shrinkSpeed = 4f; // Velocidad a la que el objeto se reduce de tamaño (contrae).
    public float fadeSpeed = 4f;   // Velocidad a la que el objeto pierde opacidad (canal Alpha).

    private SpriteRenderer sr; // Referencia al renderizador del sprite.
    private Color color; // Almacena el color actual para manipular la transparencia.

    void Start()
    {
        // Obtiene el componente SpriteRenderer.
        sr = GetComponent<SpriteRenderer>();
        // Guarda el color inicial para empezar el proceso de desvanecimiento desde el valor Alpha actual.
        color = sr.color;
    }

    void Update()
    {
        // 1. Efecto de Contracción: Disminuye la escala del objeto en todos los ejes (X, Y, Z).
        // Se usa el operador '-=' para restar el tamaño gradualmente.
        transform.localScale -= Vector3.one * shrinkSpeed * Time.deltaTime;

        // 2. Efecto de Desvanecimiento: Reduce gradualmente el canal Alpha (transparencia).
        color.a -= fadeSpeed * Time.deltaTime;
        
        // Aplica el nuevo color con la transparencia reducida.
        sr.color = color;

        // Si la opacidad (alpha) se agota (llega a 0 o menos), se destruye el objeto.
        if (color.a <= 0)
        {
            // Elimina el objeto del destello de la escena para liberar recursos.
            Destroy(gameObject);
        }
    }
}
