using UnityEngine;

public class GhostTrail : MonoBehaviour
{
    public GameObject ghostPrefab;
    public float ghostDelay = 0.05f;
    private float timer;

    public Color[] ghostColors;

    private SpriteRenderer playerSR;

    void Start()
    {
        playerSR = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        timer -= Time.deltaTime;
    }

    // Llamar esta función cuando hagas dash
    public void SpawnGhost()
    {
        if (timer > 0) return;

        timer = ghostDelay;

        GameObject ghost = Instantiate(ghostPrefab, transform.position, Quaternion.identity);

        SpriteRenderer ghostSR = ghost.GetComponent<SpriteRenderer>();
        ghostSR.sprite = playerSR.sprite;        // copia sprite actual
        ghostSR.flipX = playerSR.flipX;          // copia flip
        ghostSR.sortingLayerID = playerSR.sortingLayerID;
        ghostSR.sortingOrder = playerSR.sortingOrder - 1;

        if (ghostColors.Length > 0)
        {
            ghostSR.color = ghostColors[Random.Range(0, ghostColors.Length)];
        }
        else
        {
            ghostSR.color = playerSR.color;
        }
    }
}
