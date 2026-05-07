using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class UndoRedoManager : MonoBehaviour
{
    public Stack<EditorCommand> undoStack;
    public Stack<EditorCommand> redoStack;

    [SerializeField] InputActionReference undoAction;
    [SerializeField] InputActionReference redoAction;

    private void Awake()
    {
        undoStack = new Stack<EditorCommand>();
        redoStack = new Stack<EditorCommand>();
    }

    private void OnEnable()
    {
        undoAction.action.performed += OnUndoPerformed;
        undoAction.action.Enable();

        redoAction.action.performed += OnRedoPerformed;
        redoAction.action.Enable();
    }

    private void OnDisable()
    {
        undoAction.action.performed -= OnUndoPerformed;
        undoAction.action.Disable();

        redoAction.action.performed -= OnRedoPerformed;
        redoAction.action.Disable();
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

    public void OnUndoPerformed(InputAction.CallbackContext context)
    {
        ExecuteUndo();
    }

    public void OnRedoPerformed(InputAction.CallbackContext context)
    {
        ExecuteRedo();
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

    public void ResetStacks()
    {
        undoStack.Clear();
        redoStack.Clear();
    }
}
