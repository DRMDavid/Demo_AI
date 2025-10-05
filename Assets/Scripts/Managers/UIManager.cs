using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/*
Integrantes del equipo:
- Hannin Abarca
- David Sánchez
- Gael Jiménez
*/

/// <summary>
/// UIManager: Clase singleton que gestiona toda la interfaz de usuario del jugador.
/// Actualiza barras de salud, armadura y energía, y maneja la transición a la escena de Game Over.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance; // Singleton para acceso global

    [Header("Configuración Temporal")]
    [SerializeField] private ConfiguracionPlayer configPlayer; // Referencia al script de stats del jugador

    [Header("UI del Jugador")]
    [SerializeField] private Image barraSaludPlayer;          // Barra de salud
    [SerializeField] private TextMeshProUGUI textoSaludPlayer; // Texto de salud
    [SerializeField] private Image barraArmaduraPlayer;       // Barra de armadura
    [SerializeField] private TextMeshProUGUI textoArmaduraPlayer; // Texto de armadura
    [SerializeField] private Image barraEnergiaPlayer;        // Barra de energía
    [SerializeField] private TextMeshProUGUI textoEnergiaPlayer; // Texto de energía

    [Header("UI Extra")]
    [SerializeField] private CanvasGroup fadePanel;           // Panel para efecto de fade

    private bool gameOverMostrado = false; // Flag para evitar cargar varias veces GameOver

    /// <summary>
    /// Awake: Inicializa la instancia singleton.
    /// </summary>
    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Update: Actualiza la UI en tiempo real y verifica si el jugador muere.
    /// </summary>
    private void Update()
    {
        ActualizarUI();

        // Si el jugador muere, cargamos la escena GameOver inmediatamente
        if (!gameOverMostrado && configPlayer.SaludActual <= 0)
        {
            gameOverMostrado = true;
            SceneManager.LoadScene("GameOverScene"); // ⚠️ Asegúrate de que el nombre coincida con tu escena
        }
    }

    /// <summary>
    /// ActualizarUI: Interpola los valores de barras y actualiza los textos.
    /// </summary>
    private void ActualizarUI()
    {
        // Interpolación suave de las barras
        barraSaludPlayer.fillAmount = Mathf.Lerp(
            barraSaludPlayer.fillAmount,
            configPlayer.SaludActual / configPlayer.SaludMax,
            10f * Time.deltaTime
        );

        barraArmaduraPlayer.fillAmount = Mathf.Lerp(
            barraArmaduraPlayer.fillAmount,
            configPlayer.Armadura / configPlayer.ArmaduraMax,
            10f * Time.deltaTime
        );

        barraEnergiaPlayer.fillAmount = Mathf.Lerp(
            barraEnergiaPlayer.fillAmount,
            configPlayer.Energia / configPlayer.EnergiaMax,
            10f * Time.deltaTime
        );

        // Actualización de textos
        textoSaludPlayer.text = $"{configPlayer.SaludActual}/{configPlayer.SaludMax}";
        textoArmaduraPlayer.text = $"{configPlayer.Armadura}/{configPlayer.ArmaduraMax}";
        textoEnergiaPlayer.text = $"{Mathf.RoundToInt(configPlayer.Energia)}/{Mathf.RoundToInt(configPlayer.EnergiaMax)}";
    }

    /// <summary>
    /// FadeNuevoDungeon: Inicia un efecto de fade del panel para transición a nuevo dungeon.
    /// </summary>
    /// <param name="valor">Valor final del alpha del panel (0 = transparente, 1 = opaco)</param>
    public void FadeNuevoDungeon(float valor)
    {
        StartCoroutine(Helper.IEFade(fadePanel, valor, 1.5f));
    }
}
