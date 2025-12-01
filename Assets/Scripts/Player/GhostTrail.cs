/*
Este código fue basado en tutoriales sobre cómo crear Efectos Visuales (VFX) 2D simples.
Enlace: https://www.youtube.com/watch?v=_z68_OoC_0o 
Integrantes del Equipo:
Hannin Abarca
Gael Jimenez
David Sanchez
*/
using UnityEngine;

// Clase que se adjunta al jugador o al objeto que genera el rastro visual (Trail).
// Su función es instanciar repetidamente un prefab de 'fantasma' (que luego se desvanece)
// para crear un efecto de movimiento rápido o dash.
public class GhostTrail : MonoBehaviour
{
    [Header("Configuración del Rastro")]
    public GameObject ghostPrefab; // El prefab que contiene el script 'Ghost' y el SpriteRenderer.
    public float ghostDelay = 0.05f; // Tiempo (en segundos) entre la creación de cada fantasma.
    private float timer; // Contador interno para controlar cuándo se puede crear el siguiente fantasma.

    [Header("Apariencia")]
    public Color[] ghostColors; // Arreglo de colores opcionales para aplicar a los fantasmas (ej. rojo para daño, azul para velocidad).

    private SpriteRenderer playerSR; // Referencia al SpriteRenderer del objeto que lleva este script (el jugador).

    void Start()
    {
        // Obtiene la referencia del SpriteRenderer del objeto actual para poder copiar su apariencia.
        playerSR = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Reduce el temporizador en cada frame. Esto es un "Cooldown" que solo se usa si la función SpawnGhost
        // es llamada repetidamente (ej. en una corrutina).
        timer -= Time.deltaTime;
    }

    // Llamar esta función cuando hagas dash (o durante el tiempo que deseas que aparezca el rastro).
    public void SpawnGhost()
    {
        // Si el temporizador aún es positivo, significa que el retraso (ghostDelay) no ha terminado.
        // Se sale de la función para evitar crear demasiados fantasmas.
        if (timer > 0) return;

        // Reinicia el temporizador para establecer el retraso antes del próximo fantasma.
        timer = ghostDelay;

        // 1. Instancia el prefab del fantasma en la posición actual del objeto (jugador/boss).
        GameObject ghost = Instantiate(ghostPrefab, transform.position, Quaternion.identity);

        // Obtiene el SpriteRenderer del fantasma recién creado.
        SpriteRenderer ghostSR = ghost.GetComponent<SpriteRenderer>();
        
        // 2. Copia la apariencia del jugador/boss al fantasma:
        ghostSR.sprite = playerSR.sprite;        // Copia el sprite actual (importante si hay animación).
        ghostSR.flipX = playerSR.flipX;          // Copia la orientación (si está volteado).
        
        // 3. Controla la capa de dibujo:
        ghostSR.sortingLayerID = playerSR.sortingLayerID;
        // Mueve el fantasma a una capa inferior (detrás del jugador) para que no lo tape.
        ghostSR.sortingOrder = playerSR.sortingOrder - 1; 

        // 4. Asigna el color:
        if (ghostColors.Length > 0)
        {
            // Asigna un color aleatorio de la lista si hay colores definidos.
            ghostSR.color = ghostColors[Random.Range(0, ghostColors.Length)];
        }
        else
        {
            // Si no hay colores especiales definidos, usa el color base del jugador.
            ghostSR.color = playerSR.color;
        }
    }
}