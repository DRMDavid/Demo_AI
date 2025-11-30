using UnityEngine;

public class Ghost : MonoBehaviour
{
    private SpriteRenderer sr;
    private float alpha;
    public float fadeSpeed = 5f;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        alpha = 1f;
    }

    void Update()
    {
        alpha -= fadeSpeed * Time.deltaTime;

        if (sr != null)
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }

        if (alpha <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
