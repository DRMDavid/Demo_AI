/*
Este código fue tomado y adaptado de un curso de Udemy del creador Gianny Dantas:
"Aprende a crear un videojuego de acción 2D con Unity"
Enlace: https://www.udemy.com/course/aprende-a-crear-un-videojuego-de-accion-2d-con-unity/?couponCode=KEEPLEARNING
Integrantes del Equipo: 
Hannin Abarca 
Gael Jimenez 
David Sanchez 
*/
using UnityEngine;
using UnityEngine.Serialization;

public class LevelManager : MonoBehaviour
{ 
    // Implementación del patrón Singleton para un acceso global al manager.
    public static LevelManager Instance;
  
    // Atributo que mantiene la compatibilidad de serialización si se cambia el nombre de la variable.
    [FormerlySerializedAs("platillas")] 
    [Header("Congfi")]
    // Referencia al ScriptableObject que contiene todas las plantillas y props.
    [SerializeField] private PlatillasDeNivel roomPlantillas;
  
    // Propiedad de solo lectura para acceder a las plantillas desde otras clases.
    public PlatillasDeNivel RoomPlantillas  => roomPlantillas;

    // Se asegura de que la instancia Singleton se establezca al cargar la escena.
    private void Awake()
    {
        Instance = this;
    }
}