using UnityEngine;
using UnityEngine.InputSystem;
using static RhythmGameManager;

public class RhythmPlayer : MonoBehaviour
{
    private RhythmGameManager rhythmManager;

    [SerializeField] InputActionReference moveLeftAction;
    [SerializeField] InputActionReference moveRightAction;

    private void Awake()
    {
        rhythmManager = GameObject.FindFirstObjectByType<RhythmGameManager>();
    }

    private void OnEnable()
    {
        moveLeftAction.action.Enable();
        moveRightAction.action.Enable();

        moveLeftAction.action.performed     += MoveLeft;
        moveRightAction.action.performed    += MoveRight;
    }

    private void OnDisable()
    {
        moveLeftAction.action.performed     -= MoveLeft;
        moveRightAction.action.performed    -= MoveRight;

        moveLeftAction.action.Disable();
        moveRightAction.action.Disable();
    }

    private RhythmGameManager.LanePosition lanePosition = RhythmGameManager.LanePosition.MIDDLE_POS;

    void MoveLeft(InputAction.CallbackContext context)
    {
        if (lanePosition == RhythmGameManager.LanePosition.LEFT_POS)
        {
            return;
        }

        if (lanePosition == RhythmGameManager.LanePosition.MIDDLE_POS)
        {
            ChangePosition(RhythmGameManager.LanePosition.LEFT_POS);
        }
        else if (lanePosition == RhythmGameManager.LanePosition.RIGHT_POS)
        {
            ChangePosition(RhythmGameManager.LanePosition.MIDDLE_POS);
        }
    }

    void MoveRight(InputAction.CallbackContext context)
    {
        if (lanePosition == RhythmGameManager.LanePosition.RIGHT_POS)
        {
            return;
        }

        if (lanePosition == RhythmGameManager.LanePosition.MIDDLE_POS)
        {
            ChangePosition(RhythmGameManager.LanePosition.RIGHT_POS);
        }
        else if (lanePosition == RhythmGameManager.LanePosition.LEFT_POS)
        {
            ChangePosition(RhythmGameManager.LanePosition.MIDDLE_POS);
        }
    }

    public void ChangePosition(RhythmGameManager.LanePosition lanePosition)
    {
        this.lanePosition = lanePosition;

        if (this.lanePosition == RhythmGameManager.LanePosition.LEFT_POS)
        {
            this.transform.localPosition = ValueStorer.playerLeftPosition;
        }
        else if (this.lanePosition == RhythmGameManager.LanePosition.MIDDLE_POS)
        {
            this.transform.localPosition = ValueStorer.playerMiddlePosition;
        }
        else if (this.lanePosition == RhythmGameManager.LanePosition.RIGHT_POS)
        {
            this.transform.localPosition = ValueStorer.playerRightPosition;
        }
    }
}
