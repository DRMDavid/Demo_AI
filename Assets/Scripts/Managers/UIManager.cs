/*
Este código fue tomado y adaptado de un curso de Udemy del creador Gianny Dantas:
"Aprende a crear un videojuego de acción 2D con Unity"
Enlace: https://www.udemy.com/course/aprende-a-crear-un-videojuego-de-accion-2d-con-unity/?couponCode=KEEPLEARNING
Integrantes del Equipo: 
Hannin Abarca 
Gael Jimenez 
David Sanchez 
*/
using TMPro; // Necesario para usar componentes TextMeshPro.
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Necesario para cargar la escena de Game Over.

// Clase que gestiona la Interfaz de Usuario (HUD) del juego.
public class UIManager : MonoBehaviour
{
    // Implementación del patrón Singleton para un acceso global al manager.
    public static UIManager Instance;

    [Header("Temp")]
    // Referencia al ScriptableObject o componente que guarda las estadísticas actuales del jugador.
    [SerializeField] private ConfiguracionPlayer configPlayer;

    [Header("UI Player")]
    // Componentes de imagen (barras) y texto (valores) para las estadísticas del jugador.
    [SerializeField] private Image barraSaludPlayer;
    [SerializeField] private TextMeshProUGUI textoSaludPlayer;
    [SerializeField] private Image barraArmaduraPlayer;
    [SerializeField] private TextMeshProUGUI textoArmaduraPlayer;
    [SerializeField] private Image barraEnergiaPlayer;
    [SerializeField] private TextMeshProUGUI textoEnergiaPlayer;

    [Header("UI Extra")]
    // Panel usado para efectos de transición (como el oscurecimiento de pantalla).
    [SerializeField] private CanvasGroup fadePanel;

    // Bandera para asegurar que la escena de Game Over solo se cargue una vez.
    private bool gameOverMostrado = false;

    // Se asegura de que la instancia Singleton se establezca al cargar la escena.
    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        ActualizarUI();

        // Lógica de Game Over: Si el jugador muere y la escena no ha sido cargada, la cargamos.
        if (!gameOverMostrado && configPlayer.SaludActual <= 0)
        {
            gameOverMostrado = true;
            SceneManager.LoadScene("GameOverScene"); 
        }
    }

    // Actualiza las barras y textos de las estadísticas del jugador.
    private void ActualizarUI()
    {
        // Usa Mathf.Lerp para una actualización suave de la barra de salud (animación).
        barraSaludPlayer.fillAmount = Mathf.Lerp(barraSaludPlayer.fillAmount,
            configPlayer.SaludActual / configPlayer.SaludMax, 10f * Time.deltaTime);
        // Actualización suave de la barra de armadura.
        barraArmaduraPlayer.fillAmount = Mathf.Lerp(barraArmaduraPlayer.fillAmount,
            configPlayer.Armadura / configPlayer.ArmaduraMax, 10f * Time.deltaTime);
        // Actualización suave de la barra de energía.
        barraEnergiaPlayer.fillAmount = Mathf.Lerp(barraEnergiaPlayer.fillAmount,
            configPlayer.Energia / configPlayer.EnergiaMax, 10f * Time.deltaTime);

        // Formatea y muestra los valores de texto de las estadísticas.
        textoSaludPlayer.text = $"{configPlayer.SaludActual}/{configPlayer.SaludMax}";
        textoArmaduraPlayer.text = $"{configPlayer.Armadura}/{configPlayer.ArmaduraMax}";
        // La energía se redondea al entero más cercano para la visualización.
        textoEnergiaPlayer.text = $"{Mathf.RoundToInt(configPlayer.Energia)}/{Mathf.RoundToInt(configPlayer.EnergiaMax)}";
    }

    // Método público para iniciar un efecto de fundido (fade) de la pantalla.
    public void FadeNuevoDungeon(float valor)
    {
        // Inicia una corrutina (presumiblemente definida en la clase Helper) para el efecto de transición.
        StartCoroutine(Helper.IEFade(fadePanel, valor, 1.5f));
    }
}