// using System.Collections;
// using Unity.XR.GoogleVr;
// using UnityEngine;
//
// public class PlayerMovimiento : MonoBehaviour
// {
//     [Header("Config")]
//     [SerializeField] private float velocidadMovimiento;
//
//     [Header("Dash")] 
//     [SerializeField] private float velocidadDash;
//     [SerializeField] private float tiempoDash;
//     [SerializeField] private float transparencia;
//
//     public Vector2 DireccionMovimiento => direccionMovimiento;
//     
//     private Rigidbody2D rb2D;
//     private PlayerAcciones acciones;
//     private SpriteRenderer spriteRenderer;
//
//     private bool usandoDash;
//     private float velocidadActual;
//     private Vector2 direccionMovimiento;
//
//     private void Awake()
//     {
//         acciones = new PlayerAcciones();
//         rb2D = GetComponent<Rigidbody2D>();
//         spriteRenderer = GetComponentInChildren<SpriteRenderer>();
//     }
//
//     private void Start()
//     { 
//         velocidadActual = velocidadMovimiento;
//         acciones.Movimiento.Dash.performed += ctx => Dash();
//     }
//
//     private void Update()
//     {
//         CapturarInput();
//         RotarPlayer();
//     }
//
//     private void FixedUpdate()
//     {
//         MoverPlayer();
//     }
//
//     private void MoverPlayer()
//     {
//         rb2D.MovePosition(rb2D.position + direccionMovimiento * 
//             (velocidadActual * Time.fixedDeltaTime));
//     }
//
//     private void Dash()
//     {
//         if (usandoDash)
//         {
//             return;
//         }
//
//         usandoDash = true;
//         StartCoroutine(IEDash());
//     }
//
//     private IEnumerator IEDash()
//     {
//         velocidadActual = velocidadDash;
//         ModificarSpriteRenderer(transparencia);
//         yield return new WaitForSeconds(tiempoDash);
//         ModificarSpriteRenderer(1f);
//         velocidadActual = velocidadMovimiento;
//         usandoDash = false;
//     }
//
//     private void ModificarSpriteRenderer(float valor)
//     {
//         Color color = spriteRenderer.color;
//         color = new Color(color.r, color.g, color.b, valor);
//         spriteRenderer.color = color;
//     }
//
//     public void GirarHaciaDerecha()
//     {
//         spriteRenderer.flipX = false;
//     }
//     
//     private void RotarPlayer()
//     {
//         if (direccionMovimiento.x >= 0.1f)
//         {
//             spriteRenderer.flipX = false;
//         }
//         else if (direccionMovimiento.x < 0f)
//         {
//             spriteRenderer.flipX = true;
//         }
//     }
//
//     private void CapturarInput()
//     {
//         direccionMovimiento =
//             acciones.Movimiento.Mover.ReadValue<Vector2>().normalized;
//     }
//
//     private void OnEnable()
//     {
//         acciones.Enable();
//     }
//
//     private void OnDisable()
//     {
//         acciones.Disable();
//     }
// }
