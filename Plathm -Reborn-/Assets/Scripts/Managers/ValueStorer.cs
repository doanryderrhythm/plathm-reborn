using UnityEngine;

public class ValueStorer : MonoBehaviour
{
    public static float laneWidth = 3f;

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
}
