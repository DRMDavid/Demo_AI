/*
Este código fue basado en el video de YouTube
Enlace: https://youtu.be/3Nl8UPyODgQ?si=UISpmu5yan8PTVtU
Integrantes del Equipo:
Hannin Abarca
Gael Jimenez
David Sanchez
*/
using System.Collections;
using UnityEngine;

// Clase encargada de gestionar todos los efectos visuales (VFX) y efectos de "jugo" (Juice)
// del jefe final, como vibración, destellos de color, deformación y la instanciación de prefabs.
public class BossVFXManager : MonoBehaviour
{
    [Header("REFERENCIAS VISUALES")]
    // Objeto que se va a manipular (mover, escalar, etc.). Generalmente el padre del sprite.
    public Transform visualTransform;    // Arrastra el objeto 'Square' aquí
    // El componente SpriteRenderer para cambiar el color, el sprite o la transparencia.
    public SpriteRenderer visualSprite;  // Arrastra el 'Square' aquí también

    [Header("PREFABS DE VFX DEL BOSS (Tus prefabs)")]
    public GameObject ghostPrefab;      // El rastro visual que deja al moverse rápidamente (Dash).
    public GameObject meleeImpactPrefab; // Partículas o animación al golpear cuerpo a cuerpo.
    public GameObject shootFlashPrefab;  // Destello o efecto visual al disparar.
    public GameObject shockwavePrefab;   // Efecto de onda expansiva.
    public GameObject chargePrefab;      // Partículas o aviso al cargar un ataque.
    public GameObject burstPrefab;       // Efecto de explosión o liberación de energía.

    [Header("AJUSTES DE EFECTOS")]
    // Intervalo de tiempo entre la instanciación de cada fantasma en el rastro (Ghost Trail).
    public float ghostInterval = 0.06f;

    // Variables privadas para recordar el estado original
    private Vector3 _originalScale; // Guarda la escala inicial para restaurar la deformación.
    private Color _originalColor;   // Guarda el color inicial para restaurar el destello (Flash).

    void Start()
    {
        // Guardamos la escala y color iniciales para poder resetearlos después de los efectos.
        if (visualTransform != null) _originalScale = visualTransform.localScale;
        if (visualSprite != null) _originalColor = visualSprite.color;
    }

    // =========================================================
    // FUNCIONES PARA EFECTOS DE CÓDIGO (Juice)
    // =========================================================

    // 1. SQUASH & STRETCH (Deformar)
    // Inicia la corrutina de deformación para simular impacto o anticipación.
    public void TriggerSquash(float xForce, float yForce, float duration)
    {
        if (visualTransform == null) return;
        // Quitamos StopAllCoroutines() para que no cancele otros efectos (como Flash o Shake)
        // si ocurren al mismo tiempo.
        StartCoroutine(SquashRoutine(xForce, yForce, duration));
    }

    // Corrutina que gestiona la deformación visual.
    private IEnumerator SquashRoutine(float xStr, float yStr, float time)
    {
        float elapsed = 0f;
        while (elapsed < time)
        {
            // Calcula el progreso del tiempo (0 a 1).
            float t = elapsed / time;
            // Curva suave (Función Seno) que inicia en 0, sube a 1 y baja a 0, creando el efecto de "rebote".
            float curve = Mathf.Sin(t * Mathf.PI);

            // Calcula la nueva escala inyectando la fuerza de deformación (xStr o yStr).
            float newX = _originalScale.x + (xStr * curve);
            float newY = _originalScale.y + (yStr * curve);

            // Aplica la nueva escala.
            visualTransform.localScale = new Vector3(newX, newY, _originalScale.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        // Reset obligatorio: asegura que la escala del objeto vuelve a su estado original al terminar.
        visualTransform.localScale = _originalScale; 
    }

    // 2. FLASH (Cambio de color)
    // Inicia la corrutina para cambiar el color del sprite momentáneamente (usado para indicar daño o invulnerabilidad).
    public void TriggerFlash(Color color, float duration)
    {
        if (visualSprite == null) return;
        StartCoroutine(FlashRoutine(color, duration));
    }

    // Corrutina que cambia el color y espera antes de restaurarlo.
    private IEnumerator FlashRoutine(Color color, float duration)
    {
        // Aplica el color de destello (ej. blanco o rojo).
        visualSprite.color = color;
        // Espera la duración definida.
        yield return new WaitForSeconds(duration);
        // Restaura el color original del sprite.
        visualSprite.color = _originalColor;
    }

    // 3. SHAKE (Vibración)
    // Inicia la corrutina de vibración (usado para indicar impactos o ataques poderosos).
    public void TriggerShake(float intensity, float duration)
    {
        if (visualTransform == null) return;
        StartCoroutine(ShakeRoutine(intensity, duration));
    }

    // Corrutina que mueve la posición local del objeto aleatoriamente.
    private IEnumerator ShakeRoutine(float intensity, float duration)
    {
        // Guarda la posición inicial para que la vibración sea local.
        Vector3 originalPos = visualTransform.localPosition;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            // Genera offsets X e Y aleatorios dentro del rango de intensidad.
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;
            
            // Aplica la nueva posición (Original + Offset).
            visualTransform.localPosition = originalPos + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }
        // Restaura la posición local original para detener la vibración sin desplazamientos permanentes.
        visualTransform.localPosition = originalPos;
    }

    // =========================================================
    // FUNCIONES PARA TUS PREFABS
    // =========================================================

    // Función genérica para instanciar cualquier efecto visual (partículas, etc.).
    // Devuelve el GameObject instanciado, útil si se necesita manipularlo después.
    public GameObject SpawnVisualEffect(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab != null)
        {
            return Instantiate(prefab, position, rotation);
        }
        return null;
    }

    // Lógica específica para el Fantasma (Dash)
    // Corrutina que genera un rastro de clones (fantasmas) del jefe durante una acción.
    public IEnumerator PlayDashGhostTrail(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (ghostPrefab != null && visualTransform != null)
            {
                // Instancia el fantasma en la posición y rotación actual del objeto visual.
                GameObject ghost = Instantiate(ghostPrefab, visualTransform.position, visualTransform.rotation);
                
                // Copia la escala actual (importante si el jefe está girado o deformado).
                ghost.transform.localScale = visualTransform.localScale;

                // Si el fantasma tiene SpriteRenderer, copiamos el sprite actual del jefe
                SpriteRenderer sr = ghost.GetComponent<SpriteRenderer>();
                if (sr != null && visualSprite != null) 
                {
                    // Esto asegura que el fantasma use el frame actual del jefe si es una animación.
                    sr.sprite = visualSprite.sprite;
                }
            }
            // Espera el intervalo antes de crear el siguiente fantasma.
            yield return new WaitForSeconds(ghostInterval);
            elapsed += ghostInterval;
        }
    }
}