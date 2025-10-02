// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
//
// public class PlayerDeteccion : MonoBehaviour
// {
//     [Header("Config")]
//     [SerializeField] private float radioDeteccion;
//     [SerializeField] private bool debug;
//
//     [Header("Raycast")] 
//     [SerializeField] private LayerMask obstaculoMask;
//     
//     public EnemigoSalud EnemigoObjetivo { get; private set; }
//
//     private CircleCollider2D miCollider2D;
//     private List<EnemigoSalud> listaEnemigos = new List<EnemigoSalud>();
//     private List<EnemigoSalud> listaEnemigosEnVision = new List<EnemigoSalud>();
//     
//     private void Awake()
//     {
//         miCollider2D = GetComponent<CircleCollider2D>();
//     }
//
//     private void Start()
//     {
//         miCollider2D.radius = radioDeteccion;
//     }
//
//     private void Update()
//     {
//         CalcularEnemigosEnVision();
//         ObtenerEnemigoMasCercano();
//     }
//
//     private void ObtenerEnemigoMasCercano()
//     {
//         float distanciaMin = Mathf.Infinity;
//         EnemigoSalud enemigoBuscado = null;
//         for (int i = 0; i < listaEnemigosEnVision.Count; i++)
//         {
//             Vector3 enemigoPos = listaEnemigosEnVision[i].transform.position;
//             float distanciaConEnemigo = Vector3.Distance(transform.position, enemigoPos);
//             if (distanciaConEnemigo < distanciaMin)
//             {
//                 enemigoBuscado = listaEnemigosEnVision[i];
//                 distanciaMin = distanciaConEnemigo;
//             }
//         }
//
//         // si encontramos al enemigo, lo asignamos
//         if (enemigoBuscado != null)
//         {
//             EnemigoObjetivo = enemigoBuscado;
//             listaEnemigosEnVision.Clear();
//         }
//     }
//     
//     private void CalcularEnemigosEnVision()
//     {
//         for (int i = 0; i < listaEnemigos.Count; i++)
//         {
//             if (listaEnemigos.Count == 0 || listaEnemigos == null)
//             {
//                 return;
//             }
//
//             Vector3 posicionPlayer = transform.position;
//             Vector3 dirHaciaEnemigo = listaEnemigos[i].transform.position - posicionPlayer;
//             RaycastHit2D hit = Physics2D.Raycast(posicionPlayer,
//                 dirHaciaEnemigo, dirHaciaEnemigo.magnitude, obstaculoMask);
//             if (hit.collider == null)
//             {
//                 if (listaEnemigosEnVision.Contains(listaEnemigos[i]) == false)
//                 {
//                     listaEnemigosEnVision.Add(listaEnemigos[i]);
//                 }
//             }
//             else
//             {
//                 if (listaEnemigosEnVision.Contains(listaEnemigos[i]))
//                 {
//                     listaEnemigosEnVision.Remove(listaEnemigos[i]);
//                 }
//
//                 if (EnemigoObjetivo == listaEnemigos[i])
//                 {
//                     EnemigoObjetivo = null;
//                 }
//             }
//         }
//     }
//     
//     private void OnTriggerEnter2D(Collider2D other)
//     {
//         if (other.CompareTag("Enemigo"))
//         {
//             EnemigoSalud enemigo = other.GetComponent<EnemigoSalud>();
//             listaEnemigos.Add(enemigo);
//         }
//     }
//
//     private void OnTriggerExit2D(Collider2D other)
//     {
//         if (other.CompareTag("Enemigo"))
//         {
//             EnemigoSalud enemigo = other.GetComponent<EnemigoSalud>();
//             if (listaEnemigos.Contains(enemigo))
//             {
//                 listaEnemigos.Remove(enemigo);
//             }
//
//             if (enemigo == EnemigoObjetivo)
//             {
//                 EnemigoObjetivo = null;
//             }
//         }
//     }
//
//     private void OnDrawGizmos()
//     {
//         if (debug == false)
//         {
//             return;
//         }
//
//         Gizmos.color = Color.red;
//         for (int i = 0; i < listaEnemigos.Count; i++)
//         {
//             Gizmos.DrawLine(transform.position, listaEnemigos[i].transform.position);
//         }
//
//         if (EnemigoObjetivo != null)
//         {
//             Gizmos.color = Color.blue;
//             Gizmos.DrawLine(transform.position, EnemigoObjetivo.transform.position);
//         }
//     }
// }
