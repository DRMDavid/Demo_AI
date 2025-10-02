// using UnityEngine;
//
// public class AccionDetectarPlayerContinuo : FSMAccion
// {
//     [Header("Config")]
//     [SerializeField] private float radioDeteccion;
//     [SerializeField] private LayerMask playerMask;
//
//     private Collider2D[] resultados = new Collider2D[10];
//     private EnemigoFSM enemigoFsm;
//
//     private void Awake()
//     {
//         enemigoFsm = GetComponent<EnemigoFSM>();
//     }
//
//     public override void EjecutarAccion()
//     {
//         DetectarPlayer();
//     }
//
//     private void DetectarPlayer()
//     {
//         int cantidad = Physics2D.OverlapCircleNonAlloc(transform.position, 
//             radioDeteccion, resultados, playerMask);
//         if (cantidad <= 0)
//         {
//             enemigoFsm.Player = null;
//             return;
//         }
//
//         enemigoFsm.Player = resultados[0].transform;
//     }
//
//     private void OnDrawGizmos()
//     {
//         Gizmos.color = Color.cyan;
//         Gizmos.DrawWireSphere(transform.position, radioDeteccion);
//     }
// }