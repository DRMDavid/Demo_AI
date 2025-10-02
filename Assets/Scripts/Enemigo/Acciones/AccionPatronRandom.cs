/*
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class AccionPatronRandom : FSMAccion
{
    [Header("Config")]
    [SerializeField] private int cantidadProyectiles;
    [SerializeField] private float tiempoEntreAtaques;

    [Header("Velocidad Proyectil")]
    [SerializeField] private float velocidadRandomMin;
    [SerializeField] private float velocidadRandomMax;

    [Header("Tiempo entre Proyectiles")]
    [SerializeField] private float tiempoMin;
    [SerializeField] private float tiempoMax;

    private EnemigoPatrones enemigoPatrones;
    private float timer;
    
    private void Awake()
    {
        enemigoPatrones = GetComponent<EnemigoPatrones>();
    }

    private void Start()
    {
        timer = tiempoEntreAtaques;
    }

    public override void EjecutarAccion()
    {
        Disparar();
    }

    private void Disparar()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            timer = tiempoEntreAtaques;
            StartCoroutine(IERandom());
        }
    }

    private IEnumerator IERandom()
    {
        for (int i = 0; i < cantidadProyectiles; i++)
        {
            // Velocidad
            float velocidadRandom = Random.Range(velocidadRandomMin, velocidadRandomMax);

            // Direccion Random
            Vector3 randomDir = Random.insideUnitCircle.normalized;
            
            // Aplicamos
            Proyectil proyectil = enemigoPatrones.ObtenerProyectil();
            proyectil.Velocidad = velocidadRandom;
            proyectil.Direccion = randomDir;
            yield return new WaitForSeconds(Random.Range(tiempoMin, tiempoMax));
        }
    }
}*/