using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class EditorCommand
{
    protected EditorManager editorManager;
    protected UndoRedoManager undoRedoManager;

    public virtual void UndoCommand() { }
    public virtual void RedoCommand() { }
}

public class CommandAddOneNote : EditorCommand
{
    MusicNote note;
    Transform originalFolder;
    Transform undoRedoFolder;

    public CommandAddOneNote(MusicNote note)
    {
        this.note = note;
        originalFolder = note.transform.parent;
        undoRedoFolder = note.timingGroup.undoRedoFolder.transform;
    }

    public override void UndoCommand()
    {
        originalFolder = note.transform.parent;
        note.transform.SetParent(undoRedoFolder, false);
        note.gameObject.SetActive(false);
    }

    public override void RedoCommand()
    {
        note.transform.SetParent(originalFolder, false);
        note.gameObject.SetActive(true);
    }
}

public class CommandAddMultipleNotes : EditorCommand
{
    List<MusicNote> notes;
    List<Transform> originalFolders;
    List<Transform> undoRedoFolders;

    public CommandAddMultipleNotes(List<MusicNote> notes)
    {
        originalFolders = new List<Transform>();
        undoRedoFolders = new List<Transform>();

        this.notes = notes;

        for (int i = 0; i < notes.Count; i++)
        {
            originalFolders.Add(notes[i].transform.parent);
            undoRedoFolders.Add(notes[i].timingGroup.undoRedoFolder.transform);
        }
    }

    public override void UndoCommand()
    {
        for (int i = 0; i < notes.Count; i++)
        {
            originalFolders[i] = notes[i].transform.parent;
            notes[i].transform.SetParent(undoRedoFolders[i], false);
            notes[i].gameObject.SetActive(false);
        }
    }

    public override void RedoCommand()
    {
        for (int i = 0; i < notes.Count; i++)
        {
            notes[i].transform.SetParent(originalFolders[i], false);
            notes[i].gameObject.SetActive(true);
        }
    }
}

public class CommandMoveOneNote : EditorCommand
{
    MusicNote note;
    float oldTiming, currentTiming;
    float oldPos, currentPos;

    public CommandMoveOneNote(MusicNote note, float oldTiming, float currentTiming, float oldPos, float currentPos)
    {
        editorManager = GameObject.FindFirstObjectByType<EditorManager>();

        this.note = note;

        this.oldTiming = oldTiming;
        this.currentTiming = currentTiming;
        this.oldPos = oldPos;
        this.currentPos = currentPos;
    }

    public override void UndoCommand()
    {
        note.timing = oldTiming;
        note.temporaryTiming = oldTiming;

        note.transform.localPosition = new Vector3(oldPos,
            oldTiming * editorManager.chartSpeed, 0f);
    }

    public override void RedoCommand()
    {
        note.timing = currentTiming;
        note.temporaryTiming = currentTiming;

        note.transform.localPosition = new Vector3(currentPos,
            currentTiming * editorManager.chartSpeed, 0f);
    }

    public void SetNewData(float currentTiming, float currentPos)
    {
        this.currentTiming = currentTiming;
        this.currentPos = currentPos;
        Debug.Log(oldTiming + " " + oldPos + " " + currentTiming + " " + currentPos);
    }
}

public class CommandRemoveOneNote : EditorCommand
{
    List<MusicNote> notes;
    List<Transform> originalFolders;
    List<Transform> undoRedoFolders;

    public CommandRemoveOneNote()
    {
        this.notes = new List<MusicNote>();
        this.originalFolders = new List<Transform>();
        this.undoRedoFolders = new List<Transform>();
    }

    public override void UndoCommand()
    {
        for (int i = 0; i < notes.Count; i++)
        {
            notes[i].gameObject.SetActive(true);
            notes[i].transform.SetParent(originalFolders[i]);
        }
    }

    public override void RedoCommand()
    {
        for (int i = 0; i < notes.Count; i++)
        {
            notes[i].gameObject.SetActive(false);
            notes[i].transform.SetParent(undoRedoFolders[i]);
        }
    }

    public void SetNewData(MusicNote note, Transform originalFolder, Transform undoRedoFolder)
    {
        notes.Add(note);
        originalFolders.Add(originalFolder);
        undoRedoFolders.Add(undoRedoFolder);
    }
}
