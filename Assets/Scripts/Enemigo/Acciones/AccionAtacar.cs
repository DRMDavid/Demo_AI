// using UnityEngine;
//
// public class AccionAtacar : FSMAccion
// {
//     private EnemigoArma enemigoArma;
//     private EnemigoFSM enemigoFsm;
//     private float contadorTimer;
//     
//     private void Awake()
//     {
//         enemigoArma = GetComponent<EnemigoArma>();
//         enemigoFsm = GetComponent<EnemigoFSM>();
//     }
//
//     private void Start()
//     {
//         if (enemigoArma.ArmaActual != null)
//         {
//             contadorTimer = enemigoArma.ArmaActual.ItemArma.TiempoEntreUsos;
//         }
//     }
//
//     public override void EjecutarAccion()
//     {
//         Atacar();
//     }
//
//     private void Atacar()
//     {
//         if (enemigoFsm.Player == null) return;
//         contadorTimer -= Time.deltaTime;
//         if (contadorTimer <= 0)
//         {
//             contadorTimer = enemigoArma.ArmaActual.ItemArma.TiempoEntreUsos;
//             enemigoArma.UsarArma();
//         }
//     }
// }
