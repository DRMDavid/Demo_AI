using UnityEngine;

public class EfectoFlotar : MonoBehaviour
{
    public float velocidad = 2f;
    public float altura = 0.25f;
    
    private Vector3 posInicial;

    void Start()
    {
        posInicial = transform.position;
    }

    void Update()
    {
        // Mueve el objeto arriba y abajo usando una onda seno
        float nuevoY = posInicial.y + Mathf.Sin(Time.time * velocidad) * altura;
        transform.position = new Vector3(transform.position.x, nuevoY, transform.position.z);
    }
}