/*******************************************************
 * NOMBRE DEL ARCHIVO: DamagePopup.cs
 * AUTOR: Gael, Steve y David
 * DESCRIPCIÓN:
 * Controla el comportamiento visual del texto de daño flotante.
 * - Movimiento ascendente con gravedad simulada.
 * - Desvanecimiento gradual (Fade out).
 * - Ajuste de tamaño y color según cantidad de daño.
 *******************************************************/

using UnityEngine;
using TMPro; // Requerido para TextMeshPro

public class DamagePopup : MonoBehaviour
{
    private TextMeshPro textMesh;
    private float disappearTimer;
    private Color textColor;
    private Vector3 moveVector;

    private const float DISAPPEAR_TIMER_MAX = 1f;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
    }

    public void Setup(int damageAmount)
    {
        // Asigna el texto
        textMesh.text = damageAmount.ToString();

        // Lógica visual: Si el daño es alto (critico), cambia estilo
        if (damageAmount > 10) 
        {
            textMesh.fontSize = 8;
            textMesh.color = Color.red;
        }
        else
        {
            textMesh.fontSize = 5;
            textMesh.color = new Color(1f, 0.8f, 0f); // Naranja/Amarillo
        }

        textColor = textMesh.color;
        disappearTimer = DISAPPEAR_TIMER_MAX;

        // Vector de movimiento inicial: Sube rápido y se mueve un poco a los lados
        moveVector = new Vector3(Random.Range(-1f, 1f), 5f) * 5f; 
    }

    private void Update()
    {
        // Mover
        transform.position += moveVector * Time.deltaTime;
        
        // Simular gravedad (el texto frena su subida)
        moveVector -= moveVector * 8f * Time.deltaTime;

        // Desvanecer
        disappearTimer -= Time.deltaTime;
        if (disappearTimer < 0)
        {
            float disappearSpeed = 3f;
            textColor.a -= disappearSpeed * Time.deltaTime;
            textMesh.color = textColor;

            if (textColor.a < 0)
            {
                Destroy(gameObject);
            }
        }
    }
}