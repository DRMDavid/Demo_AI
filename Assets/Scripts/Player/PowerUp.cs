/*
Este código fue basado en tutoriales sobre cómo crear un sistema de Power-Ups temporales.
Enlace: https://www.youtube.com/watch?v=5C9t3ux6PWA 
Integrantes del Equipo:
Hannin Abarca
Gael Jimenez
David Sanchez
*/
using UnityEngine;
using System.Collections;

// Clase que se adjunta a un objeto de recolección (pickup) para otorgar un efecto temporal al jugador.
public class PowerUp : MonoBehaviour
{
    // Enumeración que define los diferentes tipos de Power-Ups disponibles en el juego.
    public enum TipoPowerUp
    {
        DisparoRapido,    // Ametralladora (Modifica la cadencia de disparo)
        DashInfinito,     // Energía Ilimitada (Modifica el costo de dash a cero)
        Escopeta,         // Disparo Triple (Activa un modo de disparo disperso)
        BalasPerforantes  // Atraviesa Enemigos (Permite a las balas golpear múltiples objetivos)
    }

    [Header("Configuración General")]
    public TipoPowerUp tipo;     // El tipo de Power-Up que este objeto otorgará.
    public float duracion = 5f;  // El tiempo (en segundos) que el efecto estará activo en el jugador.

    [Header("Ajustes Específicos")]
    [Tooltip("Nuevo tiempo entre disparos (ej: 0.05 para ametralladora)")]
    public float nuevaCadencia = 0.05f; 
    
    [Tooltip("Cuántos enemigos atraviesa la bala (solo para BalasPerforantes)")]
    public int cantidadPerforacion = 10;

    // Variables internas para la gestión del objeto en la escena.
    private SpriteRenderer spriteVisual; // Referencia al componente visual.
    private Collider2D colisionador;    // Referencia al colisionador para detectar la recolección.

    private void Awake()
    {
        // Se inicializan las referencias al inicio. Awake se ejecuta antes que Start.
        spriteVisual = GetComponent<SpriteRenderer>();
        colisionador = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Comprueba si el objeto que colisionó tiene la etiqueta "Player".
        if (other.CompareTag("Player"))
        {
            // Inicia la Corrutina que gestionará la duración del efecto.
            StartCoroutine(ActivarPowerUp(other.gameObject));
        }
    }

    // Corrutina principal para la activación, espera y desactivación del Power-Up.
    private IEnumerator ActivarPowerUp(GameObject jugador)
    {
        // 1. Ocultamos el objeto visualmente y desactivamos su colisión para que no se pueda recoger dos veces
        // ni interactuar con él mientras el efecto está activo.
        spriteVisual.enabled = false;
        colisionador.enabled = false;

        // 2. Buscamos los scripts necesarios en el jugador para aplicar las modificaciones
        PlayerShooter shooter = jugador.GetComponent<PlayerShooter>();
        PlayerMovimiento movimiento = jugador.GetComponent<PlayerMovimiento>();

        // --- LOG DE INICIO ---
        Debug.Log($"[POWER-UP ACTIVADO] Tipo: {tipo} | Duración: {duracion} segundos.");

        // 3. Activamos el efecto según el tipo seleccionado
        switch (tipo)
        {
            case TipoPowerUp.DisparoRapido:
                // Controla la cadencia de disparo (fire rate)
                if (shooter != null)
                {
                    float original = shooter.fireRate;
                    shooter.ModificarFireRate(nuevaCadencia); // Aplica la nueva cadencia rápida
                    
                    yield return new WaitForSeconds(duracion); // Esperamos el tiempo del efecto
                    
                    shooter.ModificarFireRate(original); // Restaura la cadencia original
                }
                break;

            case TipoPowerUp.DashInfinito:
                // Modifica el costo de energía del dash
                if (movimiento != null)
                {
                    // Asumimos que el costo normal es 5.
                    movimiento.ModificarCostoDash(0f); // Dash sin costo (infinito)
                    
                    yield return new WaitForSeconds(duracion);
                    
                    movimiento.ModificarCostoDash(5f); // Restaura el costo normal
                }
                break;

            case TipoPowerUp.Escopeta:
                // Activa el modo de disparo de escopeta/triple
                if (shooter != null)
                {
                    shooter.ActivarEscopeta(true);
                    
                    yield return new WaitForSeconds(duracion);
                    
                    shooter.ActivarEscopeta(false); // Desactiva el modo escopeta
                }
                break;

            case TipoPowerUp.BalasPerforantes:
                // Establece cuántos enemigos puede penetrar la bala
                if (shooter != null)
                {
                    // El PlayerShooter se encarga internamente de la lógica de la bala y el cambio de color.
                    shooter.SetPerforacion(cantidadPerforacion);
                    
                    yield return new WaitForSeconds(duracion);
                    
                    shooter.SetPerforacion(0); // Restaura la perforación a cero (una sola bala)
                }
                break;
        }

        // ---  LOG DE FINALIZACIÓN ---
        Debug.Log($" [POWER-UP FINALIZADO] El efecto de {tipo} ha terminado.");

        // 4. Destruimos el objeto de la escena definitivamente
        Destroy(gameObject);
    }
}