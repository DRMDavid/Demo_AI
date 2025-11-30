using UnityEngine;

public class BossUltimateBurst : MonoBehaviour
{
    public float growSpeed = 8f;
    public float fadeSpeed = 5f;

private SpriteRenderer sr;
    private Color color;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        color = sr.color * 1.5f;
    }

    void Update()
    {
        transform.localScale += Vector3.one * growSpeed * Time.deltaTime;

        color.a -= fadeSpeed * Time.deltaTime;
        sr.color = color;

        if (color.a <= 0)
            Destroy(gameObject);
    }

}
