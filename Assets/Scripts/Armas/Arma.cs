// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
//
// public class Arma : MonoBehaviour
// {
//     [Header("Config")]
//     [SerializeField] protected Transform posDisparo;
//
//     [Header("Referencias")] 
//     [SerializeField] protected ItemArma itemArma;
//
//     public PersonajeArma PersonajeArmaParent { get; set; }
//     public ItemArma ItemArma => itemArma;
//     
//     private readonly int disparoAnim = Animator.StringToHash("Disparo");
//     private Animator animator;
//
//     private void Awake()
//     {
//         animator = GetComponent<Animator>();
//     }
//
//     protected void MostrarAnimacion()
//     {
//         animator.SetTrigger(disparoAnim);
//     }
//
//     public virtual void UsarArma()
//     {
//         
//     }
// }
