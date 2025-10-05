using UnityEngine;

public class PlayerSalud : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private ConfiguracionPlayer configPlayer;

    private void Awake()
    {
        // Reinicia la vida actual del jugador a su máximo al empezar el juego.
        configPlayer.SaludActual = configPlayer.SaludMax;
    }

    private void Update()
    {
 
    }
    
    public void RecuperarSalud(float cantidad)
    {
        configPlayer.SaludActual += cantidad;
        if (configPlayer.SaludActual > configPlayer.SaludMax)
        {
            configPlayer.SaludActual = configPlayer.SaludMax;
        }
    }

    public void RecibirDamage(float cantidad)
    {
        if (configPlayer.Armadura > 0)
        {
            float DamageRestante = cantidad - configPlayer.Armadura;
            configPlayer.Armadura = Mathf.Max(configPlayer.Armadura - cantidad, 0f);
            if (DamageRestante > 0)
            {
                configPlayer.SaludActual = Mathf.Max(configPlayer.SaludActual - DamageRestante, 0f);
            }
        }
        else
        {
            configPlayer.SaludActual = Mathf.Max(configPlayer.SaludActual - cantidad, 0f);
        }

        if (configPlayer.SaludActual <= 0)
        {
            PlayerDerrotado();
        }
    }

    private void PlayerDerrotado()
    {
        Destroy(gameObject);

    }
}