using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DañoTexto : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private TextMeshProUGUI dañoTexto;

    public Transform Parent { get; set; }

    public void EstablecerDaño(float valor)
    {
        dañoTexto.text = valor.ToString();
    }

    public void DestruirTexto()
    {
        Destroy(gameObject);
    }
}
