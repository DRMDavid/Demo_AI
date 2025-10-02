// using System;
// using UnityEngine;
//
// public class PlayerSalud : MonoBehaviour, IRecibirDaño
// {
//     public static event Action EventoPlayerDerrotado;
//     
//     [Header("Config")]
//     [SerializeField] private ConfiguracionPlayer configPlayer;
//     
//     public void RecuperarSalud(float cantidad)
//     {
//         configPlayer.SaludActual += cantidad;
//         if (configPlayer.SaludActual > configPlayer.SaludMax)
//         {
//             configPlayer.SaludActual = configPlayer.SaludMax;
//         }
//     }
//
//     public void RecibirDaño(float cantidad)
//     {
//         if (configPlayer.Armadura > 0)
//         {
//             float dañoRestante = cantidad - configPlayer.Armadura;
//             configPlayer.Armadura = Mathf.Max(configPlayer.Armadura - cantidad, 0f);
//             if (dañoRestante > 0)
//             {
//                 configPlayer.SaludActual = Mathf.Max(configPlayer.SaludActual - dañoRestante, 0f);
//             }
//         }
//         else
//         {
//             configPlayer.SaludActual = Mathf.Max(configPlayer.SaludActual - cantidad, 0f);
//         }
//
//         if (configPlayer.SaludActual <= 0)
//         {
//             PlayerDerrotado();
//         }
//     }
//
//     private void PlayerDerrotado()
//     {
//         EventoPlayerDerrotado?.Invoke();
//         Destroy(gameObject);
//     }
// }
