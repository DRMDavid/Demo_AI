using UnityEngine;
using System.Collections;

public class PowerUp : MonoBehaviour
{
    public enum TipoPowerUp
    {
        DisparoRapido,    // Ametralladora 🌶️
        DashInfinito,     // Energía Ilimitada 🥤
        Escopeta,         // Disparo Triple 🍖
        BalasPerforantes  // Atraviesa Enemigos 🥕
    }

    [Header("Configuración General")]
    public TipoPowerUp tipo;
    public float duracion = 5f;

    [Header("Ajustes Específicos")]
    [Tooltip("Nuevo tiempo entre disparos (ej: 0.05 para ametralladora)")]
    public float nuevaCadencia = 0.05f; 
    
    [Tooltip("Cuántos enemigos atraviesa la bala (solo para BalasPerforantes)")]
    public int cantidadPerforacion = 10;

    // Variables internas
    private SpriteRenderer spriteVisual;
    private Collider2D colisionador;

    private void Awake()
    {
        spriteVisual = GetComponent<SpriteRenderer>();
        colisionador = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Solo el jugador puede recoger el power-up
        if (other.CompareTag("Player"))
        {
            StartCoroutine(ActivarPowerUp(other.gameObject));
        }
    }

    private IEnumerator ActivarPowerUp(GameObject jugador)
    {
        // 1. Ocultamos el objeto visualmente y desactivamos su colisión para que no se pueda recoger dos veces
        spriteVisual.enabled = false;
        colisionador.enabled = false;

        // 2. Buscamos los scripts necesarios en el jugador
        PlayerShooter shooter = jugador.GetComponent<PlayerShooter>();
        PlayerMovimiento movimiento = jugador.GetComponent<PlayerMovimiento>();

        // --- 🟢 LOG DE INICIO ---
        Debug.Log($"🟢 [POWER-UP ACTIVADO] Tipo: {tipo} | Duración: {duracion} segundos.");

        // 3. Activamos el efecto según el tipo seleccionado
        switch (tipo)
        {
            case TipoPowerUp.DisparoRapido:
                if (shooter != null)
                {
                    float original = shooter.fireRate;
                    shooter.ModificarFireRate(nuevaCadencia);
                    
                    yield return new WaitForSeconds(duracion); // Esperamos el tiempo del efecto
                    
                    shooter.ModificarFireRate(original);
                }
                break;

            case TipoPowerUp.DashInfinito:
                if (movimiento != null)
                {
                    // Asumimos que el costo normal es 5. Si tu juego usa otro, cámbialo aquí.
                    movimiento.ModificarCostoDash(0f); 
                    
                    yield return new WaitForSeconds(duracion);
                    
                    movimiento.ModificarCostoDash(5f); 
                }
                break;

            case TipoPowerUp.Escopeta:
                if (shooter != null)
                {
                    shooter.ActivarEscopeta(true);
                    
                    yield return new WaitForSeconds(duracion);
                    
                    shooter.ActivarEscopeta(false);
                }
                break;

            case TipoPowerUp.BalasPerforantes:
                if (shooter != null)
                {
                    // Al activar perforación, el PlayerShooter se encargará de cambiar el color de la bala automáticamente
                    shooter.SetPerforacion(cantidadPerforacion);
                    
                    yield return new WaitForSeconds(duracion);
                    
                    shooter.SetPerforacion(0);
                }
                break;
        }

        // --- 🔴 LOG DE FINALIZACIÓN ---
        Debug.Log($"🔴 [POWER-UP FINALIZADO] El efecto de {tipo} ha terminado.");

        // 4. Destruimos el objeto de la escena definitivamente
        Destroy(gameObject);
    }
}