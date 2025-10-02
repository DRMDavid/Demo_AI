// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
//
// public class GameManager : PersistentSingleton<GameManager>
// {
//     [Header("Armas")] 
//     [SerializeField] private Color armaComunColor;
//     [SerializeField] private Color armaRaraColor;
//     [SerializeField] private Color armaEpicaColor;
//     [SerializeField] private Color armaLegendariaColor;
//
//     public ConfiguracionPlayer Player { get; set; }
//     
//     public Color ObtenerColorArma(RarezaArma rareza)
//     {
//         switch (rareza)
//         {
//             case RarezaArma.Comun : return armaComunColor;
//             case RarezaArma.Raro : return armaRaraColor;
//             case RarezaArma.Epico : return armaEpicaColor;
//             case RarezaArma.Legendario : return armaLegendariaColor;
//         }
//         
//         return Color.white;
//     }
// }
