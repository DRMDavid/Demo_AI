using UnityEngine;

public class Movement : MonoBehaviour
{
    public float speed = 0.5f;
    private Rigidbody2D rb;
    private Vector2 input;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame - used for inputs and timers
    void Update()
    {
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");

        input.Normalize(); //Makes our diagonal movement move the same as other movements
                         //Without normalize - diagonal movement would be faster
    }

    //Called once per Physics frame - used for physics (we'll use for our movement)
    private void FixedUpdate()
    {
        rb.linearVelocity = input * speed;
    }
}