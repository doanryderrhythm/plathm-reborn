using System.Collections.Generic;
using UnityEngine;

public class TimingGroup : MonoBehaviour
{
    [Header("Note Folders")]
    public GameObject tapFolder;
    public GameObject blackFolder;
    public GameObject sliceFolder;
    public GameObject leftTeleportFolder;
    public GameObject rightTeleportFolder;
    public GameObject spikeFolder;
    public GameObject usedNotesFolder;
    public GameObject undoRedoFolder;

    public List<Transform> GetNotesForSaving()
    {
        List<Transform> notes = new List<Transform>();

        foreach (Transform noteTransform in tapFolder.transform)
            notes.Add(noteTransform);
        foreach (Transform noteTransform in blackFolder.transform)
            notes.Add(noteTransform);
        foreach (Transform noteTransform in sliceFolder.transform)
            notes.Add(noteTransform);
        foreach (Transform noteTransform in leftTeleportFolder.transform)
            notes.Add(noteTransform);
        foreach (Transform noteTransform in rightTeleportFolder.transform)
            notes.Add(noteTransform);
        foreach (Transform noteTransform in spikeFolder.transform)
            notes.Add(noteTransform);

        return notes;
    }    
}
