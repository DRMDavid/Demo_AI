// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using Random = UnityEngine.Random;
//
// public class PlayerArma : PersonajeArma
// {
//     public static event Action<Arma> EventoActualizarArmaUI; 
//     
//     [Header("Player")]
//     [SerializeField] private ConfiguracionPlayer configPlayer;
//     
//     private PlayerAcciones acciones;
//     private PlayerMovimiento playerMovimiento;
//     private PlayerEnergia playerEnergia;
//     private PlayerDeteccion playerDeteccion;
//     
//     private Coroutine coroutineArma;
//     private ItemTexto textoNombre;
//     private int armaIndex;
//     private Arma[] armasEquipadas = new Arma[2];
//     
//     protected override void Awake()
//     {
//         base.Awake();
//         acciones = new PlayerAcciones();
//         playerDeteccion = GetComponentInChildren<PlayerDeteccion>();
//         playerEnergia = GetComponent<PlayerEnergia>();
//         playerMovimiento = GetComponent<PlayerMovimiento>();
//     }
//
//     private void Start()
//     {
//         acciones.Disparo.Click.performed += ctx => DispararArma();
//         acciones.Interacciones.CambiarArma.performed += _ => CambiarArma();
//     }
//
//     private void Update()
//     {
//         RotarArma();
//     }
//
//     private void CrearArma(Arma armaPrefab)
//     {
//         armaActual = Instantiate(armaPrefab,
//             armaPosRotacion.position, Quaternion.identity, armaPosRotacion);
//         armasEquipadas[armaIndex] = armaActual;
//         armasEquipadas[armaIndex].PersonajeArmaParent = this;
//         MostrarNombreArmaActual();
//         EventoActualizarArmaUI?.Invoke(armaActual);
//     }
//
//     public void EquiparArma(Arma arma)
//     {
//         if (armasEquipadas[0] == null)
//         {
//             CrearArma(arma);
//             return;
//         }
//
//         if (armasEquipadas[1] == null)
//         {
//             armaIndex++;
//             armasEquipadas[0].gameObject.SetActive(false);
//             CrearArma(arma);
//             return;
//         }
//         
//         // Destruir arma actual
//         Destroy(armaActual.gameObject);
//         armasEquipadas[armaIndex] = null;
//         
//         // Creamos arma en lugar del ultimo arma
//         CrearArma(arma);
//     }
//
//     private void CambiarArma()
//     {
//         if (armasEquipadas[1] == null) return;
//         for (int i = 0; i < armasEquipadas.Length; i++)
//         {
//             armasEquipadas[i].gameObject.SetActive(false);
//         }
//         
//         // Alternar entre 0 y 1
//         armaIndex = 1 - armaIndex;
//         armaActual = armasEquipadas[armaIndex];
//         armaActual.gameObject.SetActive(true);
//         ReiniciarArmaParaCambio();
//         MostrarNombreArmaActual();
//         EventoActualizarArmaUI?.Invoke(armaActual);
//     }
//
//     private void DispararArma()
//     {
//         if (armaActual != null)
//         {
//             if (PodemosUsarArma())
//             {
//                 armaActual.UsarArma();
//                 playerEnergia.GastarEnergia(armaActual.ItemArma.EnergiaRequerida);
//             }
//         }
//     }
//
//     private void RotarArma()
//     {
//         if (playerMovimiento.DireccionMovimiento != Vector2.zero)
//         {
//             RotarPosicionArma(playerMovimiento.DireccionMovimiento);
//         }
//
//         if (playerDeteccion.EnemigoObjetivo != null)
//         {
//             Vector3 dirHaciaEnemigo =
//                 playerDeteccion.EnemigoObjetivo.transform.position - transform.position;
//             RotarPosicionArma(dirHaciaEnemigo);
//         }
//     }
//     
//     private bool PodemosUsarArma()
//     {
//         if (armaActual.ItemArma.TipoArma == TipoArma.Pistola && playerEnergia.TenemosEnergia)
//         {
//             return true;
//         }
//
//         if (armaActual.ItemArma.TipoArma == TipoArma.Melee)
//         {
//             return true;
//         }
//
//         return false;
//     }
//
//     private void MostrarNombreArmaActual()
//     {
//         if (coroutineArma != null)
//         {
//             StopCoroutine(coroutineArma);
//         }
//
//         if (textoNombre != null && textoNombre.gameObject.activeInHierarchy)
//         {
//             Destroy(textoNombre.gameObject);
//         }
//         
//         coroutineArma = StartCoroutine(IEArmaActual());
//     }
//     
//     private IEnumerator IEArmaActual()
//     {
//         Vector3 posTexto = transform.position + Vector3.up;
//         Color colorArma = GameManager.Instance.ObtenerColorArma(armaActual.ItemArma.Rareza);
//         textoNombre = ItemTextoManager.Instance.MostrarMensaje(armaActual.ItemArma.ID, posTexto, colorArma);
//         textoNombre.transform.SetParent(transform);
//         yield return new WaitForSeconds(1.5f);
//         Destroy(textoNombre.gameObject);
//     }
//     
//     private void ReiniciarArmaParaCambio()
//     {
//         Transform armaTransform = armaActual.transform;
//         armaTransform.rotation = Quaternion.identity;
//         armaTransform.localScale = Vector3.one;
//         armaPosRotacion.rotation = Quaternion.identity;
//         armaPosRotacion.localScale = Vector3.one;
//         playerMovimiento.GirarHaciaDerecha();
//     }
//
//     public float ObtenerDañoConsiderandoCritico()
//     {
//         float daño = armaActual.ItemArma.Daño;
//         float porc = Random.Range(0f, 100f);
//         if (porc < configPlayer.ChanceCritico)
//         {
//             daño = (daño * (configPlayer.DañoCritico / 100f)) + daño;
//             return daño;
//         }
//
//         return daño;
//     }
//     
//     private void OnEnable()
//     {
//         acciones.Enable();
//     }
//
//     private void OnDisable()
//     {
//         acciones.Disable();
//     }
// }
