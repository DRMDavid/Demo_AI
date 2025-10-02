using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public enum TipoRoom
{
    RoomLibre,
    RoomEntrada,
    RoomEnemigo,
}
public class Nivel : MonoBehaviour

{
    [Header("Cnfiguración")]
    [SerializeField] private bool mostrarDebug;
    [SerializeField] private TipoRoom tipoRoom;
    
    [Header("Grid")]
    [SerializeField] private Tilemap tilemapextra;
    
    /* esto no lo sabia pero cada diccioario tiene que estar asociado a una key y esa key tiene que tener un valor en este caso
     el valor es el bool y la key es el vector  
    */
    private Dictionary<Vector3 ,bool> listaDeTiles = new Dictionary<Vector3 ,bool>();
    
    private void Start()
    {
        ObtenerTiles();
        GenerarRoomSegunPlantilla();
    }
    // lo usaremos para instaciar enemigos y cofres 
    private void ObtenerTiles()
    {
        if (EsRoomNormal())
        {
            return;
        }

        foreach (Vector3Int tilePos in tilemapextra.cellBounds.allPositionsWithin)
        {
            Vector3Int postLocal = new Vector3Int(tilePos.x, tilePos.y, tilePos.z);
            Vector3 posicion = tilemapextra.CellToWorld(postLocal);
            Vector3 posicionModificada = new Vector3(posicion.x + 0.5f, posicion.y + 0.5f, posicion.z);
            if (tilemapextra.HasTile(postLocal))
            {
                listaDeTiles.Add(posicionModificada,true);
            }
        }
    }

    private void GenerarRoomSegunPlantilla()
    {
        if (EsRoomNormal())
        {
            return;
        }

        int indexRandom = Random.Range(0, LevelManager.Instance.RoomPlantillas.Plantillas.Length);
        Texture2D textura = LevelManager.Instance.RoomPlantillas.Plantillas[indexRandom];
        List<Vector3> posiciones  = new List<Vector3>(listaDeTiles.Keys);
        for (int y = 0, i = 0; y < textura.height; y++)
        {
            for (int x = 0; x < textura.width; x++, i++)
            {
                Color colorPixel = textura.GetPixel(x, y);
                foreach (NivelProps prop in LevelManager.Instance.RoomPlantillas.Prop)
                {
                    if (colorPixel == prop.PropColor)
                    {
                        GameObject propCreado = Instantiate(prop.PropPrefab, tilemapextra.transform);
                        propCreado.transform.position = new Vector3(posiciones[i].x, posiciones[i].y, z:0f);
                        if (listaDeTiles.ContainsKey(posiciones[i]))
                        {   
                            //Tile ya no esta disponible porque fue asignado a un prefab 
                            listaDeTiles[posiciones[i]] = false;
                        }
                    }
                }
            }
        }
    }
    
    private bool EsRoomNormal()
    {
        return tipoRoom ==  TipoRoom.RoomEntrada || tipoRoom ==  TipoRoom.RoomLibre;
    }
    private void OnDrawGizmosSelected()
    {
        if (mostrarDebug = false)
        {
            return;
        }

        if (listaDeTiles.Count > 0)
        {
            foreach (KeyValuePair<Vector3, bool> valor in listaDeTiles)
            {
                if (valor.Value)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawCube(valor.Key , Vector3.one * 0.8f);
                }
                else
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(valor.Key , 0.3f);
                }

            }

                
        }
    }
}