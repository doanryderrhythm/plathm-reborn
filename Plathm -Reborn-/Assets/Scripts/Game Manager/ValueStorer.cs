using UnityEngine;

public class ValueStorer : MonoBehaviour
{
    #region PLAYER
    public static float moveSpeed = 10f;
    public static float jumpHeight = 27f;
    public static float lightPush = 9.5f;

    public static int maxJumpCount = 2;

    public static float sizeRadiusStill = 0f;
    public static float sizeRadiusMoving = 0.2f;

    public static Vector2 colliderSizeStill = new Vector2(1f, 1f);
    public static Vector2 colliderSizeMoving = new Vector2(0.5f, 0.5f);

    public static float playerRespawnTime = 0.75f;
    #endregion

    #region LAYER MASKS
    public const string groundLM = "Default";
    #endregion

    #region VALUES
    public static float coyoteTime = 0.06f;
    public static float bufferTime = 0.12f;
    #endregion
}
