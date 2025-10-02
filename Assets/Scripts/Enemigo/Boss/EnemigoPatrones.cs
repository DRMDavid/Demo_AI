/*
using UnityEngine;

public class EnemigoPatrones : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private Proyectil prefab;
    [SerializeField] private float daño;

    public Proyectil ObtenerProyectil()
    {
        Proyectil proyectil = Instantiate(prefab);
        proyectil.transform.position = transform.position;
        proyectil.Daño = daño;
        proyectil.Direccion = Vector3.right;
        return proyectil;
    }
}
*/