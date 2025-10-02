using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemTexto : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private TextMeshProUGUI texto;

    public void EstablecerTexto(string mensaje, Color color)
    {
        texto.text = mensaje;
        texto.color = color;
    }
}
