using UnityEngine;

public class BossChargeAura : MonoBehaviour
{
    public float pulsateSpeed = 3f;
    public float pulsateAmount = 0.15f;

private float baseScale;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale.x;
    }

    void Update()
    {
        float s = baseScale + Mathf.Sin(Time.time * pulsateSpeed) * pulsateAmount;
        transform.localScale = new Vector3(s, s, 1);
    }

}
