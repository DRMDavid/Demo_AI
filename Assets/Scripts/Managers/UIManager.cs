// using System;
// using System.Collections;
// using System.Collections.Generic;
// using TMPro;
// using UnityEngine;
// using UnityEngine.SceneManagement;
// using UnityEngine.UI;
//
// public class UIManager : Singleton<UIManager>
// {
//     [Header("UI Player")] 
//     [SerializeField] private Image barraSaludPlayer;
//     [SerializeField] private TextMeshProUGUI textoSaludPlayer;
//     [SerializeField] private Image barraArmaduraPlayer;
//     [SerializeField] private TextMeshProUGUI textoArmaduraPlayer;
//     [SerializeField] private Image barraEnergiaPlayer;
//     [SerializeField] private TextMeshProUGUI textoEnergiaPlayer;
//
//     [Header("UI Extra")] 
//     [SerializeField] private CanvasGroup fadePanel;
//     [SerializeField] private GameObject gameOverPanel;
//     [SerializeField] private TextMeshProUGUI nivelActualTMP;
//     [SerializeField] private TextMeshProUGUI completadoTMP;
//     [SerializeField] private TextMeshProUGUI monedasTotalTMP;
//
//     [Header("UI Arma")]
//     [SerializeField] private GameObject panelArma;
//     [SerializeField] private TextMeshProUGUI armaActualEnergiaTMP;
//     [SerializeField] private Image armaActualIcono;
//     
//     private void Update()
//     {
//         monedasTotalTMP.text = MonedasManager.Instance.Monedas.ToString();
//         ActualizarUI();
//     }
//     
//     private void ActualizarUI()
//     {
//         ConfiguracionPlayer configPlayer = GameManager.Instance.Player;
//         
//         barraSaludPlayer.fillAmount = Mathf.Lerp(barraSaludPlayer.fillAmount, 
//             configPlayer.SaludActual / configPlayer.SaludMax, 10f * Time.deltaTime);
//         barraArmaduraPlayer.fillAmount = Mathf.Lerp(barraArmaduraPlayer.fillAmount, 
//             configPlayer.Armadura / configPlayer.ArmaduraMax, 10f * Time.deltaTime);
//         barraEnergiaPlayer.fillAmount = Mathf.Lerp(barraEnergiaPlayer.fillAmount, 
//             configPlayer.Energia / configPlayer.EnergiaMax, 10f * Time.deltaTime);
//         
//         textoSaludPlayer.text = $"{configPlayer.SaludActual}/{configPlayer.SaludMax}";
//         textoArmaduraPlayer.text = $"{configPlayer.Armadura}/{configPlayer.ArmaduraMax}";
//         textoEnergiaPlayer.text = $"{configPlayer.Energia}/{configPlayer.EnergiaMax}";
//     }
//
//     public void FadeNuevoDungeon(float valor)
//     {
//         StartCoroutine(Helper.IEFade(fadePanel, valor, 1.5f));
//     }
//
//     public void ActualizarNivelActualTexto(string nivelTexto)
//     {
//         nivelActualTMP.text = nivelTexto;
//     }
//
//     public void ButtonJugar()
//     {
//         SceneManager.LoadScene("Inicio");
//     }
//     
//     private IEnumerator IERoomCompletado()
//     {
//         completadoTMP.gameObject.SetActive(true);
//         yield return new WaitForSeconds(3f);
//         completadoTMP.gameObject.SetActive(false);
//     }
//
//     private void RespuestaEventoRoomCompletado()
//     {
//         StartCoroutine(IERoomCompletado());
//     }
//
//     private void RespuestaEventoActualizarArmaUI(Arma arma)
//     {
//         if (panelArma.activeSelf == false)
//         {
//             panelArma.SetActive(true);
//         }
//
//         armaActualEnergiaTMP.text = arma.ItemArma.EnergiaRequerida.ToString();
//         armaActualIcono.sprite = arma.ItemArma.Icono;
//     }
//
//     private void RespuestaEventoPlayerDerrotado()
//     {
//         gameOverPanel.SetActive(true);
//     }
//     
//     private void OnEnable()
//     {
//         LevelManager.EventoRoomCompletado += RespuestaEventoRoomCompletado;
//         PlayerArma.EventoActualizarArmaUI += RespuestaEventoActualizarArmaUI;
//         PlayerSalud.EventoPlayerDerrotado += RespuestaEventoPlayerDerrotado;
//     }
//
//     private void OnDisable()
//     {
//         LevelManager.EventoRoomCompletado -= RespuestaEventoRoomCompletado;
//         PlayerArma.EventoActualizarArmaUI -= RespuestaEventoActualizarArmaUI;
//         PlayerSalud.EventoPlayerDerrotado -= RespuestaEventoPlayerDerrotado;
//     }
// }
