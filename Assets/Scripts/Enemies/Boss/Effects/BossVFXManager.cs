using System.Collections;
using UnityEngine;

public class BossVFXManager : MonoBehaviour
{
    [Header("REFERENCIAS VISUALES (Arrastra el 'Square' aquí)")]
    public Transform visualTransform;    // Arrastra el objeto 'Square' aquí
    public SpriteRenderer visualSprite;  // Arrastra el 'Square' aquí también

    [Header("PREFABS DE VFX DEL BOSS (Tus prefabs)")]
    public GameObject ghostPrefab;
    public GameObject meleeImpactPrefab;
    public GameObject shootFlashPrefab;
    public GameObject shockwavePrefab;
    public GameObject chargePrefab;
    public GameObject burstPrefab;

    [Header("AJUSTES")]
    public float ghostInterval = 0.06f;

    // Variables privadas para recordar el estado original
    private Vector3 _originalScale;
    private Color _originalColor;

    void Start()
    {
        // Guardamos la escala y color iniciales para poder resetearlos después
        if (visualTransform != null) _originalScale = visualTransform.localScale;
        if (visualSprite != null) _originalColor = visualSprite.color;
    }

    // =========================================================
    // FUNCIONES PARA EFECTOS DE CÓDIGO (Juice)
    // =========================================================

    // 1. SQUASH & STRETCH (Deformar)
    public void TriggerSquash(float xForce, float yForce, float duration)
    {
        if (visualTransform == null) return;
        // Quité StopAllCoroutines() para que no te cancele el Flash o el Shake si ocurren al mismo tiempo
        StartCoroutine(SquashRoutine(xForce, yForce, duration));
    }

    private IEnumerator SquashRoutine(float xStr, float yStr, float time)
    {
        float elapsed = 0f;
        while (elapsed < time)
        {
            float t = elapsed / time;
            // Curva suave (sube y baja)
            float curve = Mathf.Sin(t * Mathf.PI);

            float newX = _originalScale.x + (xStr * curve);
            float newY = _originalScale.y + (yStr * curve);

            visualTransform.localScale = new Vector3(newX, newY, _originalScale.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        visualTransform.localScale = _originalScale; // Reset obligatorio
    }

    // 2. FLASH (Cambio de color)
    public void TriggerFlash(Color color, float duration)
    {
        if (visualSprite == null) return;
        StartCoroutine(FlashRoutine(color, duration));
    }

    private IEnumerator FlashRoutine(Color color, float duration)
    {
        visualSprite.color = color;
        yield return new WaitForSeconds(duration);
        visualSprite.color = _originalColor;
    }

    // 3. SHAKE (Vibración)
    public void TriggerShake(float intensity, float duration)
    {
        if (visualTransform == null) return;
        StartCoroutine(ShakeRoutine(intensity, duration));
    }

    private IEnumerator ShakeRoutine(float intensity, float duration)
    {
        Vector3 originalPos = visualTransform.localPosition;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;
            visualTransform.localPosition = originalPos + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }
        visualTransform.localPosition = originalPos;
    }

    // =========================================================
    // FUNCIONES PARA TUS PREFABS
    // =========================================================

    // CAMBIO IMPORTANTE: Ahora devuelve GameObject en lugar de void
    public GameObject SpawnVisualEffect(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab != null)
        {
            return Instantiate(prefab, position, rotation);
        }
        return null;
    }

    // Lógica específica para el Fantasma (Dash)
    public IEnumerator PlayDashGhostTrail(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (ghostPrefab != null && visualTransform != null)
            {
                GameObject ghost = Instantiate(ghostPrefab, visualTransform.position, visualTransform.rotation);
                ghost.transform.localScale = visualTransform.localScale;

                // Si el fantasma tiene SpriteRenderer, copiamos el sprite actual del boss
                SpriteRenderer sr = ghost.GetComponent<SpriteRenderer>();
                if (sr != null && visualSprite != null) sr.sprite = visualSprite.sprite;
            }
            yield return new WaitForSeconds(ghostInterval);
            elapsed += ghostInterval;
        }
    }
}