// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
//
// public class PersonajeArma : MonoBehaviour
// {
//     [Header("Config")] 
//     [SerializeField] protected Transform armaPosRotacion;
//     
//     protected Arma armaActual;
//     protected SpriteRenderer sp;
//     
//     protected virtual void Awake()
//     {
//         sp = GetComponent<SpriteRenderer>();
//         if (sp == null)
//         {
//             sp = GetComponentInChildren<SpriteRenderer>();
//         }
//     }
//     
//     protected void RotarPosicionArma(Vector3 direccion)
//     {
//         if (armaActual == null) return;
//         float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
//         if (direccion.x > 0f)
//         { 
//             armaPosRotacion.localScale = Vector3.one;
//             armaActual.transform.localScale = Vector3.one;
//             sp.flipX = false;
//         }
//         else if (direccion.x < 0f)
//         {
//             armaPosRotacion.localScale = new Vector3(-1, 1, 1);
//             armaActual.transform.localScale = new Vector3(-1, -1, 1);
//             sp.flipX = true; 
//         }
//
//         armaActual.transform.eulerAngles = new Vector3(0f, 0f, angulo);
//     }
// }
