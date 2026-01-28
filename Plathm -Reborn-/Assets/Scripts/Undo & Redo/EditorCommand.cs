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
    private Vector3 notePosition;

    public CommandAddOneNote(GameObject noteObject, Vector3 notePosition)
    {
        editorManager = GameObject.FindFirstObjectByType<EditorManager>();

        this.noteObject = noteObject;
        this.notePosition = notePosition;
    }

    public override void UndoCommand()
    {
        editorManager.UndoAddOneNote(this.noteObject);
    }
    public override void RedoCommand()
    {
        editorManager.RedoAddOneNote(this.noteObject, this.notePosition);
    }
    public override void FreeCommand()
    {
        MonoBehaviour.Destroy(this.noteObject);
    }
}

public class CommandMoveOneNote : EditorCommand
{
    private GameObject noteObject;
    private Vector3 noteOriginalPosition;
    private Vector3 noteNewPosition;

    public CommandMoveOneNote(GameObject noteObject, Vector3 noteOriginalPosition, Vector3 noteNewPosition)
    {
        editorManager = GameObject.FindFirstObjectByType<EditorManager>();

        this.noteObject = noteObject;
        this.noteOriginalPosition = noteOriginalPosition;
        this.noteNewPosition = noteNewPosition;
    }

    public override void UndoCommand()
    {
        editorManager.UndoMoveOneNote(noteObject, noteOriginalPosition);
    }
    public override void RedoCommand()
    {
        editorManager.RedoMoveOneNote(noteObject, noteNewPosition);
    }
    public override void FreeCommand()
    {
        
    }
}

public class CommandDeleteOneNote : EditorCommand
{
    private GameObject noteObject;
    private Vector3 noteDeletedPosition;

    public CommandDeleteOneNote(GameObject noteObject, Vector3 noteDeletedPosition)
    {
        editorManager = GameObject.FindFirstObjectByType<EditorManager>();

        this.noteObject = noteObject;
        this.noteDeletedPosition = noteDeletedPosition;
    }

    public override void UndoCommand()
    {
        editorManager.UndoDeleteOneNote(noteObject, noteDeletedPosition);
    }
    public override void RedoCommand()
    {
        editorManager.RedoDeleteOneNote(noteObject);
    }
    public override void FreeCommand()
    {
        if (noteObject != null && !noteObject.activeSelf)
        {
            MonoBehaviour.Destroy(this.noteObject);
        }
    }
}