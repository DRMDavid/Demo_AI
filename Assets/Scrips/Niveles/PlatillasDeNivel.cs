using System;
using System.Diagnostics.Contracts;
using UnityEngine;

[CreateAssetMenu(fileName = "PlatillasDeNivel", menuName = "Niveles/PlatillasDeNivel")]
public class PlatillasDeNivel : ScriptableObject
{
   [Header("Platillas")]// Agregamos el Header para hacer más hordenado el inspector 
   //variable publica para agregar o referemciar las platillas del juego 
   public Texture2D[] Plantillas; //Lo hacemos un arreglo ya que tendremos mas de una platilla

   [Header("Props")] // aqui se referencia el color del Prop y su referencia a los Prefabs 
    public NivelProps[] Prop;
  
   
}

[Serializable]
public class NivelProps
{
    public string Nombre; //agrega en el inspector para que se pueda poner un nombre 
    public Color PropColor; //agrega en el inspector para que se pueda asignar un color  
    public GameObject PropPrefab; //agrega en el inspector para que se pueda referenciar el prefab con el color  
}
