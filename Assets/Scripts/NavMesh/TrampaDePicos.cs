// TrampaDePicos.cs
using UnityEngine;

public class TrampaDePicos : MonoBehaviour
{
    [Tooltip("El daño que hacen los picos al contacto.")]
    [SerializeField] private int dano = 1;

    // Se activa cuando otro Collider2D entra en el trigger.
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Comprobamos si el objeto que ha entrado tiene la etiqueta "Player".
        if (other.CompareTag("Player"))
        {
            // Intentamos obtener el componente del script del jugador.
            PlayerMovimiento2D jugador = other.GetComponent<PlayerMovimiento2D>();

            // Si el componente existe, llamamos a su método para recibir daño.
            if (jugador != null)
            {
                jugador.RecibirDano(dano);
            }
        }
    }
}