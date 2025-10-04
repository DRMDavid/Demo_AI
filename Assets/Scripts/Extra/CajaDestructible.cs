using UnityEngine;

public class CajaDestructible : MonoBehaviour
{
    [Header("Estadísticas de la Caja")]
    public int vida = 3; // ¿Cuántos disparos necesita para romperse?

    [Header("Efectos Visuales y Sonido")]
    public GameObject efectoDestruccion; // Opcional: un prefab de partículas (humo, astillas, etc.)
    public AudioClip sonidoImpacto; // Opcional: sonido al recibir un disparo
    public AudioClip sonidoDestruccion; // Opcional: sonido al romperse

    private AudioSource audioSource;

    void Start()
    {
        // Si vamos a usar sonidos, es buena idea tener un AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    // ▼▼▼ MÉTODO MODIFICADO ▼▼▼
    // Este método se activa cuando otro Collider2D marcado como "Trigger" entra en nuestra área
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Primero, comprobamos si lo que ha entrado tiene la etiqueta "Bullet"
        if (other.gameObject.CompareTag("Bullet"))
        {
            // Destruimos la bala para que no siga su camino
            Destroy(other.gameObject);

            // Llamamos a la función que maneja el daño
            RecibirImpacto();
        }
    }

    public void RecibirImpacto()
    {
        // Reducimos la vida de la caja
        vida--;

        // Reproducimos el sonido de impacto si existe
        if (sonidoImpacto != null && audioSource != null)
        {
            audioSource.PlayOneShot(sonidoImpacto);
        }

        // Comprobamos si la vida ha llegado a cero
        if (vida <= 0)
        {
            DestruirCaja();
        }
    }

    private void DestruirCaja()
    {
        // Instanciamos el efecto de partículas en la posición de la caja si lo hemos asignado
        if (efectoDestruccion != null)
        {
            Instantiate(efectoDestruccion, transform.position, Quaternion.identity);
        }

        // Reproducimos el sonido de destrucción
        if (sonidoDestruccion != null)
        {
            AudioSource.PlayClipAtPoint(sonidoDestruccion, Camera.main.transform.position);
        }

        // Finalmente, destruimos el objeto de la caja
        Destroy(gameObject);
    }
}