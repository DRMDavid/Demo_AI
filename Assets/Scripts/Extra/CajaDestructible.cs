/*
Este código fue basado en el video de YouTube del canal Alexis Kotsiras:
Enlace: https://youtu.be/KNE5aMQHjag?si=SPH3qn0604N0vW8k
Integrantes del Equipo:
Hannin Abarca
Gael Jimenez
David Sanchez
*/
using UnityEngine;

// Componente para una caja o cualquier objeto que se rompe al recibir un número determinado de impactos.
public class CajaDestructible : MonoBehaviour
{
    [Header("Estadísticas de la Caja")]
    public int vida = 3; // Cuántos disparos necesita para romperse.

    [Header("Efectos Visuales y Sonido")]
    public GameObject efectoDestruccion; // Prefab de partículas (humo, astillas) para instanciar al romperse.
    public AudioClip sonidoImpacto; // Sonido al recibir un disparo.
    public AudioClip sonidoDestruccion; // Sonido al romperse.

    private AudioSource audioSource; // Componente para reproducir sonidos de impacto.

    void Start()
    {
        // Se busca el AudioSource o se añade uno si no existe, para reproducir el sonido de impacto.
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    // Este método se llama cuando un objeto con un Collider2D marcado como "Trigger" toca la caja.
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Comprueba si el objeto que colisionó tiene la etiqueta "Bullet".
        if (other.gameObject.CompareTag("Bullet"))
        {
            // Remueve la bala del juego inmediatamente.
            Destroy(other.gameObject);

            // Procesa el daño recibido.
            RecibirImpacto();
        }
    }

    // Lógica para procesar un impacto o un golpe.
    public void RecibirImpacto()
    {
        // Reduce la vida de la caja.
        vida--;

        // Reproduce el sonido de impacto, si está asignado.
        if (sonidoImpacto != null && audioSource != null)
        {
            // PlayOneShot evita que el clip se detenga si ya está sonando.
            audioSource.PlayOneShot(sonidoImpacto);
        }

        // Si la vida se agota, destruye la caja.
        if (vida <= 0)
        {
            DestruirCaja();
        }
    }

    // Gestiona la secuencia de destrucción de la caja.
    private void DestruirCaja()
    {
        // Instancia el efecto de partículas en la posición de la caja.
        if (efectoDestruccion != null)
        {
            Instantiate(efectoDestruccion, transform.position, Quaternion.identity);
        }

        // Reproduce el sonido de destrucción en una posición fija (en este caso, la cámara principal).
        if (sonidoDestruccion != null)
        {
            AudioSource.PlayClipAtPoint(sonidoDestruccion, Camera.main.transform.position);
        }

        // Elimina el objeto de la caja de la escena.
        Destroy(gameObject);
    }
}