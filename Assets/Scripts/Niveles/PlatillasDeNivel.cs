/*
Este código fue tomado y adaptado de un curso de Udemy del creador Gianny Dantas:
"Aprende a crear un videojuego de acción 2D con Unity"
Enlace: https://www.udemy.com/course/aprende-a-crear-un-videojuego-de-accion-2d-con-unity/?couponCode=KEEPLEARNING
Integrantes del Equipo: 
Hannin Abarca 
Gael Jimenez 
David Sanchez 
*/
using System;
using System.Diagnostics.Contracts;
using UnityEngine;

// Permite crear una instancia de este ScriptableObject desde el menú de Unity (Assets -> Create -> Niveles/PlatillasDeNivel).
[CreateAssetMenu(fileName = "PlatillasDeNivel", menuName = "Niveles/PlatillasDeNivel")]
public class PlatillasDeNivel : ScriptableObject
{
    [Header("Platillas")]
    // Arreglo de texturas (patrones) que definen la distribución de objetos dentro de una sala.
    public Texture2D[] Plantillas; 

    [Header("Props")] 
    // Arreglo que asocia un color de la textura (Plantilla) con un Prefab específico.
    public NivelProps[] Prop;
}

// Hace que la clase sea serializable y, por lo tanto, visible y editable en el Inspector de Unity.
[Serializable]
public class NivelProps
{
    public string Nombre; 
    // Color del píxel en la plantilla que representa este objeto (Prop).
    public Color PropColor; 
    // El objeto (Prefab) que se instanciará cuando el color asociado sea encontrado en la plantilla.
    public GameObject PropPrefab; 
}