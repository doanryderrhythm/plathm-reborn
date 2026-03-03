using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlatformerPlayer : MonoBehaviour
{
    [SerializeField] InputActionReference playerRunInput;
    [SerializeField] InputActionReference playerJumpInput;

    void OnEnable()
    {
        playerRunInput.action.Enable();
        playerJumpInput.action.Enable();
    }

    void OnDisable()
    {
        playerRunInput.action.Disable();
        playerJumpInput.action.Disable();
    }

    private CameraController cam;

    [SerializeField] Rigidbody2D rb;
    [SerializeField] BoxCollider2D bc;
    private int jumpCount = 2;
    private bool wasGrounded = false;

    //Landing Detection
    [SerializeField] private float maxDistance = 0.1f;
    [SerializeField] Collider2D boxCol;
    private RaycastHit2D hit;
    private bool hitDetected;
    private bool leaveDetected;

    //Buffer
    private float jumpBufferTime;
    private float coyoteTime;

    [SerializeField] GameObject jumpLighting;

    void ManageMove()
    {
        float moveRate = playerRunInput.action.ReadValue<float>();
        rb.linearVelocityX = moveRate * ValueStorer.moveSpeed;

        cam.MoveCameraOffset(moveRate);

        ChangeColliderShape(moveRate);
    }

    void ManageLeave()
    {
        if (!hitDetected && !leaveDetected)
        {
            coyoteTime -= Time.deltaTime;
            if (coyoteTime <= 0)
            {
                jumpCount -= 1;
                leaveDetected = true;
            }
        }
    }

    void ManageJump()
    {
        if (playerJumpInput.action.WasPressedThisFrame())
        {
            jumpBufferTime = ValueStorer.bufferTime;
        }
        else
        {
            jumpBufferTime -= Time.deltaTime;
        }

        if (jumpBufferTime >= 0 && jumpCount > 0)
        {
            Instantiate(jumpLighting, transform.position, Quaternion.identity);

            rb.linearVelocityY = 0;
            rb.AddForceY(ValueStorer.jumpHeight, ForceMode2D.Impulse);
            jumpCount -= 1;
            leaveDetected = true;

            jumpBufferTime = 0;
        }
        else if (playerJumpInput.action.WasReleasedThisFrame() && rb.linearVelocityY > 0)
        {
            rb.AddForceY(-ValueStorer.lightPush, ForceMode2D.Impulse);
        }
    }

    void ManageLand()
    {
        Vector2 halfExtents = boxCol.bounds.extents;
        Vector2 origin = boxCol.bounds.center;
        Vector2 direction = Vector2.down;

        hit = Physics2D.BoxCast(origin, halfExtents, 0f, Vector2.down, maxDistance, LayerMask.GetMask(ValueStorer.groundLM));

        hitDetected = hit.collider != null;

        if (hitDetected && !wasGrounded)
        {
            coyoteTime = ValueStorer.coyoteTime;

            leaveDetected = false;
            jumpCount = ValueStorer.maxJumpCount;
        }

        wasGrounded = hitDetected;
    }

    void ChangeColliderShape(float moveRate)
    {
        if (moveRate != 0f)
        {
            bc.edgeRadius = ValueStorer.sizeRadiusMoving;
            bc.size = ValueStorer.colliderSizeMoving;
        }
        else
        {
            bc.edgeRadius = ValueStorer.sizeRadiusStill;
            bc.size = ValueStorer.colliderSizeStill;
        }
    }

    private bool isDead = false;
    [SerializeField] GameObject deathLighting;

    void Awake()
    {
        jumpCount = ValueStorer.maxJumpCount;

        jumpBufferTime = 0;
        coyoteTime = ValueStorer.coyoteTime;

        cam = GameObject.FindFirstObjectByType<CameraController>();
    }

    void Start()
    {
        
    }

    void Update()
    {
        ManageMove();
        ManageJump();

        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            DestroyPlayer();
        }
    }

    void FixedUpdate()
    {
        ManageLeave();
        ManageLand();
    }

    void DestroyPlayer()
    {
        isDead = true;
        Instantiate(deathLighting, transform.position, Quaternion.identity);
        GameManager.Instance.StartCoroutine(GameManager.Instance.RespawnPlayer());
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead)
        {
            return;
        }

        DestroyPlayer();
    }
}
