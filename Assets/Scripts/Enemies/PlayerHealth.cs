using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Stats")]
    public float maxHP = 10f;
    private float currentHP;

    [Header("UI")]
    public Slider healthSlider; // opcional

    [Header("Hit Feedback")]
    public Color hitColor = Color.red;
    public float flashDuration = 0.1f;

    private SpriteRenderer spriteRenderer;
    private bool isInvulnerable = false;

    void Awake()
    {
        currentHP = maxHP;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHP;
            healthSlider.value = currentHP;
        }
    }

    public void TakeDamage(float amount)
    {
        TakeDamage(amount, Vector2.zero);
    }

    public void TakeDamage(float amount, Vector2 knockbackDirection)
    {
        if (isInvulnerable) return;

        currentHP -= amount;
        currentHP = Mathf.Max(currentHP, 0);

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null && knockbackDirection != Vector2.zero)
        {
            rb.AddForce(knockbackDirection * 5f, ForceMode2D.Impulse);
        }

        StartCoroutine(HitFlash());

        if (currentHP <= 0)
        {
            Die();
        }

        if (healthSlider != null)
            healthSlider.value = currentHP;
    }

    IEnumerator HitFlash()
    {
        isInvulnerable = true;
        if (spriteRenderer != null)
        {
            Color original = spriteRenderer.color;
            spriteRenderer.color = hitColor;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = original;
        }
        isInvulnerable = false;
    }

    void Die()
    {
        gameObject.SetActive(false);
        Debug.Log("Jugador muerto");
    }
}
