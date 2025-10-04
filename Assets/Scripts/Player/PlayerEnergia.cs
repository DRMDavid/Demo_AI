using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEnergia : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private ConfiguracionPlayer configPlayer;

    private void Update()
    {
    }
    
    public void GastarEnergia(float cantidad)
    {
        configPlayer.Energia -= cantidad;
        if (configPlayer.Energia < 0f)
        {
            configPlayer.Energia = 0;
        }
    }

    public void RecuperarEnergia(float cantidad)
    {
        configPlayer.Energia += cantidad;
        if (configPlayer.Energia > configPlayer.EnergiaMax)
        {
            configPlayer.Energia = configPlayer.EnergiaMax;
        }
    }
}
