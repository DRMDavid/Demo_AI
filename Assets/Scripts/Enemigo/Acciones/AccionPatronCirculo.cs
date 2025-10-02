/*
using UnityEngine;

public class AccionPatronCirculo : FSMAccion
{
    [Header("Config")]
    [SerializeField] private int numeroProyectiles;
    [SerializeField] private float tiempoEntreAtaques;

    private EnemigoPatrones enemigoPatrones;
    private float contadorAtaque;
    
    private void Awake()
    {
        enemigoPatrones = GetComponent<EnemigoPatrones>();
    }

    private void Start()
    {
        contadorAtaque = tiempoEntreAtaques;
    }

    public override void EjecutarAccion()
    {
        Disparar();
    }

    private void Disparar()
    {
        contadorAtaque -= Time.deltaTime;
        if (contadorAtaque <= 0)
        {
            float angulo = 360 / numeroProyectiles;
            for (int i = 0; i < numeroProyectiles; i++)
            {
                float nuevoAngulo = angulo * i;
                Proyectil proyectil = enemigoPatrones.ObtenerProyectil();
                proyectil.transform.rotation = 
                    Quaternion.Euler(new Vector3(0f, 0f, nuevoAngulo));
            }

            contadorAtaque = tiempoEntreAtaques;
        }
    }
}*/