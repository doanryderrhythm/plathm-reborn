using UnityEngine;

public class ValueStorer : MonoBehaviour
{
    #region PLATFORMER PLAYER
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
    public const string transparentLM = "TransparentFX";
    public const string playerLM = "Player";
    #endregion

    #region VALUES
    public static float coyoteTime = 0.06f;
    public static float bufferTime = 0.12f;

    public static float cameraXAbsolute = 3f;
    public static float cameraOffsetMove = 2.5f;
    public static float cameraDirectionMove = 2f;

    public static float bombWarningAlpha = 100f / 255f;

    public static float tankWarningAlpha = 80f / 255f;

    public const string songInfoTitle = "SONG INFO";
    public const string configurationTitle = "CONFIGURATION";

    public const string pointText = "POINT";
    public const string lineText = "LINE";
    public const string triangleText = "TRIANGLE";
    public const string squareText = "SQUARE";
    #endregion

    #region COLORS
    public static Color32 checkpointUntoggled = Color.yellow;
    public static Color32 checkpointToggled = Color.green;

    public static Color32 toggledMinParticles = new Color32(0, 78, 0, 255);
    public static Color32 toggledMaxParticles = new Color32(0, 255, 0, 255);

    public static Color32 pointDifficultyColor = new Color32(30, 255, 0, 255);
    public static Color32 lineDifficultyColor = new Color32(255, 227, 0, 255);
    public static Color32 triangleDifficultyColor = new Color32(255, 0, 0, 255);
    public static Color32 squareDifficultyColor = new Color32(183, 0, 255, 255);

    public static Color32 pointDifficultyBackground = new Color32(0, 24, 0, 255);
    public static Color32 lineDifficultyBackground = new Color32(24, 24, 0, 255);
    public static Color32 triangleDifficultyBackground = new Color32(24, 0, 0, 255);
    public static Color32 squareDifficultyBackground = new Color32(24, 0, 24, 255);
    #endregion

    #region LANE PROPERTIES
    public static float laneWidth = 3f;
    public static float gapWidth = 0.75f;

    public static float playerXMoveAbs = 3.75f;
    public static float playerYPosition = -0.375f;

    public static Vector3 leftLanePosition = new Vector3(-playerXMoveAbs, 0, 0);
    public static Vector3 middleLanePosition = new Vector3(0, 0, 0);
    public static Vector3 rightLanePosition = new Vector3(playerXMoveAbs, 0, 0);
    #endregion

    #region RHYTHM GAME PLAYER
    public static Vector3 playerLeftPosition = new Vector3(-playerXMoveAbs, playerYPosition, 0);
    public static Vector3 playerMiddlePosition = new Vector3(0, playerYPosition, 0);
    public static Vector3 playerRightPosition = new Vector3(playerXMoveAbs, playerYPosition, 0);
    #endregion

    #region CHART FILE
    public const string songNameString = "song_name: ";
    public const string songArtistString = "song_artist: ";
    public const string charterNameString = "charter: ";

    public const string informationString = "\\information.ptminf";
    public const string playerPositionString = "position: ";
    public const string playerLeftString = "left";
    public const string playerMiddleString = "middle";
    public const string playerRightString = "right";
    public const string difficultyString = "difficulty: ";
    public const string speedString = "speed(";
    #endregion

    #region NOTE TYPES
    public const string tapString = "tap(";
    public const string blackString = "black(";
    public const string leftTeleportString = "left_tele(";
    public const string rightTeleportString = "right_tele(";
    public const string sliceString = "slice(";
    public const string middleSpikeString = "middle_spike(";
    public const string sideSpikeString = "side_spike(";
    #endregion

    #region JUDGEMENTS
    public static float cPerfectJudgement = 0.04f;
    public static float perfectJudgement = 0.075f;
    public static float goodJudgement = 0.14f;
    #endregion

    #region PLAYERPREFS
    public const string prefsChartSpeed = "Chart Speed";
    public const string prefsChartOffset = "Chart Offset";
    public const string prefsIsMirror = "Is Mirrored";
    #endregion
}
