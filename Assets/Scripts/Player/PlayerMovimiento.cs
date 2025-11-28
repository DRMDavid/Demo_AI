/*******************************************************
 * NOMBRE DEL ARCHIVO: PlayerMovimiento.cs
 * AUTOR ORIGINAL: Gianny Dantas (Curso Udemy)
 * MODIFICADO Y AMPLIADO POR: Gael, david, Steve
 * CURSO: Aprende a crear un videojuego de Acción 2D con Unity - Gianny Dantas (Udemy)
 * FUENTE: https://www.udemy.com/course/aprende-a-crear-un-videojuego-de-accion-2d-con-unity/
 * 
 * DESCRIPCIÓN:
 * Controla el movimiento del jugador y la mecánica del "dash".
 * Este script permite desplazarse, girar la dirección del sprite,
 * realizar un dash temporal y manejar el consumo/regeneración de energía.
 * 
 * SECCIONES ORIGINALES:
 * - Movimiento y rotación del jugador.
 * - Sistema básico de dash.
 * - Control de transparencia durante el dash.
 * 
 * APORTACIONES PROPIAS:
 * - Sistema de gasto de energía (costoDash).
 * - Regeneración automática de energía con pausa temporal tras usar dash.
 * - Optimizaciones menores en organización del código.
 * 
 * FECHA: 05/10
 *******************************************************/

using System.Collections;
using UnityEngine;

public class PlayerMovimiento : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float velocidadMovimiento; // Velocidad base del jugador.

    [Header("Dash")]
    [SerializeField] private float velocidadDash;       // Velocidad mientras realiza dash.
    [SerializeField] private float tiempoDash;          // Duración del dash.
    [SerializeField] private float transparencia;       // Transparencia del sprite durante dash.
    [SerializeField] private float costoDash = 5f;      // 💡 Costo de energía por dash (implementación propia).

    [Header("Energía")]
    [SerializeField] private ConfiguracionPlayer configPlayer; // Referencia a la configuración del jugador.
    [SerializeField] private float regeneracionPorSegundo = 0.5f; // 💡 Regeneración pasiva de energía por segundo.

    private Rigidbody2D rb2D;
    private PlayerAcciones acciones;
    private SpriteRenderer spriteRenderer;

    private bool usandoDash;       // Controla si el jugador está actualmente en dash.
    private float velocidadActual; // Guarda la velocidad actual (normal o dash).
    private Vector2 direccionMovimiento; // Dirección del movimiento del jugador.

    private bool puedeRegenerar = true;      // 💡 Control para pausar regeneración tras usar dash.
    private Coroutine regenDelayCoroutine;   // Guarda la corrutina de pausa de regeneración.

    private void Awake()
    {
        // Inicializa componentes requeridos.
        acciones = new PlayerAcciones();
        rb2D = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        velocidadActual = velocidadMovimiento;

        // Vincula el input de dash al método Dash().
        acciones.Movimiento.Dash.performed += ctx => Dash();
    }

    private void Update()
    {
        CapturarInput();
        RotarPlayer();
        MoverPlayer();

        // 💡 Regenera energía si está permitido.
        if (puedeRegenerar)
        {
            RegenerarEnergia();
        }
    }

    private void FixedUpdate()
    {
        // Movimiento físico del jugador (más estable en FixedUpdate).
        MoverPlayer();
    }

    /// <summary>
    /// Aplica el movimiento al jugador usando Rigidbody2D.
    /// </summary>
    private void MoverPlayer()
    {
        rb2D.MovePosition(rb2D.position +
            direccionMovimiento * (velocidadActual * Time.fixedDeltaTime));
    }

    /// <summary>
    /// Ejecuta la acción de dash si hay energía suficiente.
    /// </summary>
    private void Dash()
    {
        // Si ya está usando dash o no tiene suficiente energía, no hace nada.
        if (usandoDash || configPlayer.Energia < costoDash)
        {
            return;
        }

        // 💡 Resta energía por dash.
        configPlayer.Energia -= costoDash;
        if (configPlayer.Energia < 0) configPlayer.Energia = 0;

        // 💡 Detiene regeneración por 2 segundos.
        if (regenDelayCoroutine != null) StopCoroutine(regenDelayCoroutine);
        regenDelayCoroutine = StartCoroutine(PausarRegeneracion());

        usandoDash = true;
        StartCoroutine(IEDash());
    }

    /// <summary>
    /// Corrutina que gestiona la duración del dash y sus efectos visuales.
    /// </summary>
    private IEnumerator IEDash()
    {
        velocidadActual = velocidadDash;
        ModificarSpriteRenderer(transparencia); // Efecto de transparencia durante el dash (original del curso).
        yield return new WaitForSeconds(tiempoDash);
        ModificarSpriteRenderer(1f); // Restaura la opacidad.
        velocidadActual = velocidadMovimiento;
        usandoDash = false;
    }

    /// <summary>
    /// Modifica la transparencia del sprite del jugador.
    /// </summary>
    private void ModificarSpriteRenderer(float valor)
    {
        Color color = spriteRenderer.color;
        color = new Color(color.r, color.g, color.b, valor);
        spriteRenderer.color = color;
    }

    /// <summary>
    /// Cambia la orientación del sprite según la dirección de movimiento.
    /// </summary>
    private void RotarPlayer()
    {
        if (direccionMovimiento.x >= 0.1f)
        {
            spriteRenderer.flipX = false;
        }
        else if (direccionMovimiento.x < 0f)
        {
            spriteRenderer.flipX = true;
        }
    }

    /// <summary>
    /// Captura la dirección del movimiento desde el input del jugador.
    /// </summary>
    private void CapturarInput()
    {
        direccionMovimiento = acciones.Movimiento.Mover.ReadValue<Vector2>().normalized;
    }

    private void OnEnable()
    {
        acciones.Enable();
    }

    private void OnDisable()
    {
        acciones.Disable();
    }

    /// <summary>
    /// 💡 Regenera energía gradualmente mientras no esté pausada.
    /// </summary>
    private void RegenerarEnergia()
    {
        if (configPlayer.Energia < configPlayer.EnergiaMax)
        {
            configPlayer.Energia += regeneracionPorSegundo * Time.deltaTime;
            if (configPlayer.Energia > configPlayer.EnergiaMax)
                configPlayer.Energia = configPlayer.EnergiaMax;
        }
    }

    /// <summary>
    /// 💡 Detiene la regeneración de energía por un periodo (ej. tras dash).
    /// </summary>
    private IEnumerator PausarRegeneracion()
    {
        puedeRegenerar = false;
        yield return new WaitForSeconds(2f);
        puedeRegenerar = true;
    }
    
    public void ModificarCostoDash(float nuevoCosto)
    {
        costoDash = nuevoCosto;
    }
}
