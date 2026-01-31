using UnityEngine;

public class EditorCommand
{
    protected EditorManager editorManager;

    public enum CommandType
    {
        ADD_ONE_NOTE,
        MOVE_ONE_NOTE,
        DELETE_ONE_NOTE,
    }

    protected CommandType commandType;

    public virtual void UndoCommand() { }
    public virtual void RedoCommand() { }
    public virtual void FreeCommand() { }
}

public class CommandAddOneNote : EditorCommand
{
    private GameObject noteObject;
    private float timing;
    private float notePosition;

    public CommandAddOneNote(GameObject noteObject, float timing, float notePosition)
    {
        editorManager = GameObject.FindFirstObjectByType<EditorManager>();

        this.noteObject = noteObject;
        this.timing = timing;
        this.notePosition = notePosition;
    }

    public override void UndoCommand()
    {
        //editorManager.UndoAddOneNote(this.noteObject);
    }
    public override void RedoCommand()
    {
        //editorManager.RedoAddOneNote(this.noteObject, this.timing, this.notePosition);
    }
    public override void FreeCommand()
    {
        MonoBehaviour.Destroy(this.noteObject);
    }
}

public class CommandMoveOneNote : EditorCommand
{
    private GameObject noteObject;
    private float originalTiming;
    private float newTiming;
    private float noteOriginalPosition;
    private float noteNewPosition;

    public CommandMoveOneNote(GameObject noteObject, float originalTiming, float newTiming, float noteOriginalPosition, float noteNewPosition)
    {
        editorManager = GameObject.FindFirstObjectByType<EditorManager>();

        this.noteObject = noteObject;
        this.originalTiming = originalTiming;
        this.newTiming = newTiming;
        this.noteOriginalPosition = noteOriginalPosition;
        this.noteNewPosition = noteNewPosition;
    }

    public override void UndoCommand()
    {
        //editorManager.UndoMoveOneNote(this.noteObject, this.originalTiming, this.noteOriginalPosition);
    }
    public override void RedoCommand()
    {
        //editorManager.RedoMoveOneNote(this.noteObject, this.newTiming, this.noteNewPosition);
    }
    public override void FreeCommand()
    {
        
    }
}

public class CommandDeleteOneNote : EditorCommand
{
    private GameObject noteObject;
    private float timing;
    private float noteDeletedPosition;

    public CommandDeleteOneNote(GameObject noteObject, float timing, float noteDeletedPosition)
    {
        editorManager = GameObject.FindFirstObjectByType<EditorManager>();

        this.noteObject = noteObject;
        this.timing = timing;
        this.noteDeletedPosition = noteDeletedPosition;
    }

    public override void UndoCommand()
    {
        //editorManager.UndoDeleteOneNote(this.noteObject, this.timing, this.noteDeletedPosition);
    }
    public override void RedoCommand()
    {
        //editorManager.RedoDeleteOneNote(noteObject);
    }
    public override void FreeCommand()
    {
        if (noteObject != null && !noteObject.activeSelf)
        {
            MonoBehaviour.Destroy(this.noteObject);
        }
    }
}

public class CommandChangeOffset : EditorCommand
{
    private float previousOffset;
    private float nextOffset;

    public CommandChangeOffset(float previousOffset, float nextOffset)
    {
        editorManager = GameObject.FindFirstObjectByType<EditorManager>();

        this.previousOffset = previousOffset;
        this.nextOffset = nextOffset;
    }

    public override void UndoCommand()
    {
        //editorManager.UndoChangeOffset(this.previousOffset);
    }

    public override void RedoCommand()
    {
        //editorManager.RedoChangeOffset(this.nextOffset);
    }

    public override void FreeCommand()
    {
        
    }
}

public class CommandChangeSpeed : EditorCommand
{
    private float previousSpeed;
    private float nextSpeed;

    public CommandChangeSpeed(float previousSpeed, float nextSpeed)
    {
        editorManager = GameObject.FindFirstObjectByType<EditorManager>();

        this.previousSpeed = previousSpeed;
        this.nextSpeed = nextSpeed;
    }

    public override void UndoCommand()
    {
        //editorManager.UndoChangeSpeed(this.previousSpeed);
    }

    public override void RedoCommand()
    {
        //editorManager.RedoChangeSpeed(this.nextSpeed);
    }

    public override void FreeCommand()
    {

    }
}