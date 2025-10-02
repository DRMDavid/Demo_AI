// using UnityEngine;
//
// public enum TipoArma
// {
//     Melee,
//     Pistola
// }
//
// public enum RarezaArma
// {
//     Comun,
//     Raro,
//     Epico,
//     Legendario
// }
//
// [CreateAssetMenu(menuName = "Items/Arma")]
// public class ItemArma : ItemData
// {
//     [Header("Datos")] 
//     public TipoArma TipoArma;
//     public RarezaArma Rareza;
//
//     [Header("Config")] 
//     public float Daño;
//     public float EnergiaRequerida;
//     public float TiempoEntreUsos;
//     public float DispersionMin;
//     public float DispersionMax;
//
//     [Header("Arma")] 
//     public Arma ArmaPrefab;
//     
//     public override void Recoger()
//     {
//         LevelManager.Instance.PlayerSeleccionado.GetComponent<PlayerArma>().EquiparArma(ArmaPrefab);
//     }
// }