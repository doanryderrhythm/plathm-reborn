using UnityEngine;
using UnityEngine.InputSystem;

public class TestPlayer : MonoBehaviour
{
    //Other shenanigans
    private EditorManager editorManager;
    private UndoRedoManager undoRedoManager;

    public EditorManager.LanePosition originalPosition;

    public EditorManager.LanePosition lanePosition = EditorManager.LanePosition.MIDDLE_POS;

    [SerializeField] InputAction moveLeftAction;
    [SerializeField] InputAction moveRightAction;

    private void Awake()
    {
        editorManager = GameObject.FindFirstObjectByType<EditorManager>();
        undoRedoManager = GameObject.FindFirstObjectByType<UndoRedoManager>();

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

    public void ChangePosition(EditorManager.LanePosition lanePosition)
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

    public void ChangePosition(int position)
    {
        int originalIndex = (int)lanePosition;

        this.lanePosition = (EditorManager.LanePosition)position;
        int newIndex = (int)lanePosition;

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

        CommandChangePlayerPos commandChangePlayerPos = new CommandChangePlayerPos(originalIndex, newIndex);
        undoRedoManager.ExecuteCommand(commandChangePlayerPos);
    }

    public void ChangePositionUndoRedo(int position)
    {
        this.lanePosition = (EditorManager.LanePosition)position;

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
