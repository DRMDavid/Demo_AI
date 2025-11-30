using UnityEngine;

public class BossGhost : MonoBehaviour
{
    public float fadeSpeed = 3f;
    private SpriteRenderer sr;
    private Color color;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        color = sr.color;
    }

    void Update()
    {
        color.a -= fadeSpeed * Time.deltaTime;
        sr.color = color;

        if (color.a <= 0)
            Destroy(gameObject);
    }
}
