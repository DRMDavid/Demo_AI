/*
using System;
using System.Collections.Generic;

[Serializable]
public class FSMEstado
{
    public string EstadoID;
    public List<FSMAccion> Acciones;
    public List<FSMTransicion> Transiciones;

    public void EjecutarEstado(EnemigoFSM enemigoFSM)
    {
        EjecutarAcciones();
        EjecutarTransiciones(enemigoFSM);
    }

    private void EjecutarAcciones()
    {
        if (Acciones.Count <= 0) return;
        for (int i = 0; i < Acciones.Count; i++)
        {
            Acciones[i].EjecutarAccion();
        }
    }

    private void EjecutarTransiciones(EnemigoFSM enemigoFSM)
    {
        if (Transiciones.Count <= 0) return;
        for (int i = 0; i < Transiciones.Count; i++)
        {
            bool respuesta = Transiciones[i].Decision.Decidir(enemigoFSM);
            if (respuesta)
            {
                if (string.IsNullOrEmpty(Transiciones[i].EstadoTrue) == false)
                {
                    enemigoFSM.CambiarEstado(Transiciones[i].EstadoTrue);
                    break;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(Transiciones[i].EstadoFalse) == false)
                {
                    enemigoFSM.CambiarEstado(Transiciones[i].EstadoFalse);
                    break;
                }
            }
        }
    }
}
*/