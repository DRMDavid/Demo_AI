/*
Este código fue basado en tutoriales sobre cómo crear Efectos Visuales (VFX) 2D simples.
Enlace: https://www.youtube.com/watch?v=_z68_OoC_0o 
Integrantes del Equipo:
Hannin Abarca
Gael Jimenez
David Sanchez
*/
using UnityEngine;

// Clase que gestiona un efecto de impacto cuerpo a cuerpo (Melee) o un flash de golpe.
// El efecto crece y se desvanece simultáneamente hasta su autodestrucción.
public class BossMeleeImpact : MonoBehaviour
{
    [Header("Configuración del Efecto")]
    public float growSpeed = 4f; // Velocidad a la que el objeto aumenta de tamaño (escala).
    public float fadeSpeed = 6f; // Velocidad a la que el objeto pierde opacidad (canal Alpha).

    private SpriteRenderer sr; // Referencia al renderizador del sprite.
    private Color color; // Almacena el color actual para manipular la transparencia.

    void Start()
    {
        // Obtiene el componente SpriteRenderer para manipular la apariencia del efecto.
        sr = GetComponent<SpriteRenderer>();
        // Guarda el color inicial para empezar el proceso de desvanecimiento desde el valor Alpha actual.
        color = sr.color;
    }

    void Update()
    {
        // 1. Efecto de Crecimiento: Incrementa la escala del objeto de forma uniforme.
        // Vector3.one es (1, 1, 1). La multiplicación por Time.deltaTime asegura que el crecimiento es fluido.
        transform.localScale += Vector3.one * growSpeed * Time.deltaTime;

        // 2. Efecto de Desvanecimiento: Reduce gradualmente el canal Alpha.
        color.a -= fadeSpeed * Time.deltaTime;
        
        // Aplica el nuevo color con la transparencia reducida al SpriteRenderer.
        sr.color = color;

        // Si la opacidad (alpha) se agota (llega a 0 o menos), se destruye el objeto.
        if (color.a <= 0)
        {
            // Elimina el objeto del impacto de la escena para liberar recursos.
            Destroy(gameObject);
        }
    }
}