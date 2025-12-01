/*
Este código fue basado en tutoriales sobre cómo crear Efectos Visuales (VFX) 2D simples.
Enlace: https://www.youtube.com/watch?v=_z68_OoC_0o 
Integrantes del Equipo:
Hannin Abarca
Gael Jimenez
David Sanchez
*/
using UnityEngine;

// Clase que gestiona el efecto visual de aura o energía que pulsa (crece y se encoge rítmicamente)
// para indicar que el jefe está cargando una habilidad o ataque.
public class BossChargeAura : MonoBehaviour
{
    [Header("Configuración de la Pulsación")]
    public float pulsateSpeed = 3f;   // Velocidad o frecuencia del pulso (cuántas veces pulsa por segundo).
    public float pulsateAmount = 0.15f; // Cantidad máxima que el objeto crecerá o se encogerá desde la escala base.

    private float baseScale; // Almacena la escala original del objeto en el eje X/Y.
    private SpriteRenderer sr; // Referencia al renderizador (aunque no se usa en Update, se mantiene por consistencia).

    void Start()
    {
        // Se obtiene la referencia del SpriteRenderer (manteniendo la estructura de los scripts anteriores).
        sr = GetComponent<SpriteRenderer>(); 
        
        // Se guarda la escala inicial del objeto, que servirá como el punto medio del pulso.
        baseScale = transform.localScale.x;
    }

    void Update()
    {
        // 💡 Lógica de Pulsación (Onda Sinusoidal):
        // 1. Time.time: Tiempo total desde que inició el juego (crece continuamente).
        // 2. Multiplicado por pulsateSpeed: Controla la rapidez de la oscilación.
        // 3. Mathf.Sin(...): Devuelve un valor que oscila constantemente entre -1 y 1.
        // 4. Multiplicado por pulsateAmount: Limita la amplitud del pulso (cuánto se estira/encoge).
        float s = baseScale + Mathf.Sin(Time.time * pulsateSpeed) * pulsateAmount;
        
        // Aplica la nueva escala calculada a los ejes X e Y, manteniendo el Z en 1 (para 2D).
        transform.localScale = new Vector3(s, s, 1);
    }
}
