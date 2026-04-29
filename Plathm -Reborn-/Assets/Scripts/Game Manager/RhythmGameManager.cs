using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    public enum IndicatorType
    {
        FULL_PERFECT,
        ALL_COMBO,
        NORMAL,
        FAILED,
    }

    public double currentTiming = 0f;

    [Header("Chart Information")]
    public TextAsset songInfo;
    public string songName;
    public string songArtist;
    public Sprite jacketArtRaw;
    public string level;
    public TextAsset chartFile;
    public AudioSource audioSource;
    public int difficulty;

    public bool isStarted = false;
    public double chartSpeed;

    public bool isMirrored;

    [SerializeField] int noteCount = 0;
    public float totalScore = 0.0f;

    [Header("Note Groups")]
    public List<TimingGroup> timingGroups;
    public GameObject timingGroupPrefab;
    public GameObject timingGroupStorer;

    public List<List<SpeedStorer>> speedItems;

    [Header("Input Actions")]
    [SerializeField] InputActionReference inputLeftTeleport;
    [SerializeField] InputActionReference inputRightTeleport;
    [SerializeField] InputActionReference inputSlice;

    [Header("Judgement VFX")]
    public GameObject tapCPerfectPrefab;
    public GameObject tapPerfectPrefab;
    public GameObject tapGoodPrefab;
    [Space(10.0f)]
    public GameObject blackCPerfectPrefab;
    [Space(10.0f)]
    public GameObject sliceCPerfectPrefab;
    public GameObject slicePerfectPrefab;
    public GameObject sliceGoodPrefab;
    [Space(10.0f)]
    public GameObject leftTeleportCPerfectPrefab;
    public GameObject leftTeleportPerfectPrefab;
    public GameObject leftTeleportGoodPrefab;
    [Space(10.0f)]
    public GameObject rightTeleportCPerfectPrefab;
    public GameObject rightTeleportPerfectPrefab;
    public GameObject rightTeleportGoodPrefab;
    [Space(10.0f)]
    public GameObject spikesPrefab;

    [Header("Scoring")]
    public int comboCount = 0;
    public int maxComboCount = 0;
    public IndicatorType indicatorType;

    public int CPerfectNotes;
    public int perfectNotes;
    public int goodNotes;
    public int damageNotes;
    public int missNotes;
    [Space(10.0f)]
    public int earlyCount;
    public int lateCount;
    [Space(10.0f)]
    public float health = 100f;

    [Header("Animators")]
    [SerializeField] AnimatorController openingAnimator;
    [SerializeField] AnimatorController closingAnimator;

    [Header("UI")]
    [SerializeField] TMP_Text songNameText;
    [SerializeField] TMP_Text mirrorText;

    [SerializeField] TMP_Text comboText;
    [SerializeField] TMP_Text scoreText;

    [SerializeField] TMP_Text CPerfectText;
    [SerializeField] TMP_Text perfectText;
    [SerializeField] TMP_Text goodText;
    [SerializeField] TMP_Text damageText;
    [SerializeField] TMP_Text missText;

    [SerializeField] TMP_Text currentDifficultyText;

    [SerializeField] RectTransform healthBar;

    [SerializeField] Image indicator;
    [SerializeField] Sprite FPIndicator;
    [SerializeField] Sprite ACIndicator;
    [SerializeField] Sprite normalIndicator;

    [Header("Chart Status")]
    [SerializeField] Canvas informationCanvas;
    [SerializeField] GameObject failedStatus;
    [SerializeField] GameObject healthLostStatus;
    [SerializeField] GameObject passedStatus;
    [SerializeField] GameObject allComboStatus;
    [SerializeField] GameObject fullPerfectStatus;

    private HashSet<Key> reservedTapKeys;
    private HashSet<Key> reservedBlackKeys;
    private HashSet<Key> pressedKeys = new HashSet<Key>();
    private HashSet<Key> heldKeys = new HashSet<Key>();

    private void OnEnable()
    {
        inputLeftTeleport.action.Enable();
        inputRightTeleport.action.Enable();
        inputSlice.action.Enable();

        inputLeftTeleport.action.performed  += _ => ExecuteInputAllTimingGroups(NoteTypeGeneral.LEFT_TELEPORT);
        inputRightTeleport.action.performed += _ => ExecuteInputAllTimingGroups(NoteTypeGeneral.RIGHT_TELEPORT);
        inputSlice.action.performed         += _ => ExecuteInputAllTimingGroups(NoteTypeGeneral.SLICE_NOTE);
    }

    private void OnDisable()
    {
        inputLeftTeleport.action.performed  -= _ => { };
        inputRightTeleport.action.performed -= _ => { };
        inputSlice.action.performed         -= _ => { };
    }

    private void Awake()
    {
        reservedTapKeys = new HashSet<Key>();
        reservedBlackKeys = new HashSet<Key>();
    }

    private void Start()
    {
        chartSpeed = (double)PlayerPrefs.GetFloat(ValueStorer.prefsChartSpeed, 2.0f);
        currentTiming = (double)PlayerPrefs.GetFloat(ValueStorer.prefsChartOffset, 0.0f);
        isMirrored = PlayerPrefs.GetInt(ValueStorer.prefsIsMirror, 0) == 0 ? false : true;
        RebuildReservedKeys();

        EndSongTransition();
        ImportChart();
        if (isMirrored)
            mirrorText.gameObject.SetActive(true);
        InsertInfo();
        UpdateScoreUI(true);
        UpdateIndicatorUI();

        speedItems = new List<List<SpeedStorer>>();
        StartCoroutine(GetReady());
    }

    void ImportChart()
    {
        songInfo = GameManager.Instance.chosenSongInfo;
        chartFile = GameManager.Instance.chosenChart;
        audioSource.clip = GameManager.Instance.musicClip;

        songName = GameManager.Instance.songName;
        songArtist = GameManager.Instance.songArtist;
        jacketArtRaw = GameManager.Instance.jacketArtRaw;
        level = GameManager.Instance.level;
    }

    void RebuildReservedKeys()
    {
        reservedTapKeys.Clear();
        AddReservedKeys(inputLeftTeleport, ref reservedTapKeys);
        AddReservedKeys(inputRightTeleport, ref reservedTapKeys);
        AddReservedKeys(inputSlice, ref reservedTapKeys);

        reservedBlackKeys.Clear();
        AddReservedKeys(inputLeftTeleport, ref reservedBlackKeys);
        AddReservedKeys(inputRightTeleport, ref reservedBlackKeys);
    }

    void AddReservedKeys(InputAction inputAction, ref HashSet<Key> reservedKeys)
    {
        foreach (var binding in inputAction.bindings)
        {
            if (!binding.effectivePath.StartsWith("<Keyboard>/"))
            {
                continue;
            }

            string keyName = binding.effectivePath.Replace("<Keyboard>/", "");
            if (System.Enum.TryParse(keyName, true, out Key key))
            {
                reservedKeys.Add(key);
            }
        }
    }

    private void Update()
    {
        if (isStarted)
        {
            currentTiming += (Time.deltaTime * 1000f);
            if (currentTiming >= audioSource.clip.length * 1000f)
                StartCoroutine(GoToResultsScreen(false));

            ChangeSpeedThroughTiming(currentTiming);

            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                foreach (var keyControl in keyboard.allKeys)
                {
                    Key finalKey;
                    if (!TryKeyControlToNewKey(keyControl, out finalKey))
                        continue;

                    if (keyControl.wasPressedThisFrame)
                    {
                        pressedKeys.Add(finalKey);
                        if (!reservedTapKeys.Contains(finalKey))
                        {
                            ExecuteInputAllTimingGroups(NoteTypeGeneral.TAP_NOTE);
                        }
                    }
                    else if (keyControl.wasReleasedThisFrame)
                    {
                        pressedKeys.Remove(finalKey);
                    }

                    if (keyControl.isPressed)
                    {
                        heldKeys.Add(finalKey);
                    }
                }

                if (heldKeys.Count > 0)
                {
                    foreach (Key key in heldKeys)
                    {
                        if (!reservedBlackKeys.Contains(key))
                        {
                            ExecuteInputAllTimingGroups(NoteTypeGeneral.BLACK_NOTE);
                            break;
                        }
                    }
                    heldKeys.Clear();
                }
            }
        }
    }

    private float totalScoreCount = 0f;
    private float scoreCountRate = 0f;
    private float countDuration = 0.325f;

    private bool isScoreCount = false;

    private void FixedUpdate()
    {
        if (isScoreCount)
        {
            totalScoreCount += scoreCountRate;
            if (totalScoreCount >= totalScore)
            {
                totalScoreCount = totalScore;
                isScoreCount = false;
            }
            scoreText.text = totalScoreCount.ToString("0.0000") + "%";
        }
    }

    public void StartCountingScore()
    {
        scoreCountRate = (totalScore - totalScoreCount) / (countDuration / Time.fixedDeltaTime);
        isScoreCount = true;
    }

    void ExecuteInputAllTimingGroups(NoteTypeGeneral noteType)
    {
        List<GameObject> notesInFolders = new List<GameObject>();

        for (int i = 0; i < timingGroups.Count; i++)
        {
            GameObject detectedFolder = null;

            if (noteType == NoteTypeGeneral.TAP_NOTE) detectedFolder = timingGroups[i].tapFolder;
            else if (noteType == NoteTypeGeneral.BLACK_NOTE) detectedFolder = timingGroups[i].blackFolder;
            else if (noteType == NoteTypeGeneral.SLICE_NOTE) detectedFolder = timingGroups[i].sliceFolder;
            else if (noteType == NoteTypeGeneral.LEFT_TELEPORT) detectedFolder = timingGroups[i].leftTeleportFolder;
            else if (noteType == NoteTypeGeneral.RIGHT_TELEPORT) detectedFolder = timingGroups[i].rightTeleportFolder;
            else if (noteType == NoteTypeGeneral.SPIKE) detectedFolder = timingGroups[i].spikeFolder;

            foreach (Transform detectedNote in detectedFolder.transform)
            {
                notesInFolders.Add(detectedNote.gameObject);
            }
        }

        ExecuteInput(notesInFolders, noteType == NoteTypeGeneral.BLACK_NOTE);
    }

    bool TryKeyControlToNewKey(KeyControl keyControl, out Key finalKey)
    {
        finalKey = Key.None;

        if (keyControl == null)
        {
            return false;
        }

        finalKey = keyControl.keyCode;
        return finalKey != Key.None;
    }

    void ExecuteInput(List<GameObject> notes, bool isBlackNote)
    {
        if (!isStarted)
            return;

        if (notes.Count == 0)
            return;

        MusicNote lowestNote = null;

        foreach (GameObject note in notes)
        {
            if (!note)
                continue;

            MusicNote musicNote = note.GetComponent<MusicNote>();
            if (!musicNote)
                continue;

            if (!lowestNote)
            {
                lowestNote = musicNote;
                continue;
            }

            if (musicNote.timing < lowestNote.timing)
                lowestNote = musicNote;
        }

        if (!lowestNote)
            return;

        if (isBlackNote)
        {
            List<MusicNote> lowestBlackNotes = new List<MusicNote>();
            foreach (GameObject note in notes)
            {
                MusicNote musicNote = note.GetComponent<MusicNote>();
                if (!musicNote)
                    continue;

                if (musicNote.timing <= lowestNote.timing)
                    lowestBlackNotes.Add(musicNote);
            }
            foreach (MusicNote note in lowestBlackNotes)
            {
                note.ExecuteNote();
            }
            return;
        }

        lowestNote.ExecuteNote();
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
        Vector3 confirmedPosition;
        if (!isMirrored)
            confirmedPosition = new Vector3(xPos, (float)(timing * chartSpeed) / 1000f, 0f);
        else
            confirmedPosition = new Vector3(-xPos, (float)(timing * chartSpeed) / 1000f, 0f);

        if (group > timingGroups.Count - 1)
        {
            AddSpeedGroup();
        }

        switch (noteTypeString)
        {
            case ValueStorer.tapString: confirmedNote = Instantiate(tapNotePrefab, timingGroups[group].tapFolder.gameObject.transform, false) as GameObject; break;
            case ValueStorer.blackString: confirmedNote = Instantiate(blackNotePrefab, timingGroups[group].blackFolder.gameObject.transform, false) as GameObject; break;
            case ValueStorer.leftTeleportString: 
                if (!isMirrored)
                    confirmedNote = Instantiate(leftTeleportPrefab, timingGroups[group].leftTeleportFolder.gameObject.transform, false) as GameObject;
                else
                    confirmedNote = Instantiate(rightTeleportPrefab, timingGroups[group].rightTeleportFolder.gameObject.transform, false) as GameObject;
                break;
            case ValueStorer.rightTeleportString:
                if (!isMirrored)
                    confirmedNote = Instantiate(rightTeleportPrefab, timingGroups[group].rightTeleportFolder.gameObject.transform, false) as GameObject;
                else
                    confirmedNote = Instantiate(leftTeleportPrefab, timingGroups[group].leftTeleportFolder.gameObject.transform, false) as GameObject;
                break;
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

        noteCount += 1;
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

    public void InsertInfo()
    {
        if (songInfo == null)
        {
            return;
        }

        if (songInfo is TextAsset chart)
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

                    if (line.StartsWith(ValueStorer.songNameString))
                    {
                        string songName = line.Substring(ValueStorer.songNameString.Length);
                        songNameText.text = songName;
                        return;
                    }
                }
            }
        }
    }

    void InsertChartInfo(int difficultyIndex)
    {
        player.ChangePosition(LanePosition.MIDDLE_POS);
        this.difficulty = difficultyIndex;

        switch (this.difficulty)
        {
            case 0:
                currentDifficultyText.text = ValueStorer.pointText;
                currentDifficultyText.color = ValueStorer.pointDifficultyColor;
                break;
            case 1:
                currentDifficultyText.text = ValueStorer.lineText;
                currentDifficultyText.color = ValueStorer.lineDifficultyColor;
                break;
            case 2:
                currentDifficultyText.text = ValueStorer.triangleText;
                currentDifficultyText.color = ValueStorer.triangleDifficultyColor;
                break;
            case 3:
                currentDifficultyText.text = ValueStorer.squareText;
                currentDifficultyText.color = ValueStorer.squareDifficultyColor;
                break;
            default: break;
        }

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
                    if (line.StartsWith(ValueStorer.playerPositionString))
                    {
                        string pos = line.Substring(ValueStorer.playerPositionString.Length);
                        if (pos == ValueStorer.playerLeftString)
                        {
                            if (!isMirrored)
                                player.ChangePosition(LanePosition.LEFT_POS);
                            else
                                player.ChangePosition(LanePosition.RIGHT_POS);
                        }
                        else if (pos == ValueStorer.playerMiddleString)
                            player.ChangePosition(LanePosition.MIDDLE_POS);
                        else if (pos == ValueStorer.playerRightString)
                        {
                            if (!isMirrored)
                                player.ChangePosition(LanePosition.RIGHT_POS);
                            else
                                player.ChangePosition(LanePosition.LEFT_POS);
                        }
                    }
                }
            }
        }
    }

    IEnumerator InsertChart()
    {
        switch (this.difficulty)
        {
            case 0: 
                currentDifficultyText.text = ValueStorer.pointText; 
                currentDifficultyText.color = ValueStorer.pointDifficultyColor; 
                break;
            case 1: 
                currentDifficultyText.text = ValueStorer.lineText; 
                currentDifficultyText.color = ValueStorer.lineDifficultyColor; 
                break;
            case 2: 
                currentDifficultyText.text = ValueStorer.triangleText; 
                currentDifficultyText.color = ValueStorer.triangleDifficultyColor; 
                break;
            case 3: 
                currentDifficultyText.text = ValueStorer.squareText; 
                currentDifficultyText.color = ValueStorer.squareDifficultyColor; 
                break;
            default: break;
        }

        speedItems.Clear();

        if (chartFile == null)
        {
            yield break;
        }

        if (chartFile is TextAsset chart)
        {
            using (StringReader reader = new StringReader(chart.text))
            {
                int batchSize = 150;
                int batchCount = 0;

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (batchCount >= batchSize)
                    {
                        batchCount = 0;
                        yield return null;
                    }

                    if (line == string.Empty)
                    {
                        continue;
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
                        {
                            InsertNote(group, ValueStorer.tapString, timing, xPos);
                            batchCount += 1;
                        }
                        continue;
                    }

                    if (line.StartsWith(ValueStorer.blackString))
                    {
                        string[] values = GetConvertedNoteProperties(ValueStorer.blackString, line);
                        if (int.TryParse(values[0], out int group) &&
                            float.TryParse(values[1], out float timing) &&
                            float.TryParse(values[2], out float xPos))
                        {
                            InsertNote(group, ValueStorer.blackString, timing, xPos); 
                            batchCount += 1;
                        }
                        continue;
                    }

                    if (line.StartsWith(ValueStorer.leftTeleportString))
                    {
                        string[] values = GetConvertedNoteProperties(ValueStorer.leftTeleportString, line);
                        if (int.TryParse(values[0], out int group) &&
                            float.TryParse(values[1], out float timing) &&
                            float.TryParse(values[2], out float xPos))
                        {
                            InsertNote(group, ValueStorer.leftTeleportString, timing, xPos); 
                            batchCount += 1;
                        }
                        continue;
                    }

                    if (line.StartsWith(ValueStorer.rightTeleportString))
                    {
                        string[] values = GetConvertedNoteProperties(ValueStorer.rightTeleportString, line);
                        if (int.TryParse(values[0], out int group) &&
                            float.TryParse(values[1], out float timing) &&
                            float.TryParse(values[2], out float xPos))
                        {
                            InsertNote(group, ValueStorer.rightTeleportString, timing, xPos); 
                            batchCount += 1;
                        }
                        continue;
                    }

                    if (line.StartsWith(ValueStorer.sliceString))
                    {
                        string[] values = GetConvertedNoteProperties(ValueStorer.sliceString, line);
                        if (int.TryParse(values[0], out int group) &&
                            float.TryParse(values[1], out float timing))
                        {
                            InsertNote(group, ValueStorer.sliceString, timing); 
                            batchCount += 1;
                        }
                        continue;
                    }

                    if (line.StartsWith(ValueStorer.middleSpikeString))
                    {
                        string[] values = GetConvertedNoteProperties(ValueStorer.middleSpikeString, line);
                        if (int.TryParse(values[0], out int group) &&
                            float.TryParse(values[1], out float timing))
                        {
                            InsertNote(group, ValueStorer.middleSpikeString, timing);
                            batchCount += 1;
                        }
                        continue;
                    }

                    if (line.StartsWith(ValueStorer.sideSpikeString))
                    {
                        string[] values = GetConvertedNoteProperties(ValueStorer.sideSpikeString, line);
                        if (int.TryParse(values[0], out int group) &&
                            float.TryParse(values[1], out float timing))
                        {
                            InsertNote(group, ValueStorer.sideSpikeString, timing); batchCount += 1;
                        }
                        continue;
                    }
                }
            }
        }

        ApplyTimingSpeed();
    }

    public void DeductHealth(float value)
    {
        health -= value;
        if (health <= 0)
        {
            isStarted = false;
            health = 0;
            StartCoroutine(GoToResultsScreen(true));
        }
    }

    public void CalculateScore(bool isMissed = false)
    {
        if (indicatorType == IndicatorType.FAILED)
            return;

        totalScore = 101.0f / (float)noteCount * ((float)CPerfectNotes + (float)perfectNotes * 0.9f + (float)goodNotes * 0.5f);
        UpdateIndicator();

        if (!isMissed) StartCountingScore();
        UpdateScoreUI(isMissed);
        UpdateIndicatorUI();
    }

    void UpdateScoreUI(bool isMissed = false)
    {
        comboText.text = comboCount.ToString();
        comboText.gameObject.SetActive(!isMissed);

        CPerfectText.text = CPerfectNotes.ToString();
        perfectText.text = perfectNotes.ToString();
        goodText.text = goodNotes.ToString();
        damageText.text = damageNotes.ToString();
        missText.text = missNotes.ToString();

        healthBar.localScale = new Vector2(1f, health / 100f);
    }

    void UpdateIndicator()
    {
        if (missNotes + damageNotes == 0)
        {
            if (goodNotes > 0)
            {
                indicatorType = IndicatorType.ALL_COMBO;
            }
            else
            {
                indicatorType = IndicatorType.FULL_PERFECT;
            }
        }
        else
        {
            indicatorType = IndicatorType.NORMAL;
        }
    }

    void UpdateIndicatorUI()
    {
        switch (indicatorType)
        {
            case IndicatorType.ALL_COMBO: indicator.sprite = ACIndicator; break;
            case IndicatorType.FULL_PERFECT: indicator.sprite = FPIndicator; break;
            case IndicatorType.NORMAL: indicator.sprite = normalIndicator; break;
            default: break;
        }
    }

    void EndSongTransition()
    {
        GameManager.Instance.isRhythmStarting = false;

        if (GameManager.Instance.songTransitionCanvas == null)
            return;

        Animator anim = GameManager.Instance.songTransitionCanvas.GetComponent<Animator>();
        anim.Play("Song Transition End");
    }

    IEnumerator GetReady()
    {
        InsertChartInfo(GameManager.Instance.difficultyIndex);
        yield return new WaitForSeconds(2.0f);
        yield return StartCoroutine(InsertChart());
        ChangeSpeedThroughTiming(currentTiming);
        yield return new WaitForSeconds(3.0f);
        audioSource.Play();
        isStarted = true;
    }

    void SelectChartStatus(bool isHealthLost = false)
    {
        if (isHealthLost)
        {
            indicatorType = IndicatorType.FAILED;
            healthLostStatus.SetActive(true);
            return;
        }

        if (totalScore < 80.0f)
        {
            indicatorType = IndicatorType.FAILED;
            failedStatus.SetActive(true);
            return;
        }

        switch (indicatorType)
        {
            case IndicatorType.NORMAL: passedStatus.SetActive(true); break;
            case IndicatorType.ALL_COMBO: allComboStatus.SetActive(true); break;
            case IndicatorType.FULL_PERFECT: fullPerfectStatus.SetActive(true); break;
            default: break;
        }
    }

    IEnumerator GoToResultsScreen(bool isHealthLost)
    {
        isStarted = false;
        audioSource.Stop();
        
        Animator anim = informationCanvas.GetComponent<Animator>();
        if (anim)
        {
            anim.runtimeAnimatorController = closingAnimator;
            anim.enabled = true;
            anim.Play("Song End Animation");
        }

        SelectChartStatus(isHealthLost);
        yield return new WaitForSeconds(5.0f);
        DontDestroyOnLoad(gameObject);
        SceneManager.LoadScene("Result");
    }
}
