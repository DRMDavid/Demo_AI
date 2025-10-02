// using UnityEngine;
//
// public class DecisionPlayerEnVision : FSMDecision
// {
//     [Header("Config")]
//     [SerializeField] private LayerMask obstaculoMask;
//
//     private EnemigoFSM enemigoFsm;
//
//     private void Awake()
//     {
//         enemigoFsm = GetComponent<EnemigoFSM>();
//     }
//
//     public override bool Decidir(EnemigoFSM enemigo)
//     {
//         return DetectarPlayerEnLineaDeVision(enemigo);
//     }
//
//     private bool DetectarPlayerEnLineaDeVision(EnemigoFSM enemigo)
//     {
//         if (enemigo.Player == null) return false;
//         Vector3 dirHaciaPlayer = enemigo.Player.position - transform.position;
//         RaycastHit2D hit = Physics2D.Raycast(transform.position,
//             dirHaciaPlayer.normalized, dirHaciaPlayer.magnitude, obstaculoMask);
//         if (hit.collider != null)
//         {
//             return false;
//         }
//
//         return true; 
//     }
//
//     private void OnDrawGizmos()
//     {
//         if (enemigoFsm == null) return;
//         if (enemigoFsm.Player == null) return;
//
//         Gizmos.color = DetectarPlayerEnLineaDeVision(enemigoFsm) ? Color.green : Color.red;
//         Gizmos.DrawLine(transform.position, enemigoFsm.Player.position);
//     }
// }
