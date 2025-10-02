/*
using System.Collections.Generic;
using UnityEngine;

public class EnemigoFSM : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private string estadoIDInicial;

    [Header("Estados")]
    public List<FSMEstado> estados; 

    public FSMEstado EstadoActual { get; private set; }
    public Room RoomParent { get; set; }
    public Transform Player { get; set; }
    
    private void Start()
    {
        CambiarEstado(estadoIDInicial);
    }

    private void Update()
    {
        if (EstadoActual == null) return;
        EstadoActual.EjecutarEstado(this); 
    }

    public void CambiarEstado(string nuevoEstadoID)
    {
        // Primer estado
        if (EstadoActual == null)
        {
            EstadoActual = BuscarEstado(nuevoEstadoID);
        }
        
        if (EstadoActual.EstadoID == nuevoEstadoID) return;
        EstadoActual = BuscarEstado(nuevoEstadoID);
    }

    private FSMEstado BuscarEstado(string estadoIDBuscado)
    {
        for (int i = 0; i < estados.Count; i++)
        {
            if (estados[i].EstadoID == estadoIDBuscado)
            {
                return estados[i];
            }
        }

        return null;
    }
}
*/