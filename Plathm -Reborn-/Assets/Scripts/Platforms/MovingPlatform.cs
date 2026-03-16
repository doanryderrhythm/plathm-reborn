using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] float speed;

    [SerializeField] GameObject platform;
    public Rigidbody2D platformRB;
    [SerializeField] Transform[] wayPoints;

    private int targetIndex;
    private Vector2 newPosition;

    public Transform topPosition;

    public float affectXSpeed;

    public bool isTrigger = false;
    public bool isControl = false;

    private void Start()
    {
        targetIndex = 1;

        platformRB = platform.GetComponent<Rigidbody2D>();
        newPosition = transform.position;
    }

    private void FixedUpdate()
    {
        if ((isTrigger && isControl) || (!isTrigger))
        {
            newPosition = Vector2.MoveTowards(
                platformRB.position,
                wayPoints[targetIndex].position,
                speed * Time.fixedDeltaTime
            );
        }

        Vector2 velocity = (newPosition - platformRB.position) / Time.fixedDeltaTime;

        platformRB.MovePosition(newPosition);

        affectXSpeed = velocity.x;
        platformRB.linearVelocity = velocity;

        if (Vector2.Distance(platformRB.position, wayPoints[targetIndex].position) < 0.05f)
        {
            if (targetIndex < wayPoints.Length - 1)
            {
                if (targetIndex == 0 && isControl && isTrigger)
                {
                    isControl = false;
                }
                targetIndex += 1;
            }
            else
            {
                targetIndex = 0;
            }
        }
    }

    public void ResetPosition()
    {
        targetIndex = 1;
        platform.transform.position = wayPoints[0].position;
        isControl = false;
        Debug.Log("RESET");
    }
}
