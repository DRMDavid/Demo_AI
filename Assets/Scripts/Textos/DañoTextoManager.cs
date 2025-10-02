// using TMPro;
// using UnityEngine;
//
// public class DañoTextoManager : Singleton<DañoTextoManager>
// {
//     [SerializeField] private DañoTexto dañoTextoPrefab;
//
//     public void MostrarDaño(float valor, Transform personajePos)
//     {
//         Vector3 posTexto = Random.insideUnitCircle.normalized * 0.5f;
//         DañoTexto instanciaTexto = Instantiate(dañoTextoPrefab,
//             personajePos.position + posTexto, Quaternion.identity);
//         instanciaTexto.Parent = personajePos;
//         instanciaTexto.EstablecerDaño(valor);
//     }
// }