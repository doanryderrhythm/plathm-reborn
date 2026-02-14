using UnityEngine;

public class ValueStorer : MonoBehaviour
{
    public static float laneWidth = 3f;
    public static float gapWidth = 0.75f;

    public static float playerXMoveAbs = 3.75f;
    public static float playerYPosition = -0.375f;

    public static float maxXPos = laneWidth * 0.5f + playerXMoveAbs;

    public static Vector3 playerLeftPosition = new Vector3(-playerXMoveAbs, playerYPosition, 0);
    public static Vector3 playerMiddlePosition = new Vector3(0, playerYPosition, 0);
    public static Vector3 playerRightPosition = new Vector3(playerXMoveAbs, playerYPosition, 0);

    public static Vector3 leftLanePosition = new Vector3(-playerXMoveAbs, 0, 0);
    public static Vector3 middleLanePosition = new Vector3(0, 0, 0);
    public static Vector3 rightLanePosition = new Vector3(playerXMoveAbs, 0, 0);

    public static float minLeftLaneX = leftLanePosition.x - laneWidth * 0.5f;
    public static float maxLeftLaneX = leftLanePosition.x + laneWidth * 0.5f;

    public static float minMiddleLaneX = middleLanePosition.x - laneWidth * 0.5f;
    public static float maxMiddleLaneX = middleLanePosition.x + laneWidth * 0.5f;

    public static float minRightLaneX = rightLanePosition.x - laneWidth * 0.5f;
    public static float maxRightLaneX = rightLanePosition.x + laneWidth * 0.5f;

    public static float cPerfectJudgement = 0.04f;
    public static float perfectJudgement = 0.075f;
    public static float goodJudgement = 0.14f;

    //Note's width
    public static float tapWidth = 0.9f;
    public static float blackWidth = 0.8f;
    public static float teleportWidth = 2.6f;
    public static float sliceWidth = 12.0f;
    public static float spikeWidth = 3f;

    //Note's height
    public static float tapHeight = 0.9f;
    public static float blackHeight = 0.8f;
    public static float teleportHeight = 1.0f;
    public static float sliceHeight = 0.8f;
    public static float spikeHeight = 0.3f;

    public static Color32 gridDefaultColor = new Color32(180, 180, 180, 255);
    public static Color32 gridSelectedColor = new Color32(255, 255, 75, 255);
    public static float gridOffset = 0.2f;

    public static Color32 chosenNoteColor = new Color32(0, 255, 52, 255);

    public static string tagVerticalGrid = "Vertical Grid";
    public static string tagHorizontalGrid = "Horizontal Grid";
    public static string tagGrid = "Grid";

    public static string currentTimingText = "Current Timing: ";
    public static string chartOffsetText = "Chart Offset: ";
    public static string chartSpeedText = "Chart Speed: ";

    public static string triggerInfoOpen = "Chart Info Open";
    public static string triggerInfoClose = "Chart Info Close";

    public static string noTimingGroupText = "No Timing Groups";
    public static string hasTimingGroupText = "Timing Group ";

    public static string noSpeedGroupText = "No Speed Groups";
    public static string hasSpeedGroupText = "Speed Group ";

    public static float mouseScrollSpeed = 0.5f;

    //Difficulty Strings
    public static string difficultyText = "Current Difficulty: ";
    public static string pointText = "POINT";
    public static string lineText = "LINE";
    public static string triangleText = "TRIANGLE";
    public static string squareText = "SQUARE";

    //Save & Load Strings
    #region Project Information
    public const string songNameString = "song_name: ";
    public const string songArtistString = "song_artist: ";
    public const string charterNameString = "charter: ";
    public const string chartOffsetString = "offset: ";
    public const string chartSpeedString = "speed: ";
    #endregion

    #region Chart Timings
    public const string timingString = "timing(";
    #endregion

    #region Chart File
    public const string informationString = "\\information.ptminf";
    public const string difficultyString = "difficulty: ";
    public const string speedString = "speed(";
    #endregion

    #region Chart Difficulties
    public const string difficultyPoint = "\\0.ptmf";
    public const string difficultyLine = "\\1.ptmf";
    public const string difficultyTriangle = "\\2.ptmf";
    public const string difficultySquare = "\\3.ptmf";
    #endregion

    #region Note Types
    public const string tapString = "tap(";
    public const string blackString = "black(";
    public const string leftTeleportString = "left_tele(";
    public const string rightTeleportString = "right_tele(";
    public const string sliceString = "slice(";
    public const string middleSpikeString = "middle_spike(";
    public const string sideSpikeString = "side_spike(";
    #endregion
}