/*******************************************************
 * NOMBRE DEL ARCHIVO: PlayerEnergia.cs
 * AUTORES: Gianny Dantas (Curso Udemy)
 * ADAPTADO POR: Gael, David y steve
 * CURSO: Aprende a crear un videojuego de Acción 2D con Unity - Gianny Dantas (Udemy)
 * FUENTE: https://www.udemy.com/course/aprende-a-crear-un-videojuego-de-accion-2d-con-unity/
 * 
 * DESCRIPCIÓN:
 * Este script administra el consumo y la recuperación de energía del jugador.
 * Se conecta con el ScriptableObject "ConfiguracionPlayer" para modificar
 * los valores de energía actual y máxima del personaje.
 * 
 * FECHA: 05/10
 * 
 * NOTAS:
 * - Código original del curso mencionado, con comentarios agregados.
 * - No se realizaron modificaciones funcionales al código base.
 *******************************************************/

using UnityEngine;

public class PlayerEnergia : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private ConfiguracionPlayer configPlayer; // Referencia a la configuración del jugador (energía actual y máxima).

    private void Update()
    {
        // En el curso este método se deja vacío, pero podría usarse
        // para regenerar energía pasivamente o actualizar visuales.
    }

    /// <summary>
    /// Resta una cantidad de energía al jugador.
    /// </summary>
    /// <param name="cantidad">Cantidad de energía que se desea gastar.</param>
    public void GastarEnergia(float cantidad)
    {
        configPlayer.Energia -= cantidad;

        // Evita que el valor de energía sea negativo.
        if (configPlayer.Energia < 0f)
        {
            configPlayer.Energia = 0f;
        }
    }

    /// <summary>
    /// Recupera una cantidad de energía al jugador.
    /// </summary>
    /// <param name="cantidad">Cantidad de energía que se desea recuperar.</param>
    public void RecuperarEnergia(float cantidad)
    {
        configPlayer.Energia += cantidad;

        // Evita que la energía actual supere la energía máxima.
        if (configPlayer.Energia > configPlayer.EnergiaMax)
        {
            configPlayer.Energia = configPlayer.EnergiaMax;
        }
    }
}
