/*
using UnityEngine;

public class RecogerItem : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private ItemData item;

    private PlayerAcciones acciones;
    private bool podemosInteractuar;
    private ItemTexto textoCreado;
    
    private void Awake()
    {
        acciones = new PlayerAcciones();
    }

    private void Start()
    {
        acciones.Interacciones.Recoger.performed += ctx => Recoger();
    }

    private void Recoger()
    {
        if (podemosInteractuar)
        {
            item.Recoger();
            Destroy(gameObject);
        }
    }

    private void MostrarNombre()
    {
        Vector3 posTexto = new Vector3(0f, 1f, 0f);
        if (item is ItemArma arma)
        {
            Color colorArma = GameManager.Instance.ObtenerColorArma(arma.Rareza);
            textoCreado = ItemTextoManager.Instance.MostrarMensaje(arma.ID, 
                transform.position + posTexto, colorArma);
        }
        else
        {
            textoCreado = ItemTextoManager.Instance.MostrarMensaje(item.ID, 
                transform.position + posTexto, Color.white);
        }
    }

    private void OcultarNombre()
    {
        Destroy(textoCreado.gameObject);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            podemosInteractuar = true;
            MostrarNombre();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            podemosInteractuar = false;
            OcultarNombre();
        }
    }

    private void OnEnable()
    {
        acciones.Enable();
    }

    private void OnDisable()
    {
        acciones.Disable();
    }
}*/