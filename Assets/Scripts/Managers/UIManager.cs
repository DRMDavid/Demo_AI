using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Temp")]
    [SerializeField] private ConfiguracionPlayer configPlayer;

    [Header("UI Player")]
    [SerializeField] private Image barraSaludPlayer;
    [SerializeField] private TextMeshProUGUI textoSaludPlayer;
    [SerializeField] private Image barraArmaduraPlayer;
    [SerializeField] private TextMeshProUGUI textoArmaduraPlayer;
    [SerializeField] private Image barraEnergiaPlayer;
    [SerializeField] private TextMeshProUGUI textoEnergiaPlayer;

    [Header("UI Extra")]
    [SerializeField] private CanvasGroup fadePanel;

    private bool gameOverMostrado = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        ActualizarUI();

        // Si el jugador muere, cargamos la escena GameOver inmediatamente
        if (!gameOverMostrado && configPlayer.SaludActual <= 0)
        {
            gameOverMostrado = true;
            SceneManager.LoadScene("GameOverScene"); // 👈 asegúrate de que el nombre coincida con tu escena
        }
    }

    private void ActualizarUI()
    {
        barraSaludPlayer.fillAmount = Mathf.Lerp(barraSaludPlayer.fillAmount,
            configPlayer.SaludActual / configPlayer.SaludMax, 10f * Time.deltaTime);
        barraArmaduraPlayer.fillAmount = Mathf.Lerp(barraArmaduraPlayer.fillAmount,
            configPlayer.Armadura / configPlayer.ArmaduraMax, 10f * Time.deltaTime);
        barraEnergiaPlayer.fillAmount = Mathf.Lerp(barraEnergiaPlayer.fillAmount,
            configPlayer.Energia / configPlayer.EnergiaMax, 10f * Time.deltaTime);

        textoSaludPlayer.text = $"{configPlayer.SaludActual}/{configPlayer.SaludMax}";
        textoArmaduraPlayer.text = $"{configPlayer.Armadura}/{configPlayer.ArmaduraMax}";
        textoEnergiaPlayer.text = $"{Mathf.RoundToInt(configPlayer.Energia)}/{Mathf.RoundToInt(configPlayer.EnergiaMax)}";
    }

    public void FadeNuevoDungeon(float valor)
    {
        StartCoroutine(Helper.IEFade(fadePanel, valor, 1.5f));
    }
}
