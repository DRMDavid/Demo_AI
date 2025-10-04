using System.Collections;
using UnityEngine;

public class PlayerMovimiento : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float velocidadMovimiento;

    [Header("Dash")] 
    [SerializeField] private float velocidadDash;
    [SerializeField] private float tiempoDash;
    [SerializeField] private float transparencia;
    [SerializeField] private float costoDash = 5f;   // ✅ costo fijo

    [Header("Energía")]
    [SerializeField] private ConfiguracionPlayer configPlayer;
    [SerializeField] private float regeneracionPorSegundo = 0.5f; // ✅ regeneración lenta

    private Rigidbody2D rb2D;
    private PlayerAcciones acciones;
    private SpriteRenderer spriteRenderer;

    private bool usandoDash;
    private float velocidadActual;
    private Vector2 direccionMovimiento;

    private bool puedeRegenerar = true;  // ✅ control regeneración
    private Coroutine regenDelayCoroutine;

    private void Awake()
    {
        acciones = new PlayerAcciones();
        rb2D = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    { 
        velocidadActual = velocidadMovimiento;
        acciones.Movimiento.Dash.performed += ctx => Dash();
    }

    private void Update()
    {
        CapturarInput();
        RotarPlayer();
        MoverPlayer();

        if (puedeRegenerar)
        {
            RegenerarEnergia();
        }
    }

    private void FixedUpdate()
    {
        MoverPlayer();
    }

    private void MoverPlayer()
    {
        rb2D.MovePosition(rb2D.position + direccionMovimiento * 
            (velocidadActual * Time.fixedDeltaTime));
    }

    private void Dash()
    {
        if (usandoDash || configPlayer.Energia < costoDash)
        {
            return;
        }

        // ✅ gastar energía
        configPlayer.Energia -= costoDash;
        if (configPlayer.Energia < 0) configPlayer.Energia = 0;

        // ✅ pausa regeneración por 2 segundos
        if (regenDelayCoroutine != null) StopCoroutine(regenDelayCoroutine);
        regenDelayCoroutine = StartCoroutine(PausarRegeneracion());

        usandoDash = true;
        StartCoroutine(IEDash());
    }

    private IEnumerator IEDash()
    {
        velocidadActual = velocidadDash;
        ModificarSpriteRenderer(transparencia);
        yield return new WaitForSeconds(tiempoDash);
        ModificarSpriteRenderer(1f);
        velocidadActual = velocidadMovimiento;
        usandoDash = false;
    }

    private void ModificarSpriteRenderer(float valor)
    {
        Color color = spriteRenderer.color;
        color = new Color(color.r, color.g, color.b, valor);
        spriteRenderer.color = color;
    }

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

    private void CapturarInput()
    {
        direccionMovimiento =
            acciones.Movimiento.Mover.ReadValue<Vector2>().normalized;
    }

    private void OnEnable()
    {
        acciones.Enable();
    }

    private void OnDisable()
    {
        acciones.Disable();
    }

    // ✅ regeneración automática
    private void RegenerarEnergia()
    {
        if (configPlayer.Energia < configPlayer.EnergiaMax)
        {
            configPlayer.Energia += regeneracionPorSegundo * Time.deltaTime;
            if (configPlayer.Energia > configPlayer.EnergiaMax)
                configPlayer.Energia = configPlayer.EnergiaMax;
        }
    }

    // ✅ pausa regeneración 2 segundos
    private IEnumerator PausarRegeneracion()
    {
        puedeRegenerar = false;
        yield return new WaitForSeconds(2f);  // pausa
        puedeRegenerar = true;                // vuelve a regenerar
    }

}
