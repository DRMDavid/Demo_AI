// PlayerMovimiento2D.cs
using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para reiniciar la escena

// Asegura que el GameObject tenga siempre un componente Rigidbody2D.
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovimiento2D : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [Tooltip("La velocidad a la que se moverá el jugador.")]
    [SerializeField] private float velocidad = 5f;

    [Header("Estadísticas del Jugador")]
    [Tooltip("La cantidad de vidas iniciales del jugador.")]
    [SerializeField] public int vida = 3;

    // --- Componentes y Variables Internas ---
    private Rigidbody2D rb2D;
    private Vector2 direccionMovimiento; // Guarda la dirección del input (ej: arriba, izquierda)

    private void Awake()
    {
        // Obtener la referencia al componente Rigidbody2D una sola vez.
        rb2D = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // --- LECTURA DE INPUT ---
        // Se ejecuta en cada fotograma. Es el mejor lugar para leer inputs.

        // Input.GetAxisRaw devuelve -1, 0 o 1 (sin suavizado), ideal para respuesta inmediata.
        float movX = Input.GetAxisRaw("Horizontal"); // Teclas A/D o flechas izquierda/derecha
        float movY = Input.GetAxisRaw("Vertical");   // Teclas W/S o flechas arriba/abajo

        // Normalizamos el vector para que el movimiento diagonal no sea más rápido.
        direccionMovimiento = new Vector2(movX, movY).normalized;
    }

    private void FixedUpdate()
    {
        // --- APLICACIÓN DE FÍSICAS ---
        // Se ejecuta a un ritmo fijo. Es el mejor lugar para aplicar fuerzas o cambiar la velocidad.

        // Movemos el personaje cambiando directamente su velocidad.
        rb2D.linearVelocity = direccionMovimiento * velocidad;
    }

    /// <summary>
    /// Reduce la vida del jugador y comprueba si ha sido derrotado.
    /// </summary>
    /// <param name="cantidadDano">La cantidad de vida a restar.</param>
    public void RecibirDano(int cantidadDano)
    {
        vida -= cantidadDano;
        Debug.Log("¡El jugador ha recibido daño! Vidas restantes: " + vida);

        if (vida <= 0)
        {
            Debug.Log("El jugador ha sido derrotado.");
            // Destruye el objeto del jugador.
            Destroy(gameObject);

            // Opcional: Si quieres reiniciar la escena automáticamente al morir:
            // SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}