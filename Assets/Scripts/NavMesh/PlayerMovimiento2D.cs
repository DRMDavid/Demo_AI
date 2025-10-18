// ================================================================
// Archivo: PlayerMovimiento2D.cs
// Descripción: Controla el movimiento y la vida del jugador en un entorno 2D.
// Permite desplazarse con el teclado y recibir daño.
// IMPLEMENTADO POR: Gael, David y Steve
// ================================================================
using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para reiniciar la escena

[RequireComponent(typeof(Rigidbody2D))] // Asegura que el jugador tenga un Rigidbody2D
public class PlayerMovimiento2D : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [Tooltip("La velocidad a la que se moverá el jugador.")]
    [SerializeField] private float velocidad = 5f;

    [Header("Estadísticas del Jugador")]
    [Tooltip("La cantidad de vidas iniciales del jugador.")]
    [SerializeField] public int vida = 3;

    // --- Componentes y Variables Internas ---
    private Rigidbody2D rb2D;              // Referencia al componente Rigidbody2D
    private Vector2 direccionMovimiento;   // Dirección del movimiento (input del jugador)

    private void Awake()
    {
        // Obtiene la referencia al Rigidbody2D solo una vez al iniciar.
        rb2D = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // --- LECTURA DE INPUT ---
        // Se ejecuta cada fotograma. Ideal para leer entradas del teclado.

        // Input.GetAxisRaw devuelve -1, 0 o 1 (sin suavizado), ideal para respuesta inmediata.
        float movX = Input.GetAxisRaw("Horizontal"); // Teclas A/D o flechas izquierda/derecha
        float movY = Input.GetAxisRaw("Vertical");   // Teclas W/S o flechas arriba/abajo

        // Normaliza el vector para evitar que el movimiento diagonal sea más rápido.
        direccionMovimiento = new Vector2(movX, movY).normalized;
    }

    private void FixedUpdate()
    {
        // --- APLICACIÓN DE FÍSICAS ---
        // Se ejecuta a intervalos fijos, ideal para manipular físicas o movimiento.

        // Actualiza la velocidad del Rigidbody2D en función de la dirección y la velocidad configurada.
        rb2D.linearVelocity = direccionMovimiento * velocidad;
    }

    /// <summary>
    /// Aplica daño al jugador y verifica si ha perdido todas sus vidas.
    /// </summary>
    /// <param name="cantidadDano">Cantidad de vida que se resta.</param>
    public void RecibirDano(int cantidadDano)
    {
        vida -= cantidadDano;
        Debug.Log("¡El jugador ha recibido daño! Vidas restantes: " + vida);

        // Si la vida llega a cero o menos, el jugador es destruido.
        if (vida <= 0)
        {
            Debug.Log("El jugador ha sido derrotado.");
            Destroy(gameObject);

            // 🔹 Opción: Reiniciar automáticamente la escena al morir.
            // SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
