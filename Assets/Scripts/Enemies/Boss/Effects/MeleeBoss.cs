using UnityEngine;

public class BossMeleeImpact : MonoBehaviour
{
    public float growSpeed = 4f;
    public float fadeSpeed = 6f;

private SpriteRenderer sr;
    private Color color;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        color = sr.color;
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
