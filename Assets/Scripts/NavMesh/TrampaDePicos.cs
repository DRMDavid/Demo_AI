// ================================================================
// Archivo: TrampaDePicos.cs
// Descripci�n: Gestiona el comportamiento de una trampa de picos que 
// inflige da�o al jugador al entrar en contacto con ella.
// IMPLEMENTADO POR: Gael, David y Steve
// ================================================================

using UnityEngine;

/// <summary>
/// Controla una trampa de picos que causa da�o al jugador 
/// cuando este entra en su �rea de colisi�n.
/// </summary>
public class TrampaDePicos : MonoBehaviour
{
    [Tooltip("El da�o que hacen los picos al contacto.")]
    [SerializeField] private int dano = 1;

    /// <summary>
    /// Se ejecuta autom�ticamente cuando otro Collider2D entra en el trigger.
    /// </summary>
    /// <param name="other">El Collider2D que entra en contacto con la trampa.</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Comprobamos si el objeto que ha entrado tiene la etiqueta "Player".
        if (other.CompareTag("Player"))
        {
            // Intentamos obtener el componente del script del jugador.
            PlayerMovimiento2D jugador = other.GetComponent<PlayerMovimiento2D>();

            // Si el componente existe, llamamos a su m�todo para recibir da�o.
            if (jugador != null)
            {
                jugador.RecibirDano(dano);
            }
        }
    }
}
