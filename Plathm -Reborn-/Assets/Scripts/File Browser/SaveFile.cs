using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class SaveFile : MonoBehaviour
{
    [SerializeField] EditorManager editorManager;
    [SerializeField] UIManager uiManager;

    [SerializeField] TMP_InputField difficultyInputField;

    [SerializeField] TMP_InputField songNameInputField;
    [SerializeField] TMP_InputField songArtistInputField;
    [SerializeField] TMP_InputField charterInputField;
    [SerializeField] TMP_InputField chartOffsetInputField;
    [SerializeField] TMP_InputField chartSpeedInputField;

    [SerializeField] TMP_Text directoryText;

    public void SaveChart()
    {
        string directory = directoryText.text;

        string chartInfoFile = directory + ValueStorer.informationString;

        if (string.IsNullOrEmpty(chartInfoFile))
        {
            return;
        }

        string chartInfo = string.Empty;

        chartInfo = chartInfo + ValueStorer.songNameString + songNameInputField.text + "\n" +
                                ValueStorer.songArtistString + songArtistInputField.text + "\n" +
                                ValueStorer.charterNameString + charterInputField.text + "\n" +
                                ValueStorer.chartOffsetString + chartOffsetInputField.text + "\n" +
                                ValueStorer.chartSpeedString + chartSpeedInputField.text + "\n\n";

        List<BPMStorer> BPMItems = uiManager.timingItems;
        for (int i = 0; i < BPMItems.Count; i++)
        {
            chartInfo = chartInfo + ValueStorer.timingString + BPMItems[i].timing + "," + BPMItems[i].BPM + ")\n";
        }

        File.WriteAllText(chartInfoFile, chartInfo);


        string chartFile;
        switch (editorManager.difficulty)
        {
            case 0: chartFile = directory + ValueStorer.difficultyPoint; break;
            case 1: chartFile = directory + ValueStorer.difficultyLine; break;
            case 2: chartFile = directory + ValueStorer.difficultyTriangle; break;
            case 3: chartFile = directory + ValueStorer.difficultySquare; break;
            default: chartFile = string.Empty; break;
        }

        Debug.Log(chartFile);

        if (string.IsNullOrEmpty(chartFile))
        {
            return;
        }

        string content = ValueStorer.difficultyString + difficultyInputField.text + "\n\n";

        List<List<SpeedStorer>> speedItems = uiManager.speedItems;
        for (int group = 0; group < speedItems.Count; group++)
        {
            for (int item = 0; item < speedItems[group].Count; item++)
            {
                content = content + GetSpeedDataToText(group, speedItems[group][item].timing, speedItems[group][item].speedMulti);
            }
        }

        content = content + "\n";

        List<TimingGroup> timingGroups = editorManager.timingGroups;
        List<Transform> notes = new List<Transform>();
        for (int i = 0; i < timingGroups.Count; i++)
        {
            notes.AddRange(timingGroups[i].GetNotesForSaving());
        }
        foreach (Transform note in notes)
        {
            if (note == null)
            {
                continue;
            }

            MusicNote musicNote = note.GetComponent<MusicNote>();
            if (musicNote == null)
            {
                continue;
            }

            int timingGroupIndex = timingGroups.IndexOf(musicNote.timingGroup);
            string saveLine = GetNoteDataToText(musicNote.GetNoteType(), timingGroupIndex, musicNote.timing * 1000f, musicNote.transform.position.x);
            content = content + saveLine;
        }

        File.WriteAllText(chartFile, content);
    }

    string GetNoteDataToText(MusicNote.NoteType noteType, int group, float timing, float xPos = 0)
    {
        string saveLine = string.Empty;
        switch (noteType)
        {
            case MusicNote.NoteType.TAP_NOTE:
                saveLine = ValueStorer.tapString + group + "," + timing + "," + xPos + ")\n";
                break;
            case MusicNote.NoteType.BLACK_NOTE:
                saveLine = ValueStorer.blackString + group + "," + timing + "," + xPos + ")\n";
                break;
            case MusicNote.NoteType.LEFT_TELEPORT:
                saveLine = ValueStorer.leftTeleportString + group + "," + timing + "," + xPos + ")\n";
                break;
            case MusicNote.NoteType.RIGHT_TELEPORT:
                saveLine = ValueStorer.rightTeleportString + group + "," + timing + "," + xPos + ")\n";
                break;
            case MusicNote.NoteType.SLICE_NOTE:
                saveLine = ValueStorer.sliceString + group + "," + timing + ")\n";
                break;
            case MusicNote.NoteType.MIDDLE_SPIKE:
                saveLine = ValueStorer.middleSpikeString + group + "," + timing + ")\n";
                break;
            case MusicNote.NoteType.SIDE_SPIKE:
                saveLine = ValueStorer.sideSpikeString + group + "," + timing + ")\n";
                break;
            default: break;
        }
        return saveLine;
    }

    string GetSpeedDataToText(int group, float timing, float speedMulti)
    {
        string saveLine = ValueStorer.speedString + group + "," + timing + "," + speedMulti + ")\n";
        return saveLine;
    }
}
