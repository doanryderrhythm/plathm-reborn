using UnityEngine;

public class ValueStorer : MonoBehaviour
{
    #region PLAYER
    public static float moveSpeed = 10f;
    public static float jumpHeight = 27f;
    public static float lightPush = 9.5f;
    public static float gravityGround = 10f;
    public static float gravityMove = 50f;

    public static int maxJumpCount = 2;

    public static float sizeRadiusStill = 0f;
    public static float sizeRadiusMoving = 0.2f;

    public static Vector2 colliderSizeStill = new Vector2(1f, 1f);
    public static Vector2 colliderSizeMoving = new Vector2(0.5f, 0.5f);

    public static float playerRespawnTime = 0.75f;
    #endregion

    #region LAYER MASKS
    public const string groundLM = "Default";
    public const string checkpointLM = "Checkpoint";
    public const string harmfulLM = "Harmful";
    public const string movingPlatformLM = "Moving Platform";
    #endregion

    #region VALUES
    public static float coyoteTime = 0.06f;
    public static float bufferTime = 0.12f;

    public static float cameraXAbsolute = 3f;
    public static float cameraOffsetMove = 2.5f;
    public static float cameraDirectionMove = 2f;

    public static float bombWarningAlpha = 100f / 255f;
    #endregion

    #region COLORS
    public static Color32 checkpointUntoggled = Color.yellow;
    public static Color32 checkpointToggled = Color.green;

    public static Color32 toggledMinParticles = new Color32(0, 78, 0, 255);
    public static Color32 toggledMaxParticles = new Color32(0, 255, 0, 255);
    #endregion
}
