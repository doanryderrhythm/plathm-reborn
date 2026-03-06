using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] float speed;

    [SerializeField] GameObject platform;
    public Rigidbody2D platformRB;
    [SerializeField] Transform[] wayPoints;

    private int targetIndex;
    private Vector3 lastPosition;

    public float affectXSpeed;

    private void Start()
    {
        targetIndex = 0;

        platformRB = platform.GetComponent<Rigidbody2D>();
        lastPosition = platformRB.position;
    }

    private void FixedUpdate()
    {
        platformRB.MovePosition(Vector2.MoveTowards(
            platformRB.position, 
            wayPoints[targetIndex].position, 
            speed * Time.fixedDeltaTime));

        affectXSpeed = (platformRB.position.x - lastPosition.x) / Time.fixedDeltaTime;
        lastPosition = platformRB.position;

        if (Vector2.Distance(platformRB.position, wayPoints[targetIndex].position) < 0.05f)
        {
            if (targetIndex < wayPoints.Length - 1)
            {
                targetIndex += 1;
            }
            else
            {
                targetIndex = 0;
            }
        }
    }
}
