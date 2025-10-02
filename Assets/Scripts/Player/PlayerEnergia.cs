using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEnergia : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private ConfiguracionPlayer configPlayer;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            GastarEnergia(20f);
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            RecuperarEnergia(1f); 
        }
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
