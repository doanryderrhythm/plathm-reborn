using UnityEngine;

public class ValueStorer : MonoBehaviour
{
    public static float playerXMoveAbs = 3.75f;
    public static float playerYPosition = -0.375f;

    public static Vector3 playerLeftPosition = new Vector3(-playerXMoveAbs, playerYPosition, 0);
    public static Vector3 playerMiddlePosition = new Vector3(0, playerYPosition, 0);
    public static Vector3 playerRightPosition = new Vector3(playerXMoveAbs, playerYPosition, 0);

    public static float cPerfectJudgement = 0.04f;
    public static float perfectJudgement = 0.075f;
    public static float goodJudgement = 0.14f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
