// Arma.cs (versión sin ScriptableObject)
using UnityEngine;

public abstract class Arma : MonoBehaviour
{
    [Header("Configuración Básica")]
    public int Damage = 2;
    public float cadencia = 0.5f; // segundos entre ataques

    protected float ultimoAtaque;
    protected Transform puntoDisparo; // opcional para melee

    public virtual void Inicializar(Transform puntoDisparo = null)
    {
        this.puntoDisparo = puntoDisparo;
    }

    public virtual bool PuedeAtacar()
    {
        return Time.time >= ultimoAtaque + cadencia;
    }

    public abstract void Atacar();
}