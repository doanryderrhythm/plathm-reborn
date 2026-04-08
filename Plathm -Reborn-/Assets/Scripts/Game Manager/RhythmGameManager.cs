using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;

public class RhythmGameManager : MonoBehaviour
{
    public enum LanePosition
    {
        NONE,
        LEFT_POS,
        MIDDLE_POS,
        RIGHT_POS,
    }

    public enum JudgementType
    {
        CPERFECT,
        PERFECT,
        GOOD,
        DAMAGE,
        MISS,
    }

    public enum NoteTypeGeneral
    {
        TAP_NOTE,
        BLACK_NOTE,
        LEFT_TELEPORT,
        RIGHT_TELEPORT,
        SLICE_NOTE,
        SPIKE,
    }

    double currentTiming = 0f;

    [Header("Chart Information")]
    public TextAsset songInfo;
    public TextAsset chartFile;
    public AudioSource audioSource;
    public int difficulty;

    bool isStarted = false;
    public double chartSpeed;

    [Header("Note Groups")]
    public List<TimingGroup> timingGroups;
    public GameObject timingGroupPrefab;
    public GameObject timingGroupStorer;

    public List<List<SpeedStorer>> speedItems;

    private void Start()
    {
        speedItems = new List<List<SpeedStorer>>();
        StartCoroutine(GetReady());
    }

    private void Update()
    {
        if (isStarted)
        {
            currentTiming += (Time.deltaTime * 1000f);
            ChangeSpeedThroughTiming(currentTiming);
        }
    }

    public void ChangeSpeedThroughTiming(double timing)
    {
        for (int index = 0; index < timingGroups.Count; index++)
        {
            List<SpeedStorer> speeds = speedItems[index];

            double tempSpeedMulti = 1f;
            double totalLength = 0f;

            if (speeds.Count <= 0)
            {
                tempSpeedMulti = 1f;
                totalLength += (timing * tempSpeedMulti / 1000f);
                timingGroups[index].gameObject.transform.position = new Vector3(0, (float)(-totalLength * chartSpeed), 0);
                continue;
            }

            bool isChecked = false;
            for (int i = 0; i < speeds.Count; i++)
            {
                if (timing < speeds[i].timing)
                {
                    if (i == 0)
                    {
                        tempSpeedMulti = 1f;
                        totalLength += (timing * tempSpeedMulti / 1000f);
                    }
                    else
                    {
                        tempSpeedMulti = speeds[i - 1].speedMulti;
                        totalLength += (timing - speeds[i - 1].timing) / 1000f * tempSpeedMulti;
                    }
                    timingGroups[index].gameObject.transform.position = new Vector3(0, (float)(-totalLength * chartSpeed), 0);
                    isChecked = true;
                    break;
                }

                if (i == 0)
                {
                    tempSpeedMulti = 1f;
                    totalLength += (speeds[i].timing * tempSpeedMulti / 1000f);
                }
                else
                {
                    tempSpeedMulti = speeds[i - 1].speedMulti;
                    totalLength += (speeds[i].timing - speeds[i - 1].timing) / 1000f * tempSpeedMulti;
                }
            }

            if (!isChecked)
            {
                tempSpeedMulti = speeds[speeds.Count - 1].speedMulti;
                totalLength += (timing - speeds[speeds.Count - 1].timing) / 1000f * tempSpeedMulti;
                timingGroups[index].gameObject.transform.position = new Vector3(0, (float)(-totalLength * chartSpeed), 0);
            }
        }
    }

    [SerializeField] RhythmPlayer player;

    public void AddSpeedGroup()
    {
        GameObject timingGroupObj = Instantiate(timingGroupPrefab.gameObject, Vector3.zero, Quaternion.identity) as GameObject;
        timingGroupObj.transform.SetParent(timingGroupStorer.transform, false);

        TimingGroup newTimingGroup = timingGroupObj.GetComponent<TimingGroup>();
        timingGroups.Add(newTimingGroup);

        List<SpeedStorer> newSpeedItems = new List<SpeedStorer>();
        speedItems.Add(newSpeedItems);
    }

    public void AddSpeedItem(int index, float timing, float speedMulti)
    {
        if (index < 0 || index >= speedItems.Count)
        {
            return;
        }

        SpeedStorer speedItem = new SpeedStorer(timing, speedMulti);
        speedItems[index].Add(speedItem);
    }

    [Header("Notes")]
    [SerializeField] GameObject tapNotePrefab;
    [SerializeField] GameObject blackNotePrefab;
    [SerializeField] GameObject leftTeleportPrefab;
    [SerializeField] GameObject rightTeleportPrefab;
    [SerializeField] GameObject sliceNotePrefab;
    [SerializeField] GameObject middleSpikePrefab;
    [SerializeField] GameObject sideSpikePrefab;

    string[] GetConvertedNoteProperties(string subLine, string line)
    {
        string content = line.Replace(subLine, "").Replace(")", "");
        string[] values = content.Split(',');
        return values;
    }

    void InsertNote(int group, string noteTypeString, double timing, float xPos = 0f)
    {
        GameObject confirmedNote = null;
        Vector3 confirmedPosition = new Vector3(xPos, (float)(timing * chartSpeed) / 1000f, 0f);

        if (group > timingGroups.Count - 1)
        {
            AddSpeedGroup();
        }

        switch (noteTypeString)
        {
            case ValueStorer.tapString: confirmedNote = Instantiate(tapNotePrefab, timingGroups[group].tapFolder.gameObject.transform, false) as GameObject; break;
            case ValueStorer.blackString: confirmedNote = Instantiate(blackNotePrefab, timingGroups[group].blackFolder.gameObject.transform, false) as GameObject; break;
            case ValueStorer.leftTeleportString: confirmedNote = Instantiate(leftTeleportPrefab, timingGroups[group].leftTeleportFolder.gameObject.transform, false) as GameObject; break;
            case ValueStorer.rightTeleportString: confirmedNote = Instantiate(rightTeleportPrefab, timingGroups[group].rightTeleportFolder.gameObject.transform, false) as GameObject; break;
            case ValueStorer.sliceString: confirmedNote = Instantiate(sliceNotePrefab, timingGroups[group].sliceFolder.gameObject.transform, false) as GameObject; break;
            case ValueStorer.middleSpikeString: confirmedNote = Instantiate(middleSpikePrefab, timingGroups[group].spikeFolder.gameObject.transform, false) as GameObject; break;
            case ValueStorer.sideSpikeString: confirmedNote = Instantiate(sideSpikePrefab, timingGroups[group].spikeFolder.gameObject.transform, false) as GameObject; break;
            default: break;
        }

        if (confirmedNote == null)
        {
            return;
        }

        confirmedNote.transform.localPosition = confirmedPosition;

        MusicNote musicNote = confirmedNote.GetComponent<MusicNote>();
        musicNote.timingGroup = timingGroups[group];
        musicNote.timing = timing / 1000f;
    }

    public void ApplyTimingSpeed()
    {
        for (int index = 0; index < timingGroups.Count; index++)
        {
            if (speedItems[index].Count <= 0)
            {
                return;
            }

            List<SpeedStorer> speeds = speedItems[index];
            if (speeds.Count <= 0)
            {
                continue;
            }

            List<MusicNote> notes = null;

            float tempSpeedMulti = 1f;
            float totalLength = 0f;

            for (int i = 0; i < speeds.Count; i++)
            {
                if (i == 0)
                {
                    tempSpeedMulti = 1f;
                    totalLength += (speeds[i].timing * tempSpeedMulti / 1000f);
                }
                else
                {
                    tempSpeedMulti = speeds[i - 1].speedMulti;
                    notes = FindAllNotesWithTiming(index, speeds[i - 1].timing, speeds[i].timing);
                    foreach (MusicNote note in notes)
                    {
                        note.ChangeSpeedPosition(totalLength, chartSpeed, speeds[i - 1].timing, tempSpeedMulti);
                    }

                    totalLength += (speeds[i].timing - speeds[i - 1].timing) / 1000f * tempSpeedMulti;
                }
            }
            tempSpeedMulti = speeds[speeds.Count - 1].speedMulti;
            notes = FindAllNotesWithTiming(index, speeds[speeds.Count - 1].timing, audioSource.clip.length * 1000f);
            foreach (MusicNote note in notes)
            {
                note.ChangeSpeedPosition(totalLength, chartSpeed, speeds[speeds.Count - 1].timing, tempSpeedMulti);
            }

            totalLength += (audioSource.clip.length - speeds[speeds.Count - 1].timing / 1000f) * tempSpeedMulti;
        }
    }

    List<MusicNote> FindAllNotesWithTiming(int index, float beginTiming, float endTiming)
    {
        List<MusicNote> foundTaps = FindAllNotesWithTimingFromFolder(beginTiming, endTiming, timingGroups[index].tapFolder.transform);
        List<MusicNote> foundBlacks = FindAllNotesWithTimingFromFolder(beginTiming, endTiming, timingGroups[index].blackFolder.transform);
        List<MusicNote> foundSlice = FindAllNotesWithTimingFromFolder(beginTiming, endTiming, timingGroups[index].sliceFolder.transform);
        List<MusicNote> foundSpikes = FindAllNotesWithTimingFromFolder(beginTiming, endTiming, timingGroups[index].spikeFolder.transform);
        List<MusicNote> foundLeftTeleports = FindAllNotesWithTimingFromFolder(beginTiming, endTiming, timingGroups[index].leftTeleportFolder.transform);
        List<MusicNote> foundRightTeleport = FindAllNotesWithTimingFromFolder(beginTiming, endTiming, timingGroups[index].rightTeleportFolder.transform);

        List<MusicNote> foundAllNotes = new List<MusicNote>();
        foundAllNotes.AddRange(foundTaps);
        foundAllNotes.AddRange(foundBlacks);
        foundAllNotes.AddRange(foundSlice);
        foundAllNotes.AddRange(foundSpikes);
        foundAllNotes.AddRange(foundLeftTeleports);
        foundAllNotes.AddRange(foundRightTeleport);

        return foundAllNotes;
    }

    List<MusicNote> FindAllNotesWithTimingFromFolder(float beginTiming, float endTiming, Transform folder)
    {
        List<MusicNote> foundNotes = new List<MusicNote>();
        foreach (Transform note in folder)
        {
            MusicNote musicNote = note.GetComponent<MusicNote>();
            if (musicNote != null && musicNote.timing * 1000f >= beginTiming && musicNote.timing * 1000f < endTiming)
            {
                foundNotes.Add(musicNote);
            }
        }
        return foundNotes;
    }

    public void InsertChart(int difficultyIndex)
    {
        player.ChangePosition(LanePosition.MIDDLE_POS);
        this.difficulty = difficultyIndex;

        /*
        switch (this.difficulty)
        {
            case 0: currentDifficultyText.text = ValueStorer.difficultyText + ValueStorer.pointText; break;
            case 1: currentDifficultyText.text = ValueStorer.difficultyText + ValueStorer.lineText; break;
            case 2: currentDifficultyText.text = ValueStorer.difficultyText + ValueStorer.triangleText; break;
            case 3: currentDifficultyText.text = ValueStorer.difficultyText + ValueStorer.squareText; break;
            default: break;
        }
        */

        speedItems.Clear();

        if (chartFile == null)
        {
            return;
        }

        if (chartFile is TextAsset chart)
        {
            using (StringReader reader = new StringReader(chart.text))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line == string.Empty)
                    {
                        continue;
                    }

                    /*
                    if (line.StartsWith(ValueStorer.difficultyString))
                    {
                        string diff = line.Substring(ValueStorer.difficultyString.Length);
                        difficultyInputField.text = diff;
                        continue;
                    }
                    */

                    if (line.StartsWith(ValueStorer.playerPositionString))
                    {
                        string pos = line.Substring(ValueStorer.playerPositionString.Length);
                        if (pos == ValueStorer.playerLeftString) player.ChangePosition(LanePosition.LEFT_POS);
                        else if (pos == ValueStorer.playerMiddleString) player.ChangePosition(LanePosition.MIDDLE_POS);
                        else if (pos == ValueStorer.playerRightString) player.ChangePosition(LanePosition.RIGHT_POS);
                    }

                    if (line.StartsWith(ValueStorer.speedString))
                    {
                        string content = line.Replace(ValueStorer.speedString, "").Replace(")", "");
                        string[] values = content.Split(',');

                        if (int.TryParse(values[0], out int group) &&
                            (float.TryParse(values[1], out float timing) &&
                            (float.TryParse(values[2], out float speed))))
                        {
                            if (group > timingGroups.Count - 1)
                            {
                                AddSpeedGroup();
                            }
                            AddSpeedItem(group, timing, speed);
                        }

                        continue;
                    }

                    if (line.StartsWith(ValueStorer.tapString))
                    {
                        string[] values = GetConvertedNoteProperties(ValueStorer.tapString, line);
                        if (int.TryParse(values[0], out int group) &&
                            float.TryParse(values[1], out float timing) &&
                            float.TryParse(values[2], out float xPos))
                            InsertNote(group, ValueStorer.tapString, timing, xPos);
                        continue;
                    }

                    if (line.StartsWith(ValueStorer.blackString))
                    {
                        string[] values = GetConvertedNoteProperties(ValueStorer.blackString, line);
                        if (int.TryParse(values[0], out int group) &&
                            float.TryParse(values[1], out float timing) &&
                            float.TryParse(values[2], out float xPos))
                            InsertNote(group, ValueStorer.blackString, timing, xPos);
                        continue;
                    }

                    if (line.StartsWith(ValueStorer.leftTeleportString))
                    {
                        string[] values = GetConvertedNoteProperties(ValueStorer.leftTeleportString, line);
                        if (int.TryParse(values[0], out int group) &&
                            float.TryParse(values[1], out float timing) &&
                            float.TryParse(values[2], out float xPos))
                            InsertNote(group, ValueStorer.leftTeleportString, timing, xPos);
                        continue;
                    }

                    if (line.StartsWith(ValueStorer.rightTeleportString))
                    {
                        string[] values = GetConvertedNoteProperties(ValueStorer.rightTeleportString, line);
                        if (int.TryParse(values[0], out int group) &&
                            float.TryParse(values[1], out float timing) &&
                            float.TryParse(values[2], out float xPos))
                            InsertNote(group, ValueStorer.rightTeleportString, timing, xPos);
                        continue;
                    }

                    if (line.StartsWith(ValueStorer.sliceString))
                    {
                        string[] values = GetConvertedNoteProperties(ValueStorer.sliceString, line);
                        if (int.TryParse(values[0], out int group) &&
                            float.TryParse(values[1], out float timing))
                            InsertNote(group, ValueStorer.sliceString, timing);
                        continue;
                    }

                    if (line.StartsWith(ValueStorer.middleSpikeString))
                    {
                        string[] values = GetConvertedNoteProperties(ValueStorer.middleSpikeString, line);
                        if (int.TryParse(values[0], out int group) &&
                            float.TryParse(values[1], out float timing))
                            InsertNote(group, ValueStorer.middleSpikeString, timing);
                        continue;
                    }

                    if (line.StartsWith(ValueStorer.sideSpikeString))
                    {
                        string[] values = GetConvertedNoteProperties(ValueStorer.sideSpikeString, line);
                        if (int.TryParse(values[0], out int group) &&
                            float.TryParse(values[1], out float timing))
                            InsertNote(group, ValueStorer.sideSpikeString, timing);
                        continue;
                    }
                }
            }
        }

        ApplyTimingSpeed();
    }

    IEnumerator GetReady()
    {
        yield return new WaitForSeconds(2.0f);
        InsertChart(0);
        yield return new WaitForSeconds(3.0f);
        audioSource.Play();
        isStarted = true;
    }
}
