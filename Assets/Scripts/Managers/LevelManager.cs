// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using Random = UnityEngine.Random;
//
// public class LevelManager : Singleton<LevelManager>
// {
//     public static event Action EventoRoomCompletado;
//     
//     [Header("Config")]
//     [SerializeField] private RoomPlantillas roomPlantillas;
//     [SerializeField] private LibreriaDungeon libreriaDungeon;
//
//     public GameObject PlayerSeleccionado { get; set; }
//     public RoomPlantillas RoomPlantillas => roomPlantillas;
//     public LibreriaDungeon LibreriaDungeon => libreriaDungeon;
//
//     private List<GameObject> cofreItemsNivelActual = new List<GameObject>();
//     private int nivelActualIndex;
//     private int dungeonActualIndex;
//     private int enemigosContador;
//     private GameObject dungeonGOActual;
//     private Room roomActual;
//
//     protected override void Awake()
//     {
//         base.Awake();
//         CrearPlayer();
//     }
//     
//     private void Start()
//     {
//         CrearDungeon();
//     }
//
//     private void CrearPlayer()
//     {
//         if (GameManager.Instance.Player != null)
//         {
//             PlayerSeleccionado = Instantiate(GameManager.Instance.Player.PlayerPrefab);
//         }
//     }
//     
//     private void CrearDungeon()
//     {
//         dungeonGOActual = 
//             Instantiate(libreriaDungeon.Niveles[nivelActualIndex].Dungeons[dungeonActualIndex], transform);
//         cofreItemsNivelActual = new List<GameObject>(libreriaDungeon.Niveles[nivelActualIndex].CofreItems.ItemsDisponibles);
//     }
//
//     private void ContinuarDungeons()
//     {
//         dungeonActualIndex++;
//         if (dungeonActualIndex > libreriaDungeon.Niveles[nivelActualIndex].Dungeons.Length - 1)
//         {
//             dungeonActualIndex = 0;
//             nivelActualIndex++;
//         }
//         
//         Destroy(dungeonGOActual);
//         CrearDungeon();
//         PosicionarPlayer();
//     }
//     
//     private void PosicionarPlayer()
//     {
//         Room[] roomsNuevoDungeon = dungeonGOActual.GetComponentsInChildren<Room>();
//         Room roomEntrada = null;
//         for (int i = 0; i < roomsNuevoDungeon.Length; i++)
//         {
//             if (roomsNuevoDungeon[i].TipoRoom == TipoRoom.RoomEntrada)
//             {
//                 roomEntrada = roomsNuevoDungeon[i];
//             }
//         }
//
//         if (roomEntrada != null)
//         {
//             if (PlayerSeleccionado == null) return;
//             PlayerSeleccionado.transform.position = roomEntrada.transform.position;
//         }
//     }
//
//     private void CrearBossEnRoom()
//     {
//         Vector3 posLibre = roomActual.ObtenerTileDisponible();
//         EnemigoFSM boss = Instantiate(libreriaDungeon.Niveles[nivelActualIndex].BossFinal,
//             posLibre, Quaternion.identity, roomActual.transform);
//         boss.RoomParent = roomActual;
//     }
//     
//     private void CrearEnemigosEnRoom()
//     {
//         int enemigosPorCrear = ObtenerCantidadEnemigosPorCrear();
//         enemigosContador = enemigosPorCrear;
//         for (int i = 0; i < enemigosPorCrear; i++)
//         {
//             Vector3 posLibre = roomActual.ObtenerTileDisponible();
//             EnemigoFSM enemigo = Instantiate(ObtenerEnemigoRandomPorCrear(), posLibre,
//                 Quaternion.identity, roomActual.transform);
//             enemigo.RoomParent = roomActual;
//         }
//     }
//     
//     private EnemigoFSM ObtenerEnemigoRandomPorCrear()
//     {
//         EnemigoFSM[] enemigos = libreriaDungeon.Niveles[nivelActualIndex].Enemigos;
//         int indexRandom = Random.Range(0, enemigos.Length);
//         EnemigoFSM enemigoObtenido = enemigos[indexRandom];
//         return enemigoObtenido;
//     }
//     
//     private int ObtenerCantidadEnemigosPorCrear()
//     {
//         int cantidad = Random.Range(libreriaDungeon.Niveles[nivelActualIndex].EnemigoMinPorRoom, 
//             libreriaDungeon.Niveles[nivelActualIndex].EnemigoMaxPorRoom + 1);
//         return cantidad;
//     }
//
//     private void CrearBonusEnergiaYMonedaEnemigoDerrotado(Vector3 posEnemigo)
//     {
//         int cantidadRandom = Random.Range(libreriaDungeon.Niveles[nivelActualIndex]
//             .CantidadMinBonusPorEnemigo, libreriaDungeon.Niveles[nivelActualIndex]
//             .CantidadMaxBonusPorEnemigo);
//         for (int i = 0; i < cantidadRandom; i++)
//         {
//             int bonusIndexRandom = Random.Range(0, libreriaDungeon.BonusEnemigo.Length);
//             Vector3 posBonus = Random.insideUnitCircle.normalized * libreriaDungeon.RadioCreacionBonus;
//             Instantiate(libreriaDungeon.BonusEnemigo[bonusIndexRandom],
//                 posEnemigo + posBonus, Quaternion.identity, roomActual.transform);
//         }
//     }
//     
//     private void CrearCofreEnRoomCompletado()
//     {
//         Vector3 posCofre = roomActual.ObtenerTileDisponible();
//         Instantiate(libreriaDungeon.Cofre, posCofre, Quaternion.identity, roomActual.transform);
//     }
//     
//     public GameObject ObtenerItemParaCofre()
//     {
//         int randomIndex = Random.Range(0, cofreItemsNivelActual.Count);
//         GameObject item = cofreItemsNivelActual[randomIndex];
//         cofreItemsNivelActual.Remove(item);
//         return item;
//     }
//
//     private void CrearLapidasEnEnemigosDerrotados(Vector3 enemigoPos)
//     {
//         Instantiate(libreriaDungeon.Lapida, enemigoPos,
//             Quaternion.identity, roomActual.transform);
//     }
//
//     private string ObtenerNivelActualTexto()
//     {
//         return $"{libreriaDungeon.Niveles[nivelActualIndex].Nombre} - {dungeonActualIndex + 1}";
//     }
//     
//     private IEnumerator IEContinuarDungeon()
//     {
//         UIManager.Instance.FadeNuevoDungeon(1f);
//         yield return new WaitForSeconds(1.5f);
//         ContinuarDungeons();
//         UIManager.Instance.ActualizarNivelActualTexto(ObtenerNivelActualTexto());
//         UIManager.Instance.FadeNuevoDungeon(0f);
//     }
//     
//     private void RespuestaEventoPlayerEnRoom(Room room)
//     {
//         roomActual = room;
//         if (roomActual.RoomCompletado == false)
//         {
//             roomActual.CerrarPuertas();
//             switch (roomActual.TipoRoom)
//             {
//                 case TipoRoom.RoomEnemigo:
//                     CrearEnemigosEnRoom();
//                     break;
//                 case TipoRoom.RoomBoss:
//                     CrearBossEnRoom();
//                     break;
//             }
//         }
//     }
//  
//     private void RespuestaEventoPortal()
//     {
//         StartCoroutine(IEContinuarDungeon());
//     }
//
//     private void RespuestaEventoEnemigoDerrotado(Transform enemigoPos)
//     {
//         CrearLapidasEnEnemigosDerrotados(enemigoPos.position);
//         CrearBonusEnergiaYMonedaEnemigoDerrotado(enemigoPos.position);
//         
//         enemigosContador--;
//         if (enemigosContador <= 0)
//         {
//             if (roomActual.RoomCompletado == false)
//             {
//                 enemigosContador = 0;
//                 roomActual.EstablecerRoomCompletado();
//                 CrearCofreEnRoomCompletado();
//                 EventoRoomCompletado?.Invoke();
//                 if (roomActual.TipoRoom == TipoRoom.RoomBoss && 
//                     nivelActualIndex < libreriaDungeon.Niveles.Length - 1)
//                 {
//                     Vector3 posLibre = roomActual.ObtenerTileDisponible();
//                     Instantiate(libreriaDungeon.Portal, posLibre, 
//                         Quaternion.identity, roomActual.transform);
//                 }
//             }
//         }
//     }
//     
//     private void OnEnable()
//     {
//         Room.EventoPlayerEnRoom += RespuestaEventoPlayerEnRoom;
//         Portal.EventoPortal += RespuestaEventoPortal;
//         EnemigoSalud.EventoEnemigoDerrotado += RespuestaEventoEnemigoDerrotado;
//     }
//
//     private void OnDisable()
//     {
//         Room.EventoPlayerEnRoom -= RespuestaEventoPlayerEnRoom;
//         Portal.EventoPortal -= RespuestaEventoPortal;
//         EnemigoSalud.EventoEnemigoDerrotado -= RespuestaEventoEnemigoDerrotado;
//     }
// }
