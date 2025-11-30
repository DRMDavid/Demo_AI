using UnityEngine;

public class BossShootFlash : MonoBehaviour
{
    public float shrinkSpeed = 4f;
    public float fadeSpeed = 4f;

private SpriteRenderer sr;
    private Color color;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        color = sr.color;
    }

    void Update()
    {
        transform.localScale -= Vector3.one * shrinkSpeed * Time.deltaTime;

        color.a -= fadeSpeed * Time.deltaTime;
        sr.color = color;

        if (color.a <= 0)
            Destroy(gameObject);
    }

}
