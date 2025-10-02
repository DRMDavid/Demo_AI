// using UnityEngine;
//
// public class EnemigoArma : PersonajeArma
// {
//     [Header("Config")]
//     [SerializeField] private Arma armaInicial;
//
//     public Arma ArmaActual => armaActual;
//     
//     protected override void Awake()
//     {
//         base.Awake();
//         CrearArma();
//     }
//     
//     private void Update()
//     {
//         if (LevelManager.Instance.PlayerSeleccionado == null) return;
//         Vector3 dirHaciaPlayer =
//             LevelManager.Instance.PlayerSeleccionado.transform.position - transform.position;
//         RotarPosicionArma(dirHaciaPlayer);
//     }
//
//     private void CrearArma()
//     {
//         armaActual = Instantiate(armaInicial, armaPosRotacion.position,
//             Quaternion.identity, armaPosRotacion);
//     }
//
//     public void UsarArma()
//     {
//         armaActual.UsarArma();
//     }
// }
