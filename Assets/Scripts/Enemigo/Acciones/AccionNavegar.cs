// using UnityEngine;
// using Random = UnityEngine.Random;
//
// public class AccionNavegar : FSMAccion
// {
//     [Header("Config")]
//     [SerializeField] private bool debug;
//     [SerializeField] private bool usarMovimientoRandom;
//     [SerializeField] private bool usarMovimientoTile;
//
//     [Header("Valores")]
//     [SerializeField] private float velocidadNavegacion;
//     [SerializeField] private Vector2 rangoMovimiento;
//     [SerializeField] private float distanciaMinCheck = 0.5f;
//
//     [Header("Obstaculos")]
//     [SerializeField] private LayerMask obstaculoMask;
//     [SerializeField] private float radioDeteccion;
//
//     private EnemigoFSM enemigoFsm;
//     private Vector3 posicionMovimiento;
//     private Vector3 direccionMovimiento;
//
//     private void Awake()
//     {
//         enemigoFsm = GetComponent<EnemigoFSM>();
//     }
//
//     private void Start()
//     {
//         ObtenerNuevaDireccionMovimiento();
//     }
//
//     public override void EjecutarAccion()
//     {
//         direccionMovimiento = (posicionMovimiento - transform.position).normalized;
//         transform.Translate(direccionMovimiento * (velocidadNavegacion * Time.deltaTime));
//         if (PodemosObtenerNuevaDireccion())
//         {
//             ObtenerNuevaDireccionMovimiento();
//         }
//     } 
//
//     private void ObtenerNuevaDireccionMovimiento()
//     {
//         if (usarMovimientoRandom)
//         {
//             posicionMovimiento = transform.position + ObtenerDireccionRandom();
//         } 
//
//         if (usarMovimientoTile)
//         {
//             posicionMovimiento = enemigoFsm.RoomParent.ObtenerTileDisponible();
//         }
//     }
//     
//     private Vector3 ObtenerDireccionRandom()
//     {
//         float randomX = Random.Range(-rangoMovimiento.x, rangoMovimiento.x);
//         float randomY = Random.Range(-rangoMovimiento.y, rangoMovimiento.y);
//         return new Vector3(randomX, randomY, 0f);
//     }
//
//     private bool PodemosObtenerNuevaDireccion()
//     {
//         if (Vector3.Distance(transform.position, posicionMovimiento) < distanciaMinCheck)
//         {
//             return true;
//         }
//
//         Collider2D[] resultados = new Collider2D[10];
//         int colisiones = Physics2D.OverlapCircleNonAlloc(transform.position,
//             radioDeteccion, resultados, obstaculoMask);
//         if (colisiones > 0)
//         {
//             for (int i = 0; i < colisiones; i++)
//             {
//                 if (resultados[i] != null)
//                 {
//                     Vector3 dirOpuesta = -direccionMovimiento;
//                     transform.position += dirOpuesta * 0.1f;
//                     return true;
//                 }
//             }
//         }
//
//         return false;
//     }
//     
//     private void OnDrawGizmos()
//     {
//         if (debug == false) return;
//         if (usarMovimientoRandom)
//         {
//             Gizmos.color = Color.red;
//             Gizmos.DrawWireCube(transform.position, rangoMovimiento * 2);
//         }
//         
//         Gizmos.color = Color.black;
//         Gizmos.DrawLine(transform.position, posicionMovimiento);
//         
//         Gizmos.color = Color.yellow;
//         Gizmos.DrawWireSphere(transform.position, radioDeteccion);
//     }
// }
