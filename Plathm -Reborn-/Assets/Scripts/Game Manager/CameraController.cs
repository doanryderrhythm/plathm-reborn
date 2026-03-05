using UnityEngine;

public class CameraController : MonoBehaviour
{
    public PlatformerPlayer player;

    private Vector3 offset;
    private Vector3 velocity = Vector3.zero;
    [SerializeField] float smoothTime = 0.1f;

    float xOffset = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindFirstObjectByType<PlatformerPlayer>();
            return;
        }

        offset = transform.position - player.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LateUpdate()
    {
        if (player == null) return;

        Vector3 targetPosition = player.transform.position + offset + new Vector3(xOffset, 0, 0);

        Vector3 confirmedPosition = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothTime
        );
        transform.position = new Vector3(confirmedPosition.x, confirmedPosition.y, -10);
    }

    public void MoveCameraOffset(float moveMulti)
    {
        float confirmedMulti = moveMulti;
        if ((xOffset > 0 && moveMulti < 0) || (xOffset < 0 && moveMulti > 0))
        {
            confirmedMulti *= ValueStorer.cameraDirectionMove;
        }
        xOffset += (confirmedMulti * ValueStorer.cameraOffsetMove * Time.deltaTime);
        if (xOffset <= -ValueStorer.cameraXAbsolute)
        {
            xOffset = -ValueStorer.cameraXAbsolute;
        }
        else if (xOffset >= ValueStorer.cameraXAbsolute)
        {
            xOffset = ValueStorer.cameraXAbsolute;
        }
    }

    public void ResetCameraOffset()
    {
        xOffset = 0;
    }
}
