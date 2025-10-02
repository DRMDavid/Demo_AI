/*
using UnityEngine;

public class Proyectil : MonoBehaviour
{
    [Header("Config")] 
    [SerializeField] private float velocidad;

    public float Velocidad { get; set; }
    public Vector3 Direccion { get; set; }
    public float Daño { get; set; }

    private void Start()
    {
        Velocidad = velocidad;
    }

    private void Update()
    {
        transform.Translate(Direccion * (Velocidad * Time.deltaTime));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        IRecibirDaño entidad = other.GetComponent<IRecibirDaño>();
        if (entidad != null)
        {
            entidad.RecibirDaño(Daño);
            DañoTextoManager.Instance.MostrarDaño(Daño, other.transform);
        }
        
        Destroy(gameObject);
    }
}
*/