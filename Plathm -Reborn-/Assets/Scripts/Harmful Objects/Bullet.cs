using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    public float speed;
    public float shootAngle;

    private Rigidbody2D rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(speed * Mathf.Cos(shootAngle * Mathf.Deg2Rad), speed * Mathf.Sin(shootAngle * Mathf.Deg2Rad)); 
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Destroy(gameObject);
    }
}
