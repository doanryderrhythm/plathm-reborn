using UnityEngine;
using UnityEngine.InputSystem;

public class TestPlayer : MonoBehaviour
{
    //Other shenanigans
    private EditorManager editorManager;

    [SerializeField] EditorManager.LanePosition lanePosition = EditorManager.LanePosition.MIDDLE_POS;

    [SerializeField] InputAction moveLeftAction;
    [SerializeField] InputAction moveRightAction;

    private void Awake()
    {
        editorManager = GameObject.FindFirstObjectByType<EditorManager>();

        moveLeftAction.performed += MoveLeft;
        moveRightAction.performed += MoveRight;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnEnable()
    {
        moveLeftAction.Enable();
        moveRightAction.Enable();
    }

    void OnDisable()
    {
        moveLeftAction.Disable();
        moveRightAction.Disable();
    }

    void MoveLeft(InputAction.CallbackContext context)
    {
        if (editorManager != null && !editorManager.playMode)
        {
            return;
        }

        if (lanePosition == EditorManager.LanePosition.LEFT_POS)
        {
            return;
        }

        if (lanePosition == EditorManager.LanePosition.MIDDLE_POS)
        {
            ChangePosition(EditorManager.LanePosition.LEFT_POS);
        }
        else if (lanePosition == EditorManager.LanePosition.RIGHT_POS)
        {
            ChangePosition(EditorManager.LanePosition.MIDDLE_POS);
        }
    }

    void MoveRight(InputAction.CallbackContext context)
    {
        if (editorManager != null && !editorManager.playMode)
        {
            return;
        }

        if (lanePosition == EditorManager.LanePosition.RIGHT_POS)
        {
            return;
        }

        if (lanePosition == EditorManager.LanePosition.MIDDLE_POS)
        {
            ChangePosition(EditorManager.LanePosition.RIGHT_POS);
        }
        else if (lanePosition == EditorManager.LanePosition.LEFT_POS)
        {
            ChangePosition(EditorManager.LanePosition.MIDDLE_POS);
        }
    }

    void ChangePosition(EditorManager.LanePosition lanePosition)
    {
        this.lanePosition = lanePosition;
        
        if (this.lanePosition == EditorManager.LanePosition.LEFT_POS)
        {
            this.transform.position = ValueStorer.playerLeftPosition;
        }
        else if (this.lanePosition == EditorManager.LanePosition.MIDDLE_POS)
        {
            this.transform.position = ValueStorer.playerMiddlePosition;
        }
        else if (this.lanePosition == EditorManager.LanePosition.RIGHT_POS)
        {
            this.transform.position = ValueStorer.playerRightPosition;
        }
    }

    #region GETTERS

    public EditorManager.LanePosition GetLanePosition()
    {
        return lanePosition;
    }

    #endregion
}
