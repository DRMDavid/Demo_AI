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
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random; // Especifica que se usará el Random de UnityEngine.

// Enumeración para clasificar los tipos de salas.
public enum TipoRoom
{
    RoomLibre,
    RoomEntrada,
    RoomEnemigo,
}

// Clase principal que maneja la lógica de la sala (Room).
public class Nivel : MonoBehaviour

{
    [Header("Cnfiguración")]
    [SerializeField] private bool mostrarDebug;
    [SerializeField] private TipoRoom tipoRoom;
    
    [Header("Grid")]
    [SerializeField] private Tilemap tilemapextra; // Tilemap usado para marcar las posiciones de instanciación.
    
    /* Diccionario que almacena las posiciones del mundo (Vector3) donde se pueden instanciar objetos.
     El valor (bool) indica si la posición está libre (true) o ya fue usada (false).
    */
    private Dictionary<Vector3 ,bool> listaDeTiles = new Dictionary<Vector3 ,bool>();
    
    private void Start()
    {
        ObtenerTiles();
        GenerarRoomSegunPlantilla();
    }
    
    // Recorre el Tilemap auxiliar para obtener todas las posiciones disponibles para props.
    private void ObtenerTiles()
    {
        // Si la sala es de tipo normal (Entrada o Libre), no necesita generar props.
        if (EsRoomNormal())
        {
            return;
        }

        // Itera sobre todos los puntos dentro de los límites de las celdas del Tilemap.
        foreach (Vector3Int tilePos in tilemapextra.cellBounds.allPositionsWithin)
        {
            Vector3Int postLocal = new Vector3Int(tilePos.x, tilePos.y, tilePos.z);
            Vector3 posicion = tilemapextra.CellToWorld(postLocal);
            // Ajusta la posición para que esté en el centro del tile, no en la esquina (añadiendo 0.5f).
            Vector3 posicionModificada = new Vector3(posicion.x + 0.5f, posicion.y + 0.5f, posicion.z);
            
            // Verifica si realmente hay un tile en esa posición.
            if (tilemapextra.HasTile(postLocal))
            {
                // Agrega la posición centrada al diccionario, marcándola como disponible (true).
                listaDeTiles.Add(posicionModificada,true);
            }
        }
    }

    // Instancia props (enemigos, cofres, etc.) basándose en un patrón (plantilla).
    private void GenerarRoomSegunPlantilla()
    {
        // No genera props si es una sala simple.
        if (EsRoomNormal())
        {
            return;
        }

        // Selecciona una plantilla de forma aleatoria de las disponibles en LevelManager.
        int indexRandom = Random.Range(0, LevelManager.Instance.RoomPlantillas.Plantillas.Length);
        Texture2D textura = LevelManager.Instance.RoomPlantillas.Plantillas[indexRandom];
        
        // Crea una lista de las posiciones de los tiles disponibles para poder iterar por índice.
        List<Vector3> posiciones  = new List<Vector3>(listaDeTiles.Keys);
        
        // Itera sobre los píxeles de la textura de la plantilla.
        for (int y = 0, i = 0; y < textura.height; y++)
        {
            for (int x = 0; x < textura.width; x++, i++)
            {
                Color colorPixel = textura.GetPixel(x, y);
                
                // Compara el color del píxel con los colores de los Props definidos.
                foreach (NivelProps prop in LevelManager.Instance.RoomPlantillas.Prop)
                {
                    if (colorPixel == prop.PropColor)
                    {
                        // Si el color coincide, instancia el prefab del prop.
                        GameObject propCreado = Instantiate(prop.PropPrefab, tilemapextra.transform);
                        // Asigna la posición del tile actual al prop.
                        propCreado.transform.position = new Vector3(posiciones[i].x, posiciones[i].y, z:0f);
                        
                        // Marca la posición del tile como ya usada (false) en el diccionario.
                        if (listaDeTiles.ContainsKey(posiciones[i]))
                        {   
                            listaDeTiles[posiciones[i]] = false;
                        }
                    }
                }
            }
        }
    }
    
    // Método de utilidad para determinar si la sala es de un tipo que no requiere generación de props.
    private bool EsRoomNormal()
    {
        return tipoRoom ==  TipoRoom.RoomEntrada || tipoRoom ==  TipoRoom.RoomLibre;
    }
    
    // Dibuja ayudas visuales en el editor de Unity (Gizmos).
    private void OnDrawGizmosSelected()
    {
        // Comprobar si el debug está activo. Nota: usar '==' en lugar de '=' para comparación.
        if (mostrarDebug == false)
        {
            return;
        }

        if (listaDeTiles.Count > 0)
        {
            // Itera sobre el diccionario para dibujar un cubo (libre) o una esfera (usada).
            foreach (KeyValuePair<Vector3, bool> valor in listaDeTiles)
            {
                if (valor.Value)
                {
                    // Tile disponible (libre) - Dibuja un cubo verde.
                    Gizmos.color = Color.green;
                    Gizmos.DrawCube(valor.Key , Vector3.one * 0.8f);
                }
                else
                {
                    // Tile usado - Dibuja una esfera roja.
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(valor.Key , 0.3f);
                }
            }
        }
    }
}