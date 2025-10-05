/*******************************************************
 * NOMBRE DEL ARCHIVO: PlayerSalud.cs
 * AUTOR: Gianny Dantas
 * CURSO: Aprende a crear un videojuego de Acción 2D con Unity
 * FUENTE: https://www.udemy.com/course/aprende-a-crear-un-videojuego-de-accion-2d-con-unity/
 * 
 * DESCRIPCIÓN:
 * Controla la salud y armadura del jugador.
 * Permite recibir daño, regenerar salud y manejar la derrota del jugador.
 * 
 * NOTAS:
 * - Código completamente original del curso de Gianny Dantas.
 * - No se realizaron modificaciones ni adaptaciones personales.
 * 
 * FECHA: 05/10
 *******************************************************/

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
        // Se deja vacío, posible uso futuro.
    }

    /// <summary>
    /// Recupera salud hasta el valor máximo permitido.
    /// </summary>
    public void RecuperarSalud(float cantidad)
    {
        configPlayer.SaludActual += cantidad;
        if (configPlayer.SaludActual > configPlayer.SaludMax)
        {
            configPlayer.SaludActual = configPlayer.SaludMax;
        }
    }

    /// <summary>
    /// Aplica daño al jugador considerando la armadura.
    /// </summary>
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

    /// <summary>
    /// Destruye el objeto del jugador cuando su salud llega a cero.
    /// </summary>
    private void PlayerDerrotado()
    {
        Destroy(gameObject);
    }
}
