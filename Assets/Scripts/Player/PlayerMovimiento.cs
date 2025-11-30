/*******************************************************
 * NOMBRE DEL ARCHIVO: PlayerMovimiento.cs
 * AUTOR ORIGINAL: Gianny Dantas (Curso Udemy)
 * MODIFICADO Y AMPLIADO POR: Gael, david, Steve
 * INTEGRACIÓN ADICIONAL: Efecto Hollow Knight (afterimages + stretch + multicolor)
 *******************************************************/

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
    [SerializeField] private float costoDash = 5f;

    [Header("Energía")]
    [SerializeField] private ConfiguracionPlayer configPlayer;
    [SerializeField] private float regeneracionPorSegundo = 0.5f;

    [Header("Hollow Knight Dash (Afterimage + Stretch)")]
    [SerializeField] private GameObject ghostPrefab;
    [SerializeField] private int ghostBurstCount = 3;
    [SerializeField] private float ghostBurstInterval = 0.03f;
    [SerializeField] private float stretchScaleX = 1.4f;
    [SerializeField] private float stretchScaleY = 0.75f;

    private Rigidbody2D rb2D;
    private PlayerAcciones acciones;
    private SpriteRenderer spriteRenderer;

    private bool usandoDash;
    private float velocidadActual;
    private Vector2 direccionMovimiento;

    private bool puedeRegenerar = true;
    private Coroutine regenDelayCoroutine;

    private void Awake()
    {
        acciones = new PlayerAcciones();
        rb2D = GetComponent<Rigidbody2D>();

        // 👇 ESTO ES IMPORTANTE: agarra el SpriteRenderer real del player
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
            RegenerarEnergia();
    }

    private void FixedUpdate()
    {
        MoverPlayer();
    }

    private void MoverPlayer()
    {
        rb2D.MovePosition(rb2D.position + direccionMovimiento * (velocidadActual * Time.fixedDeltaTime));
    }

    private void Dash()
    {
        if (usandoDash || configPlayer.Energia < costoDash)
            return;

        configPlayer.Energia -= costoDash;
        if (configPlayer.Energia < 0) configPlayer.Energia = 0;

        if (regenDelayCoroutine != null) StopCoroutine(regenDelayCoroutine);
        regenDelayCoroutine = StartCoroutine(PausarRegeneracion());

        usandoDash = true;

        StartCoroutine(IEDash());
        StartCoroutine(DashEffectHollowKnight());
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

    private IEnumerator DashEffectHollowKnight()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 stretched = new Vector3(
            originalScale.x * stretchScaleX,
            originalScale.y * stretchScaleY,
            originalScale.z
        );

        transform.localScale = stretched;

        for (int i = 0; i < ghostBurstCount; i++)
        {
            CreateGhost();
            yield return new WaitForSeconds(ghostBurstInterval);
        }

        yield return new WaitForSeconds(tiempoDash);

        transform.localScale = originalScale;
    }

    private void CreateGhost()
    {
        if (ghostPrefab == null) return;

        GameObject ghost = Instantiate(ghostPrefab, transform.position, transform.rotation);

        SpriteRenderer ghostSR = ghost.GetComponent<SpriteRenderer>();

        if (ghostSR != null && spriteRenderer != null)
        {
            ghostSR.sprite = spriteRenderer.sprite;
            ghostSR.flipX = spriteRenderer.flipX;

            // 🎨 MULTICOLOR RANDOM
            Color randomColor = new Color(
                Random.Range(0.6f, 1f),
                Random.Range(0.6f, 1f),
                Random.Range(0.6f, 1f),
                0.9f // alpha inicial
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
