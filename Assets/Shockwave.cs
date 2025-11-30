using UnityEngine;

public class BossShockwave : MonoBehaviour
{
    public float growSpeed = 6f;
    public float fadeSpeed = 2f;


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
