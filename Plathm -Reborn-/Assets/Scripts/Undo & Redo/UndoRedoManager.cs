using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UndoRedoManager : MonoBehaviour
{
    public Stack<EditorCommand> undoStack;
    public Stack<EditorCommand> redoStack;

    private void Start()
    {
        undoStack = new Stack<EditorCommand>();
        redoStack = new Stack<EditorCommand>();
    }

    public void ExecuteCommand(EditorCommand cmd)
    {
        undoStack.Push(cmd);
        Debug.Log(undoStack.Count);

        if (undoStack.Count > 100)
        {
            Stack<EditorCommand> tempStack = new Stack<EditorCommand>();

            while (undoStack.Count > 1)
                tempStack.Push(undoStack.Pop());

            undoStack.Pop();

            while (tempStack.Count > 0)
                undoStack.Push(tempStack.Pop());
        }

        redoStack.Clear();
    }

    public void ExecuteUndo()
    {
        if (undoStack.Count > 0)
        {
            EditorCommand undo = undoStack.Pop();
            undo.UndoCommand();

            redoStack.Push(undo);
        }
    }

    public void ExecuteRedo()
    {
        if (redoStack.Count > 0)
        {
            EditorCommand redo = redoStack.Pop();
            redo.RedoCommand();

            undoStack.Push(redo);
        }
    }
}
