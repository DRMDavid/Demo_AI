/*******************************************************
 * NOMBRE DEL ARCHIVO: DamagePopup.cs
 * AUTOR: David Sanchez (Implementación)
 * * DESCRIPCIÓN: 
 * Controla la animación de los números de daño (Floating Text).
 * Se asegura de renderizarse por encima de los sprites en juegos 2D.
 * * REFERENCIA: 
 * - Floating Damage Text (CodeMonkey): https://www.youtube.com/watch?v=iD1_JczQcFY
 *******************************************************/

using UnityEngine;
using TMPro; // Necesario para TextMeshPro

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
        
        // CORRECCIÓN CRÍTICA PARA 2D:
        // Fuerza al texto a dibujarse en la capa 2000 para que nada lo tape.
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.sortingOrder = 2000; 
        }
    }

    public void Setup(int damageAmount)
    {
        textMesh.text = damageAmount.ToString();

        // Estilo Crítico vs Normal
        if (damageAmount > 10) 
        {
            textMesh.fontSize = 8;
            textMesh.color = new Color(1f, 0.2f, 0.2f); // Rojo Intenso
        }
        else
        {
            textMesh.fontSize = 5;
            textMesh.color = new Color(1f, 0.8f, 0f); // Amarillo
        }

        textColor = textMesh.color;
        disappearTimer = DISAPPEAR_TIMER_MAX;

        // Movimiento inicial aleatorio hacia los lados y arriba
        moveVector = new Vector3(Random.Range(-1f, 1f), 3f) * 8f; 
    }

    private void Update()
    {
        // Mover hacia arriba
        transform.position += moveVector * Time.deltaTime;
        
        // Simular gravedad (el texto sube rápido y luego frena)
        moveVector -= moveVector * 8f * Time.deltaTime;

        // Desvanecer (Fade Out)
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