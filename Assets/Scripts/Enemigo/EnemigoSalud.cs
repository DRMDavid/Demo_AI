// using System;
// using System.Collections;
// using UnityEngine;
//
// public class EnemigoSalud : MonoBehaviour, IRecibirDaño
// {
//     public static event Action<Transform> EventoEnemigoDerrotado; 
//     
//     [Header("Config")]
//     [SerializeField] private float salud;
//
//     private SpriteRenderer sp;
//     private Color colorInicial;
//     private float saludActual;
//     private Coroutine colorCoroutine;
//     
//     private void Awake()
//     {
//         sp = GetComponent<SpriteRenderer>();
//     }
//
//     private void Start()
//     {
//         saludActual = salud;
//         colorInicial = sp.color;
//     }
//
//     private IEnumerator IERecibirDaño()
//     {
//         sp.color = Color.red;
//         yield return new WaitForSeconds(0.15f);
//         sp.color = colorInicial;
//     }
//
//     private void MostrarColorDaño()
//     {
//         if (colorCoroutine != null)
//         {
//             StopCoroutine(colorCoroutine);
//         }
//
//         colorCoroutine = StartCoroutine(IERecibirDaño());
//     }
//
//     public void RecibirDaño(float cantidad)
//     {
//         saludActual -= cantidad;
//         MostrarColorDaño();
//         if (saludActual <= 0)
//         {
//             EventoEnemigoDerrotado?.Invoke(transform);
//             Destroy(gameObject);
//         }
//     }
// }
