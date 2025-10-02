/*using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : Singleton<MenuManager>
{
    [Header("Config")]
    [SerializeField] private PlayerCreacion[] players;

    [Header("UI")]
    [SerializeField] private GameObject panelPlayer;
    [SerializeField] private Image playerIcono;
    [SerializeField] private TextMeshProUGUI playerNombre;
    [SerializeField] private TextMeshProUGUI playerNivel;
    [SerializeField] private TextMeshProUGUI playerSaludTMP;
    [SerializeField] private TextMeshProUGUI playerArmaduraTMP;
    [SerializeField] private TextMeshProUGUI playerEnergia;
    [SerializeField] private TextMeshProUGUI playerCritico;
    [SerializeField] private TextMeshProUGUI monedasTMP;
    [SerializeField] private TextMeshProUGUI playerCostoDesbloquearTMP;
    [SerializeField] private TextMeshProUGUI playerCostoMejoraTMP;
    [SerializeField] private Image saludBarra;
    [SerializeField] private Image armaduraBarra;
    [SerializeField] private Image energiaBarra;
    [SerializeField] private Image chanceCriticoBarra;

    [Header("Botones")] 
    [SerializeField] private GameObject botonMejora;
    [SerializeField] private GameObject botonDesbloquear;
    [SerializeField] private GameObject botonSeleccionar;
    
    private PlayerSeleccion playerActual;
    
    private void Start()
    {
        CrearPlayers();
    }

    private void Update()
    { 
        monedasTMP.text = MonedasManager.Instance.Monedas.ToString();
    }

    private void CrearPlayers()
    {
        for (int i = 0; i < players.Length; i++)
        {
            PlayerMovimiento player = Instantiate(players[i].Player,
                players[i].PosicionCrear.position, Quaternion.identity, players[i].PosicionCrear);
            player.enabled = false;
            player.GetComponent<Rigidbody2D>().isKinematic = true;
        }
    }

    public void DesbloquearPlayer()
    {
        if (MonedasManager.Instance.Monedas >= playerActual.ConfigPlayer.CostoDesbloquear)
        {
            MonedasManager.Instance.RemoverMonedas(playerActual.ConfigPlayer.CostoDesbloquear);
            playerActual.ConfigPlayer.Desbloqueado = true;
            VerificarPlayer();
            MostrarPlayersStats();
        }
    }
    
    public void MejorarPlayer()
    {
        if (MonedasManager.Instance.Monedas >= playerActual.ConfigPlayer.CostoMejorar)
        {
            MonedasManager.Instance.RemoverMonedas(playerActual.ConfigPlayer.CostoMejorar);
            ActualizarPlayerStats();
            MostrarPlayersStats();
        }
    }
    
    public void SeleccionarPlayer()
    {
        if (playerActual.ConfigPlayer.Desbloqueado)
        {
            playerActual.GetComponent<PlayerMovimiento>().enabled = true;
            playerActual.GetComponent<Rigidbody2D>().isKinematic = false;   
            playerActual.ConfigPlayer.ResetPlayerStats();
            GameManager.Instance.Player = playerActual.ConfigPlayer;
            CerrarPanelPlayer();
        }
    }
    
    public void ClickPlayer(PlayerSeleccion player)
    {
        playerActual = player;
        VerificarPlayer();
        MostrarPlayersStats();
    }
    
    private void MostrarPlayersStats()
    {
        panelPlayer.SetActive(true);
        playerIcono.sprite = playerActual.ConfigPlayer.Icono;
        playerNombre.text = playerActual.ConfigPlayer.Nombre;
        playerNivel.text = $"Nivel {playerActual.ConfigPlayer.Nivel}";
        playerSaludTMP.text = playerActual.ConfigPlayer.SaludMax.ToString();
        playerArmaduraTMP.text = playerActual.ConfigPlayer.ArmaduraMax.ToString();
        playerEnergia.text = playerActual.ConfigPlayer.EnergiaMax.ToString();
        playerCritico.text = playerActual.ConfigPlayer.ChanceCritico.ToString();

        playerCostoDesbloquearTMP.text = playerActual.ConfigPlayer.CostoDesbloquear.ToString();
        playerCostoMejoraTMP.text = playerActual.ConfigPlayer.CostoMejorar.ToString();

        saludBarra.fillAmount = playerActual.ConfigPlayer.SaludMax /
                                playerActual.ConfigPlayer.SaludMejoraMax;
        armaduraBarra.fillAmount = playerActual.ConfigPlayer.ArmaduraMax /
                                   playerActual.ConfigPlayer.ArmaduraMejoraMax;
        energiaBarra.fillAmount = playerActual.ConfigPlayer.EnergiaMax /
                                  playerActual.ConfigPlayer.EnergiaMejoraMax;
        chanceCriticoBarra.fillAmount = playerActual.ConfigPlayer.ChanceCritico /
                                        playerActual.ConfigPlayer.ChanceCriticoMejoraMax;
    }

    private void ActualizarPlayerStats()
    {
        ConfiguracionPlayer config = playerActual.ConfigPlayer;
        config.Nivel++;
        config.SaludMax++;
        config.ArmaduraMax++;
        config.EnergiaMax += 10f;
        config.ChanceCritico += 2f;
        config.DañoCritico += 5f;

        config.SaludMax = Mathf.Min(config.SaludMax, config.SaludMejoraMax);
        config.ArmaduraMax = Mathf.Min(config.ArmaduraMax, config.ArmaduraMejoraMax);
        config.EnergiaMax = Mathf.Min(config.EnergiaMax, config.EnergiaMejoraMax);
        config.ChanceCritico = Mathf.Min(config.ChanceCritico, config.ChanceCriticoMejoraMax);
        
        float mejora = config.CostoMejorar;
        config.CostoMejorar = mejora + (mejora * (config.MultiplicadorMejora / 100f));
    }

    private void VerificarPlayer()
    {
        if (playerActual.ConfigPlayer.Desbloqueado == false)
        {
            botonSeleccionar.SetActive(false);
            botonMejora.SetActive(false);
            botonDesbloquear.SetActive(true);
        }
        else
        {
            botonMejora.SetActive(true);
            botonSeleccionar.SetActive(true);
            botonDesbloquear.SetActive(false);
        }
    }
    
    public void CerrarPanelPlayer()
    {
        panelPlayer.SetActive(false);
    }
}

[Serializable]
public class PlayerCreacion
{
    public PlayerMovimiento Player;
    public Transform PosicionCrear;
}
*/