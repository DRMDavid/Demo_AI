// using System;
// using UnityEngine;
//
// public class ItemTextoManager : Singleton<ItemTextoManager>
// {
//     [Header("Texto")]
//     [SerializeField] private ItemTexto textoPrefab;
//     
//     public ItemTexto MostrarMensaje(string mensaje, Vector3 posicion, Color color)
//     {
//         ItemTexto texto = Instantiate(textoPrefab, transform);
//         texto.EstablecerTexto(mensaje, color);
//         texto.transform.position = posicion;
//         return texto;
//     }
// }