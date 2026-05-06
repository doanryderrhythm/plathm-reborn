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

public class CommandRemoveNotes : EditorCommand
{
    List<MusicNote> notes;
    List<Transform> originalFolders;
    List<Transform> undoRedoFolders;

    public CommandRemoveNotes()
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

public class CommandMirrorNotes : EditorCommand
{
    List<MusicNote> notes;
    List<MusicNote> originalTeleportNotes, newTeleportNotes;
    List<Transform> originalFolders, newFolders;

    bool isCopied = false;
    List<MusicNote> copiedNotes = null;
    List<Transform> originalCopiedFolders = null;

    public CommandMirrorNotes(List<MusicNote> notes, 
        List<MusicNote> originalTeleportNotes, List<Transform> originalFolders,
        List<MusicNote> newTeleportNotes, List<Transform> newFolders)
    {
        editorManager = GameObject.FindFirstObjectByType<EditorManager>();
        this.notes = notes;

        this.originalTeleportNotes = originalTeleportNotes;
        this.newTeleportNotes = newTeleportNotes;

        this.originalFolders = originalFolders;
        this.newFolders = newFolders;
    }

    public override void UndoCommand()
    {
        MirrorNotes(true);
    }

    public override void RedoCommand()
    {
        MirrorNotes(false);
    }

    void MirrorNotes(bool isUndo)
    {
        ToggleEverythingOff();

        for (int i = 0; i < notes.Count; i++)
        {
            if (notes[i] == null)
                continue;

            notes[i].transform.localPosition = new Vector3(
                -notes[i].transform.localPosition.x,
                notes[i].transform.localPosition.y, 0);
        }

        if (isUndo)
        {
            for (int i = 0; i < originalTeleportNotes.Count; i++)
            {
                if (originalTeleportNotes[i] == null)
                    continue;

                originalTeleportNotes[i].transform.SetParent(originalFolders[i]);
                originalTeleportNotes[i].gameObject.SetActive(true);

                newTeleportNotes[i].transform.SetParent(
                    newTeleportNotes[i].timingGroup.undoRedoFolder.transform);
                newTeleportNotes[i].gameObject.SetActive(false);
            }
        }
        else
        {
            for (int i = 0; i < newTeleportNotes.Count; i++)
            {
                if (newTeleportNotes[i] == null)
                    continue;

                newTeleportNotes[i].transform.SetParent(newFolders[i]);
                newTeleportNotes[i].gameObject.SetActive(true);

                originalTeleportNotes[i].transform.SetParent(
                    originalTeleportNotes[i].timingGroup.undoRedoFolder.transform);
                originalTeleportNotes[i].gameObject.SetActive(false);
            }
        }

        if (isCopied && isUndo)
        {
            for (int i = 0; i < copiedNotes.Count; i++)
            {
                copiedNotes[i].transform.SetParent(copiedNotes[i].timingGroup.undoRedoFolder.transform, false);
                copiedNotes[i].gameObject.SetActive(false);
            }
        }
        else if (isCopied && !isUndo)
        {
            for (int i = 0; i < copiedNotes.Count; i++)
            {
                copiedNotes[i].transform.SetParent(originalCopiedFolders[i], false);
                copiedNotes[i].gameObject.SetActive(true);
            }
        }
    }

    public void InsertNewData(List<MusicNote> copiedNotes)
    {
        this.isCopied = true;
        this.copiedNotes = copiedNotes;

        originalCopiedFolders = new List<Transform>();
        for (int i = 0; i < copiedNotes.Count; i++)
            originalCopiedFolders.Add(copiedNotes[i].transform.parent);
    }

    void ToggleEverythingOff()
    {
        foreach (MusicNote note in notes)
            note.ToggleSelected(false);
        foreach (MusicNote note in originalTeleportNotes)
            note.ToggleSelected(false);
        foreach (MusicNote note in newTeleportNotes)
            note.ToggleSelected(false);
    }
}

public class CommandChangePlayerPos : EditorCommand
{
    TestPlayer player;
    int originalPos, newPos;

    public CommandChangePlayerPos(int originalPos, int newPos)
    {
        player = GameObject.FindFirstObjectByType<TestPlayer>();
        this.originalPos = originalPos;
        this.newPos = newPos;
    }

    public override void UndoCommand()
    {
        player.ChangePositionUndoRedo(originalPos);
    }

    public override void RedoCommand()
    {
        player.ChangePositionUndoRedo(newPos);
    }
}

public class CommandMoveMultipleNotes : EditorCommand
{
    List<MusicNote> notes;
    List<float> oldTimings, currentTimings;
    List<float> oldPoses, currentPoses;

    bool isArea = false;
    float oldOpenTiming, currentOpenTiming;
    float oldCloseTiming, currentCloseTiming;

    public CommandMoveMultipleNotes(List<MusicNote> notes, 
        List<float> oldTimings, List<float> currentTimings)
    {
        editorManager = GameObject.FindFirstObjectByType<EditorManager>();

        this.notes = notes;
        this.oldTimings = oldTimings;
        this.currentTimings = currentTimings;
    }

    public override void UndoCommand()
    {
        for (int i = 0; i < notes.Count; i++)
        {
            notes[i].timing = oldTimings[i];
            notes[i].temporaryTiming = oldTimings[i];

            notes[i].transform.localPosition = new Vector3(
                notes[i].transform.localPosition.x,
                oldTimings[i] * editorManager.chartSpeed, 0f);
        }

        if (isArea)
            editorManager.ExecuteNoteAreaSignal(notes, oldOpenTiming, oldCloseTiming);
    }

    public override void RedoCommand()
    {
        for (int i = 0; i < notes.Count; i++)
        {
            notes[i].timing = currentTimings[i];
            notes[i].temporaryTiming = currentTimings[i];

            notes[i].transform.localPosition = new Vector3(
                notes[i].transform.localPosition.x,
                currentTimings[i] * editorManager.chartSpeed, 0f);
        }

        if (isArea)
            editorManager.ExecuteNoteAreaSignal(notes, currentOpenTiming, currentCloseTiming);
    }

    public void InsertOpenInterface(float oldOpenTiming, float currentOpenTiming)
    {
        isArea = true;
        this.oldOpenTiming = oldOpenTiming;
        this.currentOpenTiming = currentOpenTiming;
    }

    public void InsertCloseInterface(float oldCloseTiming, float currentCloseTiming)
    {
        isArea = true;
        this.oldCloseTiming = oldCloseTiming;
        this.currentCloseTiming = currentCloseTiming;
    }
}
