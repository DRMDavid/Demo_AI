/*******************************************************
 * NOMBRE DEL ARCHIVO: PlayerMovimiento.cs
 * AUTOR ORIGINAL: Gianny Dantas (Curso Udemy)
 * MODIFICADO POR: Hannin Abarca, Gael Jimenez, David Sanchez
 * * DESCRIPCIÓN:
 * Controla el movimiento básico, el sistema de Dash con efecto "fantasma" (Afterimage)
 * y gestiona la energía del jugador.
 * Incluye correcciones para evitar la duplicación visual del arma durante el Dash.
 * * REFERENCIAS Y FUENTES:
 * - Movimiento Base: https://www.udemy.com/course/aprende-a-crear-un-videojuego-de-accion-2d-con-unity/
 * - Efecto Dash (Hollow Knight Style): https://www.youtube.com/watch?v=kYmJ4U2Fv-Y
 *******************************************************/

using System.Collections;
using UnityEngine;

public class PlayerMovimiento : MonoBehaviour
{
    // ================================================================
    // CONFIGURACIÓN DE MOVIMIENTO
    // ================================================================
    [Header("Configuración Base")]
    [Tooltip("Velocidad estándar de movimiento al caminar.")]
    [SerializeField] private float velocidadMovimiento;

    [Header("Configuración del Dash")]
    [Tooltip("Velocidad explosiva durante el deslizamiento.")]
    [SerializeField] private float velocidadDash;
    [Tooltip("Duración en segundos del deslizamiento.")]
    [SerializeField] private float tiempoDash;
    [Tooltip("Opacidad del sprite durante el dash (0 a 1).")]
    [SerializeField] private float transparencia;
    [Tooltip("Costo de energía por cada uso.")]
    [SerializeField] private float costoDash = 5f;

    [Header("Sistema de Energía")]
    [SerializeField] private ConfiguracionPlayer configPlayer;
    [SerializeField] private float regeneracionPorSegundo = 0.5f;

    [Header("Efectos Visuales (Ghost Trail)")]
    [SerializeField] private GameObject ghostPrefab;
    [SerializeField] private int ghostBurstCount = 3;
    [SerializeField] private float ghostBurstInterval = 0.03f;
    [SerializeField] private float stretchScaleX = 1.4f;
    [SerializeField] private float stretchScaleY = 0.75f;
    
    // ================================================================
    // CORRECCIÓN BUG VISUAL DEL ARMA
    // ================================================================
    [Header("Corrección Visual Arma")]
    [Tooltip("Arrastra aquí el SpriteRenderer del arma (hijo del Player) para ocultarla durante el Dash.")]
    [SerializeField] private SpriteRenderer weaponRenderer; 

    // VARIABLES DE CONTROL INTERNO
    private float multiplicadorVelocidad = 1.0f; // Usado por estados como Hielo
    private Rigidbody2D rb2D;
    private PlayerAcciones acciones;
    private SpriteRenderer spriteRenderer; // Referencia al cuerpo del player

    private bool usandoDash;
    private float velocidadActual;
    private Vector2 direccionMovimiento;

    private bool puedeRegenerar = true;
    private Coroutine regenDelayCoroutine;

    private void Awake()
    {
        // Inicialización de inputs y componentes físicos
        acciones = new PlayerAcciones();
        rb2D = GetComponent<Rigidbody2D>();

        // Busca el renderer en los hijos (generalmente el cuerpo)
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        velocidadActual = velocidadMovimiento;
        // Suscribirse al evento de Dash del Input System
        acciones.Movimiento.Dash.performed += ctx => Dash();
    }

    private void Update()
    {
        // Lectura de inputs frame a frame
        CapturarInput();
        RotarPlayer();

        // Regeneración pasiva de energía
        if (puedeRegenerar)
            RegenerarEnergia();
    }

    private void FixedUpdate()
    {
        // Aplicación de física (movimiento) en intervalos fijos
        MoverPlayer();
    }

    /// <summary>
    /// Mueve el Rigidbody2D considerando la velocidad base y modificadores externos (Veneno/Hielo).
    /// </summary>
    private void MoverPlayer()
    {
        // Calculamos velocidad final: (Base o Dash) * Factor Externo (ej. 0.5 si está congelado)
        float velocidadFinal = velocidadActual * multiplicadorVelocidad;
        rb2D.MovePosition(rb2D.position + direccionMovimiento * (velocidadFinal * Time.fixedDeltaTime));
    }

    /// <summary>
    /// Permite a scripts externos (como PlayerStatusManager) alterar la velocidad.
    /// </summary>
    public void SetMultiplicadorVelocidad(float valor)
    {
        multiplicadorVelocidad = valor;
        Debug.Log($"[PlayerMovimiento] Velocidad modificada a x{valor}");
    }

    /// <summary>
    /// Inicia la lógica del Dash si hay energía suficiente.
    /// </summary>
    private void Dash()
    {
        if (usandoDash || configPlayer.Energia < costoDash)
            return;

        // Consumo de energía
        configPlayer.Energia -= costoDash;
        if (configPlayer.Energia < 0) configPlayer.Energia = 0;

        // Pausar regeneración brevemente
        if (regenDelayCoroutine != null) StopCoroutine(regenDelayCoroutine);
        regenDelayCoroutine = StartCoroutine(PausarRegeneracion());

        usandoDash = true;

        // Iniciar rutinas de movimiento y efectos
        StartCoroutine(IEDash());
        StartCoroutine(DashEffectHollowKnight());
    }

    /// <summary>
    /// Corrutina principal del Dash: Aumenta velocidad y gestiona la visibilidad del arma.
    /// </summary>
    private IEnumerator IEDash()
    {
        // 1. Ocultar el arma real para evitar duplicados con el efecto fantasma
        if (weaponRenderer != null) 
        {
            weaponRenderer.enabled = false;
        }
        
        velocidadActual = velocidadDash;
        ModificarSpriteRenderer(transparencia); // Hacer semitransparente al jugador

        yield return new WaitForSeconds(tiempoDash);

        // 2. Restaurar visibilidad del arma
        if (weaponRenderer != null) 
        {
            weaponRenderer.enabled = true;
        }
        
        // Restaurar estado normal
        ModificarSpriteRenderer(1f);
        velocidadActual = velocidadMovimiento;
        usandoDash = false;
    }

    /// <summary>
    /// Genera copias estáticas (afterimages) detrás del jugador.
    /// </summary>
    private IEnumerator DashEffectHollowKnight()
    {
        // Efecto de estiramiento (Squash & Stretch)
        Vector3 originalScale = transform.localScale;
        Vector3 stretched = new Vector3(
            originalScale.x * stretchScaleX,
            originalScale.y * stretchScaleY,
            originalScale.z
        );

        transform.localScale = stretched;

        // Generar fantasmas en ráfaga
        for (int i = 0; i < ghostBurstCount; i++)
        {
            CreateGhost();
            yield return new WaitForSeconds(ghostBurstInterval);
        }

        yield return new WaitForSeconds(tiempoDash);

        transform.localScale = originalScale; // Volver a escala normal
    }

    private void CreateGhost()
    {
        if (ghostPrefab == null) return;

        GameObject ghost = Instantiate(ghostPrefab, transform.position, transform.rotation);
        SpriteRenderer ghostSR = ghost.GetComponent<SpriteRenderer>();

        // Copiar el sprite actual del jugador al fantasma
        if (ghostSR != null && spriteRenderer != null)
        {
            ghostSR.sprite = spriteRenderer.sprite;
            ghostSR.flipX = spriteRenderer.flipX;

            // Color aleatorio para efecto psicodélico/energético
            Color randomColor = new Color(
                Random.Range(0.6f, 1f),
                Random.Range(0.6f, 1f),
                Random.Range(0.6f, 1f),
                0.9f 
            );
            ghostSR.color = randomColor;
        }
        ghost.transform.localScale = transform.localScale;
    }

    private void ModificarSpriteRenderer(float valor)
    {
        Color color = spriteRenderer.color;
        spriteRenderer.color = new Color(color.r, color.g, color.b, valor);
    }

    private void RotarPlayer()
    {
        if (direccionMovimiento.x >= 0.1f) spriteRenderer.flipX = false;
        else if (direccionMovimiento.x < 0f) spriteRenderer.flipX = true;
    }

    private void CapturarInput()
    {
        direccionMovimiento = acciones.Movimiento.Mover.ReadValue<Vector2>().normalized;
    }

    private void OnEnable() => acciones.Enable();
    private void OnDisable() => acciones.Disable();

    private void RegenerarEnergia()
    {
        if (configPlayer.Energia < configPlayer.EnergiaMax)
        {
            configPlayer.Energia += regeneracionPorSegundo * Time.deltaTime;
            if (configPlayer.Energia > configPlayer.EnergiaMax)
                configPlayer.Energia = configPlayer.EnergiaMax;
        }
    }

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